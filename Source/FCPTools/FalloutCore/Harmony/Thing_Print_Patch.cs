using HarmonyLib;
using Verse;

namespace FCP.Core;

[HarmonyPatch(typeof(Thing), nameof(Thing.Print))]
public static class Thing_Print_Patch
{
	public static bool Prefix(Thing __instance)
	{
		if (__instance is Buildings.Building_StorageShelf)
			return true;
		
		Building edifice = __instance.Position.GetEdifice(__instance.Map);
		if (edifice is Buildings.Building_StorageShelf)
			return false;
		
		return true;
	}
}
