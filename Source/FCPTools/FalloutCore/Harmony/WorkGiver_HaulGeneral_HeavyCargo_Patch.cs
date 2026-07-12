using HarmonyLib;

namespace FCP.Core;

[HarmonyPatch(typeof(WorkGiver_Scanner), nameof(WorkGiver_Scanner.HasJobOnThing))]
public static class WorkGiver_HaulGeneral_HeavyCargo_Patch
{
    public static void Postfix(WorkGiver_Scanner __instance, Pawn pawn, Thing t, ref bool __result)
    {
        if (!__result || __instance is not WorkGiver_HaulGeneral) return;

        ModExtension_HeavyCargo ext = t.def.GetModExtension<ModExtension_HeavyCargo>();
        if (ext == null) return;

        if (pawn.GetStatValue(StatDefOf.CarryingCapacity) < ext.minCarryingCapacity)
        {
            __result = false;
        }
    }
}
