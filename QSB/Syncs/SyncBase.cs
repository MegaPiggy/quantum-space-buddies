﻿using OWML.Common;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET.Components;
using System;
using System.Linq;
using UnityEngine;

namespace QSB.Syncs
{
	/*
	 * Rewrite number : 9
	 * God has cursed me for my hubris, and my work is never finished.
	 */

	public abstract class SyncBase<T> : QNetworkTransform where T: Component
	{
		public uint AttachedNetId
		{
			get
			{
				if (NetIdentity == null)
				{
					DebugLog.ToConsole($"Error - Trying to get AttachedNetId with null NetIdentity! Type:{GetType().Name} GrandType:{GetType().GetType().Name}", MessageType.Error);
					return uint.MaxValue;
				}

				return NetIdentity.NetId.Value;
			}
		}

		public uint PlayerId
		{
			get
			{
				if (!IsPlayerObject)
				{
					return uint.MaxValue;
				}

				if (NetIdentity == null)
				{
					DebugLog.ToConsole($"Error - Trying to get PlayerId with null NetIdentity! Type:{GetType().Name} GrandType:{GetType().GetType().Name}", MessageType.Error);
					return uint.MaxValue;
				}

				return NetIdentity.RootIdentity != null
					? NetIdentity.RootIdentity.NetId.Value
					: AttachedNetId;
			}
		}

		public PlayerInfo Player => QSBPlayerManager.GetPlayer(PlayerId);

		private bool _baseIsReady
		{
			get
			{
				if (NetId.Value is uint.MaxValue or 0U)
				{
					return false;
				}

				if (!WorldObjectManager.AllObjectsAdded)
				{
					return false;
				}

				if (IsPlayerObject)
				{
					if (!QSBPlayerManager.PlayerExists(PlayerId))
					{
						return false;
					}

					if (Player == null)
					{
						return false;
					}

					if (!Player.IsReady && !IsLocalPlayer)
					{
						return false;
					}
				}

				return true;
			}
		}

		public abstract bool IsReady { get; }
		public abstract bool UseInterpolation { get; }
		public abstract bool IgnoreDisabledAttachedObject { get; }
		public abstract bool IgnoreNullReferenceTransform { get; }
		public abstract bool DestroyAttachedObject { get; }
		public abstract bool IsPlayerObject { get;  }

		public T AttachedObject { get; set; }
		public Transform ReferenceTransform { get; set; }

		public string LogName => $"{PlayerId}.{NetId.Value}:{GetType().Name}";
		protected virtual float DistanceLeeway { get; } = 5f;
		private float _previousDistance;
		protected const float SmoothTime = 0.1f;
		private Vector3 _positionSmoothVelocity;
		private Quaternion _rotationSmoothVelocity;
		protected bool _isInitialized;

		protected abstract T SetAttachedObject();
		protected abstract bool UpdateTransform();

		public virtual void Start()
		{
			if (IsPlayerObject)
			{
				var lowestBound = QSBWorldSync.GetUnityObjects<PlayerTransformSync>()
				.Where(x => x.NetId.Value <= NetId.Value).OrderBy(x => x.NetId.Value).Last();
				NetIdentity.SetRootIdentity(lowestBound.NetIdentity);
			}

			DontDestroyOnLoad(gameObject);
			QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
		}

		protected virtual void OnDestroy()
		{
			if (DestroyAttachedObject)
			{
				if (!HasAuthority && AttachedObject != null)
				{
					Destroy(AttachedObject.gameObject);
				}
			}

			QSBSceneManager.OnSceneLoaded -= OnSceneLoaded;
		}

		protected virtual void Init()
		{
			if (!QSBSceneManager.IsInUniverse)
			{
				DebugLog.ToConsole($"Error - {LogName} is being init-ed when not in the universe!", MessageType.Error);
			}

			// TODO : maybe make it's own option
			if (DestroyAttachedObject)
			{
				if (!HasAuthority && AttachedObject != null)
				{
					Destroy(AttachedObject.gameObject);
				}
			}

			AttachedObject = SetAttachedObject();
			_isInitialized = true;
		}

