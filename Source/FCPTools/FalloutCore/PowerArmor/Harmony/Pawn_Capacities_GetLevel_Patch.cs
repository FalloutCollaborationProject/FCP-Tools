using HarmonyLib;

namespace FCP.Core.PowerArmor;

[HarmonyPatch(typeof(PawnCapacitiesHandler), "GetLevel")]
public static class Pawn_Capacities_GetLevel_Patch
{
    public static void Postfix(PawnCapacitiesHandler __instance, PawnCapacityDef capacity, ref float __result, Pawn ___pawn)
    {
        if (capacity != PawnCapacityDefOf.Moving || __result >= 1f)
            return;

        if (___pawn.Dead || ___pawn.apparel == null)
            return;

        List<Apparel> worn = ___pawn.apparel.WornApparel;
        for (int i = 0; i < worn.Count; i++)
        {
            var props = worn[i].def.GetCompProperties<CompProperties_PowerArmor>();
            if (props == null)
                continue;

            if (props.ignoresLegs)
                __result = 1f;
            return;
        }
    }
}
