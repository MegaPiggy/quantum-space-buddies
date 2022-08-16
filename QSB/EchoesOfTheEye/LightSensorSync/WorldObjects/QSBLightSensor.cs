﻿using Cysharp.Threading.Tasks;
using QSB.AuthoritySync;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Threading;

/*
 * For those who come here,
 * leave while you still can.
 */

namespace QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;

/// <summary>
/// TODO: switch this over to some sort of auth system.
/// list of illuminators doesn't work because if a player illuminates and then leaves,
/// it'll be considered illuminated forever until they come back.
///
/// BUG: this breaks in zone2.
/// the sector it's enabled in is bigger than the sector the zone2 walls are enabled in :(
/// maybe this can be fixed by making the collision group use the same sector.
/// </summary>
internal class QSBLightSensor : AuthWorldObject<SingleLightSensor>
{
	internal bool _locallyIlluminated;

	public Action OnDetectLocalLight;
	public Action OnDetectLocalDarkness;


	public override bool CanOwn => AttachedObject.enabled;

	public override void SendInitialState(uint to)
	{
		base.SendInitialState(to);
		// todo initial state
	}

	public override async UniTask Init(CancellationToken ct)
	{
		await base.Init(ct);

		// do this stuff here instead of Start, since world objects won't be ready by that point
		Delay.RunWhen(() => QSBWorldSync.AllObjectsReady, () =>
		{
			if (AttachedObject._sector != null)
			{
				if (AttachedObject._startIlluminated)
				{
					_locallyIlluminated = true;
					OnDetectLocalLight?.Invoke();
				}
			}
		});
	}
}
