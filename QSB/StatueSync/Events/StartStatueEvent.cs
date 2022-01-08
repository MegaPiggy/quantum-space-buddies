﻿using QSB.ClientServerStateSync;
using QSB.Events;
using QSB.Utility;
using UnityEngine;

namespace QSB.StatueSync.Events
{
	internal class StartStatueEvent : QSBEvent<StartStatueMessage>
	{
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener()
			=> GlobalMessenger<Vector3, Quaternion, float>.AddListener(EventNames.QSBStartStatue, Handler);

		public override void CloseListener()
			=> GlobalMessenger<Vector3, Quaternion, float>.RemoveListener(EventNames.QSBStartStatue, Handler);

		private void Handler(Vector3 position, Quaternion rotation, float degrees)
			=> SendEvent(CreateMessage(position, rotation, degrees));

		private StartStatueMessage CreateMessage(Vector3 position, Quaternion rotation, float degrees) => new()
		{
			AboutId = LocalPlayerId,
			PlayerPosition = position,
			PlayerRotation = rotation,
			CameraDegrees = degrees
		};

		public override void OnReceiveLocal(bool server, StartStatueMessage message)
		{
			if (!QSBCore.IsHost)
			{
				return;
			}

			ServerStateManager.Instance.FireChangeServerStateEvent(ServerState.InStatueCutscene);
		}

		public override void OnReceiveRemote(bool server, StartStatueMessage message)
		{
			StatueManager.Instance.BeginSequence(message.PlayerPosition, message.PlayerRotation, message.CameraDegrees);

			if (!QSBCore.IsHost)
			{
				return;
			}

			ServerStateManager.Instance.FireChangeServerStateEvent(ServerState.InStatueCutscene);
		}
	}
}