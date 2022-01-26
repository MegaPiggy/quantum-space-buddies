﻿using QSB.ConversationSync.Messages;
using QSB.LogSync.Messages;
using QSB.Messaging;
using QSB.Utility;

namespace QSB.WorldSync
{
	public class RequestInitialStatesMessage : QSBMessage
	{
		public RequestInitialStatesMessage()
		{
			DebugLog.DebugWrite("getting initial states");
		}

		public override void OnReceiveRemote()
		{
			QSBCore.UnityEvents.RunWhen(() => QSBWorldSync.AllObjectsReady,
				() => SendInitialStates(From));
		}

		private static void SendInitialStates(uint to)
		{
			DebugLog.DebugWrite($"sending initial states to {to}");

			if (QSBCore.IsHost)
			{
				QSBWorldSync.DialogueConditions.ForEach(condition
					=> new DialogueConditionMessage(condition.Key, condition.Value) { To = to }.Send());

				QSBWorldSync.ShipLogFacts.ForEach(fact
					=> new RevealFactMessage(fact.Id, fact.SaveGame, false) { To = to }.Send());
			}

			foreach (var worldObject in QSBWorldSync.GetWorldObjects())
			{
				worldObject.SendInitialState(to);
			}
		}
	}
}
