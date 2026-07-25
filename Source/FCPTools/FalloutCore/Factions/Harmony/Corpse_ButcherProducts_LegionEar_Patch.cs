using System.Collections.Generic;
using HarmonyLib;
// ReSharper disable InconsistentNaming

namespace FCP.Factions;

[HarmonyPatch(typeof(Corpse), nameof(Corpse.ButcherProducts))]
public static class Corpse_ButcherProducts_LegionEar_Patch
{
    private const int EarsPerCorpse = 2;

    private static bool lookedUp;
    private static FactionDef legionFactionDef;
    private static ThingDef earDef;

    public static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, Corpse __instance)
    {
        foreach (Thing product in __result)
        {
            yield return product;
        }

        if (!lookedUp)
        {
            legionFactionDef = DefDatabase<FactionDef>.GetNamedSilentFail("FCP_Faction_Caesars_Legion");
            earDef = DefDatabase<ThingDef>.GetNamedSilentFail("FCP_Item_Legion_Ear");
            lookedUp = true;
        }

        if (legionFactionDef == null || earDef == null)
        {
            yield break;
        }

        Pawn innerPawn = __instance.InnerPawn;
        if (innerPawn?.Faction?.def != legionFactionDef || !innerPawn.RaceProps.Humanlike)
        {
            yield break;
        }

        Thing ears = ThingMaker.MakeThing(earDef);
        ears.stackCount = EarsPerCorpse;
        yield return ears;
    }
}
