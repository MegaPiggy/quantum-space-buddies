﻿using Cysharp.Threading.Tasks;
using QSB.Messaging;
using QSB.MeteorSync.Messages;
using QSB.WorldSync;
using System.Threading;
using UnityEngine;

namespace QSB.MeteorSync.WorldObjects;

public class QSBFragment : WorldObject<FragmentIntegrity>
{
	public override async UniTask Init(CancellationToken ct)
	{
		if (QSBCore.IsHost)
		{
			LeashLength = Random.Range(MeteorManager.WhiteHoleVolume._debrisDistMin, MeteorManager.WhiteHoleVolume._debrisDistMax);
		}
	}

	public override void SendInitialState(uint to) =>
		this.SendMessage(new FragmentInitialStateMessage(this) { To = to });

	public void SetIntegrity(float integrity)
	{
		if (OWMath.ApproxEquals(AttachedObject._integrity, integrity))
		{
			return;
		}

		if (AttachedObject._integrity <= 0f)
		{
			return;
		}

		AttachedObject._integrity = integrity;
		AttachedObject.CallOnTakeDamage();
	}

	/// <summary>
	/// what the leash length will be when we eventually detach and fall thru white hole.
	/// <para/>
	/// generated by the server and sent to clients in the initial state message.
	/// </summary>
	public float? LeashLength;
}
