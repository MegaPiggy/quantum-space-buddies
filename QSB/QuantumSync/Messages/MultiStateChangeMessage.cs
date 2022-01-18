﻿using Mirror;
using OWML.Common;
using QSB.Messaging;
using QSB.QuantumSync.WorldObjects;
using QSB.Utility;

namespace QSB.QuantumSync.Messages
{
	internal class MultiStateChangeMessage : QSBWorldObjectMessage<QSBMultiStateQuantumObject>
	{
		private int StateIndex;

		public MultiStateChangeMessage(int stateIndex) => StateIndex = stateIndex;

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(StateIndex);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			StateIndex = reader.Read<int>();
		}

		public override void OnReceiveRemote()
		{
			if (WorldObject.ControllingPlayer != From)
			{
				DebugLog.ToConsole($"Error - Got MultiStateChangeEvent for {WorldObject.Name} from {From}, but it's currently controlled by {WorldObject.ControllingPlayer}!", MessageType.Error);
				return;
			}

			WorldObject.ChangeState(StateIndex);
		}
	}
}