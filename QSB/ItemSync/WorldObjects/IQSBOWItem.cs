﻿using QSB.WorldSync;
using UnityEngine;

namespace QSB.ItemSync.WorldObjects
{
	public interface IQSBOWItem : IWorldObjectTypeSubset
	{
		void DropItem(Vector3 position, Vector3 normal, Sector sector);
	}
}