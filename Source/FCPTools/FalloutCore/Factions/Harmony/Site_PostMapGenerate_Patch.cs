using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace FCP.Factions.Harmony;

[HarmonyPatch(typeof(Site), "PostMapGenerate")]
public static class Site_PostMapGenerate_Patch
{
	public static void Postfix(Site __instance)
	{
		if (__instance.Map == null)
			return;

		GameComponent_FactionOutpostTemplates templateComp = Current.Game?.GetComponent<GameComponent_FactionOutpostTemplates>();
		if (templateComp == null)
			return;

		NamedSettlement template = templateComp.GetTemplate(__instance.Tile);
		if (template == null)
			return;

		MapComponent_FactionOutpost mapComp = __instance.Map.GetComponent<MapComponent_FactionOutpost>();
		if (mapComp == null)
		{
			mapComp = new MapComponent_FactionOutpost(__instance.Map);
			__instance.Map.components.Add(mapComp);
		}

		mapComp.prefab = template.prefab;
		mapComp.guaranteedPawnKinds = template.guaranteedPawnKinds;
		mapComp.guaranteedCharacters = template.guaranteedCharacters;
		mapComp.traders = template.traders;

		templateComp.RemoveTemplate(__instance.Tile);
	}
}
