using System.Reflection;
using HarmonyLib;

namespace FCP.Core;

[HarmonyPatchCategory(FCPCoreMod.LatePatchesCategory)]
[HarmonyPatch(typeof(PawnApparelGenerator), "GenerateStartingApparelFor")]
public static class PawnApparelGenerator_GenerateStartingApparelFor_Patch
{
    private static List<ThingStuffPair> removedPairs = new List<ThingStuffPair>();
    private static FieldInfo allApparelPairsField;

    public static void Prefix(Pawn pawn)
    {
        if (UniqueCharactersTracker.Instance != null && UniqueCharactersTracker.Instance.IsUniquePawn(pawn))
        {
            FCPLog.Warning($"GenerateStartingApparelFor running on unique pawn {pawn} (this destroys and regenerates all current apparel)");
        }

        var xenotype = pawn.genes?.Xenotype;
        if (xenotype == null)
            return;

        allApparelPairsField ??= typeof(PawnApparelGenerator).GetField("allApparelPairs", BindingFlags.NonPublic | BindingFlags.Static);
        var allApparelPairs = (List<ThingStuffPair>)allApparelPairsField.GetValue(null);
        removedPairs = allApparelPairs.Where(x => !x.thing.CanUseByXenotype(xenotype)).ToList();
        allApparelPairs.RemoveAll(x => removedPairs.Contains(x));
    }

    public static void Finalizer()
    {
        if (removedPairs.Count == 0)
            return;

        var allApparelPairs = (List<ThingStuffPair>)allApparelPairsField.GetValue(null);
        allApparelPairs.AddRange(removedPairs);
        removedPairs.Clear();
    }
}
