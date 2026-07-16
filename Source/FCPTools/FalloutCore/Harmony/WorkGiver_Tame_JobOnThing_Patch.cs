using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace FCP.Core;

[HarmonyPatch(typeof(WorkGiver_Tame), "JobOnThing")]
public static class WorkGiver_Tame_JobOnThing_Patch
{
    public static void Postfix(Pawn pawn, Thing t, ref Job __result)
    {
        if (__result == null)
        {
            return;
        }
        if (!t.def.CanUseByXenotype(pawn.genes?.Xenotype))
        {
            __result = null;
        }
    }
}
