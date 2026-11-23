using FCP.Core;
using HarmonyLib;
using RimWorld.Planet;

namespace FCP.Synths;

[HarmonyPatchCategory(FCPCoreMod.InstitutePatchesCategory)]
[HarmonyPatch(typeof(CaravanEnterMapUtility), nameof(CaravanEnterMapUtility.Enter),
    new[] { typeof(Caravan), typeof(Map), typeof(CaravanEnterMode), typeof(CaravanDropInventoryMode), typeof(bool), typeof(Predicate<IntVec3>) })]
internal static class Patch_CaravanEnterMap_EnterMode
{
    private static void Prefix(Caravan caravan, out List<Pawn> __state)
    {
        __state = caravan?.PawnsListForReading?.ToList();
    }

    private static void Postfix(Map map, List<Pawn> __state)
    {
        SynthReplacementUtility.TryReplaceReturningPawns(__state, map);
    }
}

[HarmonyPatchCategory(FCPCoreMod.InstitutePatchesCategory)]
[HarmonyPatch(typeof(CaravanEnterMapUtility), nameof(CaravanEnterMapUtility.Enter),
    new[] { typeof(Caravan), typeof(Map), typeof(Func<Pawn, IntVec3>), typeof(CaravanDropInventoryMode), typeof(bool) })]
internal static class Patch_CaravanEnterMap_SpawnCellGetter
{
    private static void Prefix(Caravan caravan, out List<Pawn> __state)
    {
        __state = caravan?.PawnsListForReading?.ToList();
    }

    private static void Postfix(Map map, List<Pawn> __state)
    {
        SynthReplacementUtility.TryReplaceReturningPawns(__state, map);
    }
}

[HarmonyPatchCategory(FCPCoreMod.InstitutePatchesCategory)]
[HarmonyPatch(typeof(FCP.Ranks.GameComponent_Expeditions), "Return",
    new[] { typeof(Pawn), typeof(FCP.Ranks.ExpeditionData) })]
internal static class Patch_ExpeditionReturn
{
    private static void Postfix(Pawn pawn, bool __result)
    {
        if (__result)
            SynthReplacementUtility.TryReplace(pawn);
    }
}
