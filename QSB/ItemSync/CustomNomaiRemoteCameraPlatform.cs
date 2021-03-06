﻿using OWML.Common;
using OWML.Utils;
using QSB.Player;
using QSB.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.ItemSync
{
	class CustomNomaiRemoteCameraPlatform : NomaiShared
	{
		private static List<CustomNomaiRemoteCameraPlatform> s_platforms;
		private static MaterialPropertyBlock s_matPropBlock;
		private static int s_propID_Fade;
		private static int s_propID_HeightMaskScale;
		private static int s_propID_WaveScale;
		private static int s_propID_Ripple2Position;
		private static int s_propID_Ripple2Params;
		public string _dataPointID
		{
			get => _oldPlatform.GetValue<string>("_dataPointID");
			set => _oldPlatform.SetValue("_dataPointID", value);
		}
		public Sector _visualSector
		{
			get => _oldPlatform.GetValue<Sector>("_visualSector");
			set => _oldPlatform.SetValue("_visualSector", value);
		}
		public Sector _visualSector2
		{
			get => _oldPlatform.GetValue<Sector>("_visualSector2");
			set => _oldPlatform.SetValue("_visualSector2", value);
		}
		public Shape _connectionBounds
		{
			get => _oldPlatform.GetValue<Shape>("_connectionBounds");
			set => _oldPlatform.SetValue("_connectionBounds", value);
		}
		public MeshRenderer _poolRenderer
		{
			get => _oldPlatform.GetValue<MeshRenderer>("_poolRenderer");
			set => _oldPlatform.SetValue("_poolRenderer", value);
		}
		public float _poolFillLength
		{
			get => _oldPlatform.GetValue<float>("_poolFillLength");
			set => _oldPlatform.SetValue("_poolFillLength", value);
		}
		public float _poolEmptyLength
		{
			get => _oldPlatform.GetValue<float>("_poolEmptyLength");
			set => _oldPlatform.SetValue("_poolEmptyLength", value);
		}
		public AnimationCurve _poolHeightCurve
		{
			get => _oldPlatform.GetValue<AnimationCurve>("_poolHeightCurve");
			set => _oldPlatform.SetValue("_poolHeightCurve", value);
		}
		public AnimationCurve _poolMaskCurve
		{
			get => _oldPlatform.GetValue<AnimationCurve>("_poolMaskCurve");
			set => _oldPlatform.SetValue("_poolMaskCurve", value);
		}
		public AnimationCurve _poolWaveHeightCurve
		{
			get => _oldPlatform.GetValue<AnimationCurve>("_poolWaveHeightCurve");
			set => _oldPlatform.SetValue("_poolWaveHeightCurve", value);
		}
		public Renderer[] _transitionRenderers
		{
			get => _oldPlatform.GetValue<Renderer[]>("_transitionRenderers");
			set => _oldPlatform.SetValue("_transitionRenderers", value);
		}
		public PedestalAnimator _transitionPedestalAnimator
		{
			get => _oldPlatform.GetValue<PedestalAnimator>("_transitionPedestalAnimator");
			set => _oldPlatform.SetValue("_transitionPedestalAnimator", value);
		}
		public GameObject _transitionStone
		{
			get => _oldPlatform.GetValue<GameObject>("_transitionStone");
			set => _oldPlatform.SetValue("_transitionStone", value);
		}
		public GameObject _hologramGroup
		{
			get => _oldPlatform.GetValue<GameObject>("_hologramGroup");
			set => _oldPlatform.SetValue("_hologramGroup", value);
		}
		public Transform _playerHologram
		{
			get => _oldPlatform.GetValue<Transform>("_playerHologram");
			set => _oldPlatform.SetValue("_playerHologram", value);
		}
		public Transform _stoneHologram
		{
			get => _oldPlatform.GetValue<Transform>("_stoneHologram");
			set => _oldPlatform.SetValue("_stoneHologram", value);
		}
		public float _fadeInLength
		{
			get => _oldPlatform.GetValue<float>("_fadeInLength");
			set => _oldPlatform.SetValue("_fadeInLength", value);
		}
		public float _fadeOutLength
		{
			get => _oldPlatform.GetValue<float>("_fadeOutLength");
			set => _oldPlatform.SetValue("_fadeOutLength", value);
		}
		public OWAudioSource _ambientAudioSource
		{
			get => _oldPlatform.GetValue<OWAudioSource>("_ambientAudioSource");
			set => _oldPlatform.SetValue("_ambientAudioSource", value);
		}
		public OWAudioSource _oneShotAudioSource
		{
			get => _oldPlatform.GetValue<OWAudioSource>("_oneShotAudioSource");
			set => _oldPlatform.SetValue("_oneShotAudioSource", value);
		}
		public DarkZone _darkZone
		{
			get => _oldPlatform.GetValue<DarkZone>("_darkZone");
			set => _oldPlatform.SetValue("_darkZone", value);
		}
		private OWCamera _playerCamera;
		private CustomNomaiRemoteCamera _ownedCamera;
		private SharedStoneSocket _socket;
		private PedestalAnimator _pedestalAnimator;
		private bool _platformActive;
		private float _poolT;
		private bool _showPlayerRipples;
		private Transform _activePlayerHolo;
		private float _transitionFade;
		private CustomNomaiRemoteCameraPlatform _slavePlatform;
		private List<Sector> _alreadyOccupiedSectors;
		private NomaiRemoteCameraPlatform _oldPlatform;
		private bool _peopleStillOnPlatform;
		private bool _wasInBounds;
		private CameraState _cameraState;

		private void Awake()
		{
			_oldPlatform = GetComponent<NomaiRemoteCameraPlatform>();
			_id = _oldPlatform.GetValue<NomaiRemoteCameraPlatform.ID>("_id");
			_sharedStone = _oldPlatform.GetValue<SharedStone>("_sharedStone");
			_ownedCamera = GetComponentInChildren<CustomNomaiRemoteCamera>();
			_alreadyOccupiedSectors = new List<Sector>(16);
			DebugLog.DebugWrite($"{_oldPlatform.name} state to DISCONNECTED");
			_cameraState = CameraState.Disconnected;
			DebugLog.DebugWrite($"{_oldPlatform.name} - active = false");
			_platformActive = false;
			_poolT = 0f;
			_showPlayerRipples = false;
			_activePlayerHolo = null;
			_transitionFade = 0f;
			if (s_matPropBlock == null)
			{
				s_matPropBlock = new MaterialPropertyBlock();
				s_propID_Fade = Shader.PropertyToID("_Fade");
				s_propID_HeightMaskScale = Shader.PropertyToID("_HeightMaskScale");
				s_propID_WaveScale = Shader.PropertyToID("_WaveScale");
				s_propID_Ripple2Position = Shader.PropertyToID("_Ripple2Position");
				s_propID_Ripple2Params = Shader.PropertyToID("_Ripple2Params");
			}
			_socket = GetComponentInChildren<SharedStoneSocket>();
			if (_socket != null)
			{
				_pedestalAnimator = _socket.GetPedestalAnimator();
			}
			else
			{
				Debug.LogWarning("SharedStoneSocket not found!", this);
			}
			UpdatePoolRenderer();
			_hologramGroup.SetActive(false);
			UpdateRendererFade();
			_transitionStone.SetActive(false);
		}

		private void Start()
		{
			if (s_platforms == null)
			{
				s_platforms = new List<CustomNomaiRemoteCameraPlatform>(32);
			}
			s_platforms.Add(this);
			_playerCamera = Locator.GetPlayerCamera();
			if (_socket != null)
			{
				var socket = _socket;
				socket.OnSocketableRemoved += OnSocketableRemoved;
				var socket2 = _socket;
				socket2.OnSocketableDonePlacing += OnSocketableDonePlacing;
			}
			enabled = false;
		}

		private void OnDestroy()
		{
			if (_socket != null)
			{
				var socket = _socket;
				socket.OnSocketableRemoved -= OnSocketableRemoved;
				var socket2 = _socket;
				socket2.OnSocketableDonePlacing -= OnSocketableDonePlacing;
			}
			if (s_platforms != null)
			{
				s_platforms.Remove(this);
			}
		}

		private void Update()
		{
			if (_platformActive)
			{
				var inBounds = _connectionBounds.PointInside(_playerCamera.transform.position);
				_peopleStillOnPlatform = QSBPlayerManager.PlayerList.Any(x => _connectionBounds.PointInside(x.Camera.transform.position));
				if (!inBounds && _wasInBounds)
				{
					OnLeaveBounds();
					_wasInBounds = false;
				}
				else if (inBounds && !_wasInBounds)
				{
					OnEnterBounds();
					_wasInBounds = true;
				}
			}
			if (_platformActive)
			{
				UpdatePools(1f, _poolFillLength);
			}
			else
			{
				UpdatePools(0f, _poolEmptyLength);
			}
			switch (_cameraState)
			{
				case CameraState.WaitingForPedestalContact:
					if (_pedestalAnimator.HasMadeContact())
					{
						_cameraState = CameraState.Disconnected;
						ConnectCamera();
					}
					break;
				case CameraState.Connecting_FadeIn:
					_transitionFade = Mathf.MoveTowards(_transitionFade, 1f, Time.deltaTime / _fadeInLength);
					UpdateRendererFade();
					if (_transitionFade == 1f)
					{
						_slavePlatform._poolT = _poolT;
						_slavePlatform._showPlayerRipples = true;
						_slavePlatform._activePlayerHolo = _playerHologram.GetChild(0);
						_slavePlatform.UpdatePoolRenderer();
						_transitionFade = 0f;
						UpdateRendererFade();
						_slavePlatform._transitionFade = 1f;
						_slavePlatform.UpdateRendererFade();
						SwitchToRemoteCamera();
						_hologramGroup.SetActive(true);
						UpdateHologramTransforms();
						_ambientAudioSource.FadeIn(3f, true, false, 1f);
						Locator.GetAudioMixer().MixRemoteCameraPlatform(_fadeInLength);
						DebugLog.DebugWrite($"{_oldPlatform.name} state to CONNECTING_FADEOUT");
						_cameraState = CameraState.Connecting_FadeOut;
					}
					break;
				case CameraState.Connecting_FadeOut:
					_slavePlatform._transitionFade = Mathf.MoveTowards(_slavePlatform._transitionFade, 0f, Time.deltaTime / _fadeInLength);
					_slavePlatform.UpdateRendererFade();
					UpdateHologramTransforms();
					_slavePlatform._poolT = _poolT;
					_slavePlatform.UpdatePoolRenderer();
					if (_slavePlatform._transitionFade == 0f)
					{
						DebugLog.DebugWrite($"{_oldPlatform.name} state to CONNECTED");
						_cameraState = CameraState.Connected;
					}
					break;
				case CameraState.Connected:
					VerifySectorOccupancy();
					UpdateHologramTransforms();
					_slavePlatform._poolT = _poolT;
					_slavePlatform.UpdatePoolRenderer();
					break;
				case CameraState.Disconnecting_FadeIn:
					_slavePlatform._transitionFade = Mathf.MoveTowards(_slavePlatform._transitionFade, 1f, Time.deltaTime / _fadeOutLength);
					_slavePlatform.UpdateRendererFade();
					UpdateHologramTransforms();
					_slavePlatform._poolT = _poolT;
					_slavePlatform.UpdatePoolRenderer();
					if (_slavePlatform._transitionFade == 1f)
					{
						_slavePlatform._poolT = ((!(_slavePlatform._sharedStone == null)) ? 1f : 0f);
						_slavePlatform._showPlayerRipples = false;
						_slavePlatform._activePlayerHolo = null;
						_slavePlatform.UpdatePoolRenderer();
						_slavePlatform._transitionFade = 0f;
						_slavePlatform.UpdateRendererFade();
						_transitionFade = 1f;
						UpdateRendererFade();
						SwitchToPlayerCamera();
						_hologramGroup.SetActive(false);
						DebugLog.DebugWrite($"{_oldPlatform.name} state to DISCONNECTING_FADEOUT");
						_cameraState = CameraState.Disconnecting_FadeOut;
					}
					break;
				case CameraState.Disconnecting_FadeOut:
					_transitionFade = Mathf.MoveTowards(_transitionFade, 0f, Time.deltaTime / _fadeOutLength);
					UpdateRendererFade();
					if (_transitionFade == 0f)
					{
						DebugLog.DebugWrite($"{_oldPlatform.name} state to DISCONNECTED");
						_cameraState = CameraState.Disconnected;
					}
					break;
			}
			if (_cameraState == CameraState.Disconnected && !_platformActive && _poolT == 0f)
			{
				enabled = false;
			}
		}

		private void UpdatePools(float target, float length)
		{
			_poolT = Mathf.MoveTowards(_poolT, target, Time.deltaTime / length);
			if (OWMath.ApproxEquals(_poolT, target))
			{
				if (_slavePlatform != null && target == 0f)
				{
					DebugLog.DebugWrite($"_poolT reached target 0, setting slave platform to null.");
					_slavePlatform = null;
				}
				return;
			}
			UpdatePoolRenderer();
			_slavePlatform._poolT = target;
			_slavePlatform.UpdatePoolRenderer();
		}

		private void VerifySectorOccupancy()
		{
			if (_slavePlatform._visualSector != null && !_slavePlatform._visualSector.ContainsOccupant(DynamicOccupant.Player))
			{
				DebugLog.ToConsole($"Warning - Player was somehow removed from the {_slavePlatform.name}'s visual sectors!  Re-adding...", MessageType.Warning);
				_slavePlatform._visualSector.AddOccupant(Locator.GetPlayerSectorDetector());
				var parentSector = _slavePlatform._visualSector.GetParentSector();
				while (parentSector != null)
				{
					if (!parentSector.ContainsOccupant(DynamicOccupant.Player))
					{
						parentSector.AddOccupant(Locator.GetPlayerSectorDetector());
					}
					parentSector = parentSector.GetParentSector();
				}
			}
			if (_slavePlatform._visualSector2 != null && !_slavePlatform._visualSector2.ContainsOccupant(DynamicOccupant.Player))
			{
				DebugLog.DebugWrite($"{_oldPlatform.name} re-add player occupant to {_slavePlatform._visualSector2.name}");
				_slavePlatform._visualSector2.AddOccupant(Locator.GetPlayerSectorDetector());
			}
		}


		private void UpdatePoolRenderer()
		{
			_poolRenderer.transform.localPosition = new Vector3(0f, _poolHeightCurve.Evaluate(_poolT), 0f);
			_poolRenderer.material.SetFloat(s_propID_HeightMaskScale, _poolMaskCurve.Evaluate(_poolT));
			var waveScale = _poolRenderer.sharedMaterial.GetVector(s_propID_WaveScale);
			waveScale.y = _poolWaveHeightCurve.Evaluate(_poolT);
			_poolRenderer.material.SetVector(s_propID_WaveScale, waveScale);
			var ripplePosition = _poolRenderer.sharedMaterial.GetVector(s_propID_Ripple2Position);
			var rippleParams = _poolRenderer.sharedMaterial.GetVector(s_propID_Ripple2Params);
			if (_showPlayerRipples)
			{
				var playerPosition = _poolRenderer.transform.InverseTransformPoint(_activePlayerHolo.position);
				ripplePosition.x = playerPosition.x;
				ripplePosition.y = playerPosition.z;
				rippleParams.x = 0.5f;
			}
			_poolRenderer.material.SetVector(s_propID_Ripple2Position, ripplePosition);
			_poolRenderer.material.SetVector(s_propID_Ripple2Params, rippleParams);
		}

		private void UpdateRendererFade()
		{
			s_matPropBlock.SetFloat(s_propID_Fade, 1f - _transitionFade);
			for (var i = 0; i < _transitionRenderers.Length; i++)
			{
				_transitionRenderers[i].SetPropertyBlock(s_matPropBlock);
				if (_transitionRenderers[i].enabled && _transitionFade == 0f)
				{
					_transitionRenderers[i].enabled = false;
				}
				else if (!_transitionRenderers[i].enabled && _transitionFade > 0f)
				{
					_transitionRenderers[i].enabled = true;
				}
			}
			_ownedCamera.SetImageEffectFade(1f - _transitionFade);
		}

		private void UpdateHologramTransforms()
		{
			_hologramGroup.transform.position = _slavePlatform.transform.position;
			_hologramGroup.transform.rotation = _slavePlatform.transform.rotation;
			var playerTransform = Locator.GetPlayerTransform();
			_playerHologram.position = TransformPoint(playerTransform.position, this, _slavePlatform);
			_playerHologram.rotation = TransformRotation(playerTransform.rotation, this, _slavePlatform);
			if (_sharedStone != null)
			{
				var transform = _sharedStone.transform;
				_stoneHologram.position = TransformPoint(transform.position, this, _slavePlatform);
				_stoneHologram.rotation = TransformRotation(transform.rotation, this, _slavePlatform);
			}
			else
			{
				_stoneHologram.SetPositionAndRotation(new Vector3(0f, -2f, 0f), Quaternion.identity);
			}
		}

		private void OnSocketableRemoved(OWItem socketable)
		{
			DebugLog.DebugWrite($"{_oldPlatform.name}  socketable removed");
			if (_slavePlatform == null)
			{
				return;
			}
			DisconnectCamera();
			_transitionStone.SetActive(false);
			_slavePlatform._transitionStone.SetActive(false);
			_socket.GetPedestalAnimator().PlayOpen();
			if (_transitionPedestalAnimator != null)
			{
				_transitionPedestalAnimator.PlayOpen();
			}
			if (_slavePlatform._pedestalAnimator != null)
			{
				_slavePlatform._pedestalAnimator.PlayOpen();
			}
			if (_slavePlatform._transitionPedestalAnimator != null)
			{
				_slavePlatform._transitionPedestalAnimator.PlayOpen();
			}
			_sharedStone = null;
			DebugLog.DebugWrite($"{_oldPlatform.name} - active = false");
			_platformActive = false;
		}

		private void OnSocketableDonePlacing(OWItem socketable)
		{
			DebugLog.DebugWrite($"{_oldPlatform.name} socketable done placing");
			DebugLog.DebugWrite($"{_oldPlatform.name} - active = true");
			_platformActive = true;
			_sharedStone = socketable as SharedStone;
			if (_sharedStone == null)
			{
				Debug.LogError("Placed an empty item or a non SharedStone in a NomaiRemoteCameraPlatform");
			}
			_slavePlatform = GetPlatform(_sharedStone.GetRemoteCameraID());
			if (_slavePlatform == null)
			{
				Debug.LogError("Shared stone with Remote Camera ID: " + _sharedStone.GetRemoteCameraID() + " has no registered camera platform!");
			}
			if (_slavePlatform == this || !_slavePlatform.gameObject.activeInHierarchy)
			{
				DebugLog.DebugWrite($"{_oldPlatform.name} slavePlatform error!");
				_sharedStone = null;
				_slavePlatform = null;
				return;
			}
			_transitionStone.SetActive(true);
			_slavePlatform._transitionStone.SetActive(true);
			_socket.GetPedestalAnimator().PlayClose();
			if (_transitionPedestalAnimator != null)
			{
				_transitionPedestalAnimator.PlayClose();
			}
			if (_slavePlatform._pedestalAnimator != null)
			{
				_slavePlatform._pedestalAnimator.PlayClose();
			}
			if (_slavePlatform._transitionPedestalAnimator != null)
			{
				_slavePlatform._transitionPedestalAnimator.PlayClose();
			}
			enabled = true;
		}

		private void SwitchToRemoteCamera()
		{
			DebugLog.DebugWrite($"{_oldPlatform.name} SwitchToRemoteCamera");
			GlobalMessenger.FireEvent("EnterNomaiRemoteCamera");
			_slavePlatform.RevealFactID();
			_slavePlatform._ownedCamera.Activate(this, _playerCamera);
			_slavePlatform._ownedCamera.SetImageEffectFade(0f);
			_alreadyOccupiedSectors.Clear();
			if (_slavePlatform._visualSector != null)
			{
				if (_visualSector.ContainsOccupant(DynamicOccupant.Player))
				{
					DebugLog.DebugWrite($"{_oldPlatform.name} {_visualSector.name} is already occupied.");
					_alreadyOccupiedSectors.Add(_visualSector);
				}
				DebugLog.DebugWrite($"{_oldPlatform.name} Add player occupant to {_slavePlatform._visualSector.name}");
				_slavePlatform._visualSector.AddOccupant(Locator.GetPlayerSectorDetector());
				var parentSector = _slavePlatform._visualSector.GetParentSector();
				while (parentSector != null)
				{
					if (parentSector.ContainsOccupant(DynamicOccupant.Player))
					{
						DebugLog.DebugWrite($"{_oldPlatform.name} Parent {parentSector} is already occupied.");
						_alreadyOccupiedSectors.Add(parentSector);
					}
					DebugLog.DebugWrite($"{_oldPlatform.name} Add player occupant to parent {parentSector.name}");
					parentSector.AddOccupant(Locator.GetPlayerSectorDetector());
					parentSector = parentSector.GetParentSector();
				}
			}
			if (_slavePlatform._visualSector2 != null)
			{
				DebugLog.DebugWrite($"{_oldPlatform.name} Add player occupant to {_slavePlatform._visualSector2.name}");
				_slavePlatform._visualSector2.AddOccupant(Locator.GetPlayerSectorDetector());
			}
			if (_slavePlatform._darkZone != null)
			{
				DebugLog.DebugWrite($"{_oldPlatform.name} Add player to darkzone {_slavePlatform._darkZone}");
				_slavePlatform._darkZone.AddPlayerToZone(true);
			}
		}

		private void SwitchToPlayerCamera()
		{
			DebugLog.DebugWrite($"{_oldPlatform.name} SwitchToPlayerCamera");
			if (_slavePlatform._visualSector != null)
			{
				if (!_alreadyOccupiedSectors.Contains(_slavePlatform._visualSector))
				{
					_slavePlatform._visualSector.RemoveOccupant(Locator.GetPlayerSectorDetector());
				}
				var parentSector = _slavePlatform._visualSector.GetParentSector();
				while (parentSector != null)
				{
					if (!_alreadyOccupiedSectors.Contains(parentSector))
					{
						parentSector.RemoveOccupant(Locator.GetPlayerSectorDetector());
					}
					parentSector = parentSector.GetParentSector();
				}
			}
			if (_slavePlatform._visualSector2 != null)
			{
				_slavePlatform._visualSector2.RemoveOccupant(Locator.GetPlayerSectorDetector());
			}
			if (_slavePlatform._darkZone != null)
			{
				_slavePlatform._darkZone.RemovePlayerFromZone(true);
			}
			GlobalMessenger.FireEvent("ExitNomaiRemoteCamera");
			_slavePlatform._ownedCamera.Deactivate();
			_slavePlatform._ownedCamera.SetImageEffectFade(0f);
		}

		protected void RevealFactID()
		{
			if (_dataPointID.Length > 0)
			{
				Locator.GetShipLogManager().RevealFact(_dataPointID, true, true);
			}
		}

		private void ConnectCamera()
		{
			DebugLog.DebugWrite($"{_oldPlatform.name} ConnectCamera");
			var cameraState = _cameraState;
			if (cameraState != CameraState.Disconnected && cameraState != CameraState.Disconnecting_FadeOut)
			{
				if (cameraState == CameraState.Disconnecting_FadeIn)
				{
					DebugLog.DebugWrite($"{_oldPlatform.name} state to CONNECTING_FADEOUT");
					_cameraState = CameraState.Connecting_FadeOut;
				}
			}
			else
			{
				DebugLog.DebugWrite($"{_oldPlatform.name} state to CONNECTING_FADEIN");
				_cameraState = CameraState.Connecting_FadeIn;
			}
			_oneShotAudioSource.PlayOneShot(AudioType.NomaiRemoteCameraEntry, 1f);
			enabled = true;
		}

		private void DisconnectCamera()
		{
			DebugLog.DebugWrite($"{_oldPlatform.name} DisconnectCamera");
			var cameraState = this._cameraState;
			if (cameraState != CameraState.Connected && cameraState != CameraState.Connecting_FadeOut)
			{
				if (cameraState == CameraState.Connecting_FadeIn)
				{
					DebugLog.DebugWrite($"{_oldPlatform.name} state to DISCONNECTING_FADEOUT");
					_cameraState = CameraState.Disconnecting_FadeOut;
				}
			}
			else
			{
				DebugLog.DebugWrite($"{_oldPlatform.name} state to DISCONNECTING_FADEIN");
				_cameraState = CameraState.Disconnecting_FadeIn;
				_ambientAudioSource.FadeOut(0.5f, OWAudioSource.FadeOutCompleteAction.STOP, 0f);
				_oneShotAudioSource.PlayOneShot(AudioType.NomaiRemoteCameraExit, 1f);
				Locator.GetAudioMixer().UnmixRemoteCameraPlatform(_fadeOutLength);
			}
		}

		private void OnEnterBounds()
		{
			DebugLog.DebugWrite($"{_oldPlatform.name} OnEnterBounds");
			if (!_platformActive)
			{
				DebugLog.DebugWrite($"{_oldPlatform.name}  - entered while platform is not active, ignore");
				return;
			}
			if (_pedestalAnimator.HasMadeContact())
			{
				ConnectCamera();
			}
			else if (_cameraState == CameraState.Disconnected)
			{
				DebugLog.DebugWrite($"{_oldPlatform.name} state to WAITINGFORPEDASTALCONTACT");
				_cameraState = CameraState.WaitingForPedestalContact;
			}
		}

		private void OnLeaveBounds()
		{
			DebugLog.DebugWrite($"{_oldPlatform.name} OnLeaveBounds");
			DisconnectCamera();
			if (_peopleStillOnPlatform)
			{
				DebugLog.DebugWrite($"{_oldPlatform.name} - people still on platform!");
				return;
			}
			DebugLog.DebugWrite($"{_oldPlatform.name}  - no one left on platform");
			DebugLog.DebugWrite($"{_oldPlatform.name} - active = false");
			_platformActive = false;
			if (_pedestalAnimator != null)
			{
				_pedestalAnimator.PlayOpen();
			}
			if (_transitionPedestalAnimator != null)
			{
				_transitionPedestalAnimator.PlayOpen();
			}
			if (_slavePlatform != null)
			{
				if (_slavePlatform._pedestalAnimator != null)
				{
					_slavePlatform._pedestalAnimator.PlayOpen();
				}
				if (_slavePlatform._transitionPedestalAnimator != null)
				{
					_slavePlatform._transitionPedestalAnimator.PlayOpen();
				}
			}
		}

		public static Vector3 TransformPoint(Vector3 worldPos, CustomNomaiRemoteCameraPlatform from, CustomNomaiRemoteCameraPlatform to)
		{
			var position = from.transform.InverseTransformPoint(worldPos);
			return to.transform.TransformPoint(position);
		}

		public static Quaternion TransformRotation(Quaternion worldRot, CustomNomaiRemoteCameraPlatform from, CustomNomaiRemoteCameraPlatform to)
		{
			var rhs = from.transform.InverseTransformRotation(worldRot);
			return to.transform.rotation * rhs;
		}

		public static CustomNomaiRemoteCameraPlatform GetPlatform(NomaiRemoteCameraPlatform.ID platformID)
		{
			if (s_platforms != null)
			{
				for (var i = 0; i < s_platforms.Count; i++)
				{
					if (s_platforms[i]._id == platformID)
					{
						return s_platforms[i];
					}
				}
			}
			return null;
		}

		public SharedStone GetSocketedStone()
		{
			return _sharedStone;
		}

		public bool IsPlatformActive()
		{
			return _platformActive;
		}

		public enum CameraState
		{
			WaitingForPedestalContact,
			Connected,
			Connecting_FadeIn,
			Connecting_FadeOut,
			Disconnected,
			Disconnecting_FadeIn,
			Disconnecting_FadeOut
		}
	}
}
