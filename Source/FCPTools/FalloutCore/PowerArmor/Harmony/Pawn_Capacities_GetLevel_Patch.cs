using HarmonyLib;
using UnityEngine;

namespace FCP.Core.PowerArmor;

[HarmonyPatch(typeof(PawnCapacitiesHandler), "GetLevel")]
public static class Pawn_Capacities_GetLevel_Patch
{
    public static void Postfix(PawnCapacitiesHandler __instance, PawnCapacityDef capacity, ref float __result, Pawn ___pawn)
    {
        if (capacity != PawnCapacityDefOf.Moving) 
            return;
        
        Apparel apparel = ___pawn.apparel?.WornApparel.FirstOrDefault(a => a.TryGetComp<CompPowerArmor>() != null);
        if (apparel == null) 
            return;
        
        var compPowerArmor = apparel.GetComp<CompPowerArmor>();
        if (compPowerArmor.Props.ignoresLegs) 
            __result = Mathf.Max(__result, 1f);
    }
}