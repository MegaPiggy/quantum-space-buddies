﻿using HarmonyLib;

namespace SteamRerouter.ModSide;

[HarmonyPatch]
public static class Patches
{
	[HarmonyPrefix]
	[HarmonyPatch(typeof(SteamEntitlementRetriever), nameof(SteamEntitlementRetriever.GetOwnershipStatus))]
	private static bool SteamEntitlementRetriever_GetOwnershipStatus(out EntitlementsManager.AsyncOwnershipStatus __result)
	{
		__result = Socket.SteamEntitlementRetriever_GetOwnershipStatus();
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Achievements), nameof(Achievements.Earn))]
	private static bool Achievements_Earn(Achievements.Type type)
	{
		Socket.Achievements_Earn(type);
		return false;
	}
}