		protected virtual void OnSceneLoaded(OWScene oldScene, OWScene newScene, bool isInUniverse) => _isInitialized = false;

		public override void Update()
		{
			if (!_isInitialized && IsReady && _baseIsReady)
			{
				try
				{
					Init();
				}
				catch (Exception ex)
				{
					DebugLog.ToConsole($"Exception when initializing {name} : {ex}", MessageType.Error);
					enabled = false;
				}

				base.Update();
				return;
			}
			else if (_isInitialized && (!IsReady || !_baseIsReady))
			{
				_isInitialized = false;
				base.Update();
				return;
			}

			if (!_isInitialized)
			{
				base.Update();
				return;
			}

			if (AttachedObject == null)
			{
				DebugLog.ToConsole($"Warning - AttachedObject {LogName} is null.", MessageType.Warning);
				_isInitialized = false;
				base.Update();
				return;
			}

			if (ReferenceTransform != null && ReferenceTransform.position == Vector3.zero)
			{
				DebugLog.ToConsole($"Warning - {LogName}'s ReferenceTransform is at (0,0,0). ReferenceTransform:{ReferenceTransform.name}, AttachedObject:{AttachedObject.name}", MessageType.Warning);
			}

			if (!AttachedObject.gameObject.activeInHierarchy && !IgnoreDisabledAttachedObject)
			{
				base.Update();
				return;
			}

			if (ReferenceTransform == null && !IgnoreNullReferenceTransform)
			{
				DebugLog.ToConsole($"Warning - {LogName}'s ReferenceTransform is null. AttachedObject:{AttachedObject.name}", MessageType.Warning);
				base.Update();
				return;
			}

			UpdateTransform();

			base.Update();
		}

		protected Vector3 SmartSmoothDamp(Vector3 currentPosition, Vector3 targetPosition)
		{
			var distance = Vector3.Distance(currentPosition, targetPosition);
			if (distance > _previousDistance + DistanceLeeway)
			{
				/*
				DebugLog.DebugWrite($"{_logName} moved too far!" +
					$"\r\n CurrentPosition:{currentPosition}," +
					$"\r\n TargetPosition:{targetPosition}");
				*/
				_previousDistance = distance;
				return targetPosition;
			}

			_previousDistance = distance;
			return Vector3.SmoothDamp(currentPosition, targetPosition, ref _positionSmoothVelocity, SmoothTime);
		}

		protected Quaternion SmartSmoothDamp(Quaternion currentRotation, Quaternion targetRotation)
		{
			return QuaternionHelper.SmoothDamp(currentRotation, targetRotation, ref _rotationSmoothVelocity, SmoothTime);
		}

		public void SetReferenceTransform(Transform referenceTransform)
			=> ReferenceTransform = referenceTransform;

		protected virtual void OnRenderObject()
		{
			if (!WorldObjectManager.AllObjectsReady
				|| !QSBCore.ShowLinesInDebug
				|| !IsReady
				|| ReferenceTransform == null)
			{
				return;
			}

			/* Red Cube = Where visible object should be
			 * Green cube = Where visible object is
			 * Magenta cube = Reference transform
			 * Red Line = Connection between Red Cube and Green Cube
			 * Cyan Line = Connection between Green cube and reference transform
			 */

			Popcron.Gizmos.Cube(ReferenceTransform.FromRelPos(transform.position), ReferenceTransform.FromRelRot(transform.rotation), Vector3.one / 8, Color.red);
			Popcron.Gizmos.Line(ReferenceTransform.FromRelPos(transform.position), AttachedObject.transform.position, Color.red);
			Popcron.Gizmos.Cube(AttachedObject.transform.position, AttachedObject.transform.rotation, Vector3.one / 6, Color.green);
			Popcron.Gizmos.Cube(ReferenceTransform.position, ReferenceTransform.rotation, Vector3.one / 8, Color.magenta);
			Popcron.Gizmos.Line(AttachedObject.transform.position, ReferenceTransform.position, Color.cyan);
		}
	}
}
