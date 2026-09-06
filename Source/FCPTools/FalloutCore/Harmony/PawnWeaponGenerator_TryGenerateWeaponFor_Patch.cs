using System.Reflection;
using HarmonyLib;

namespace FCP.Core;

[HarmonyPatchCategory(FCPCoreMod.LatePatchesCategory)]
[HarmonyPatch(typeof(PawnWeaponGenerator), "TryGenerateWeaponFor")]
public static class PawnWeaponGenerator_TryGenerateWeaponFor_Patch
{
    private static List<ThingStuffPair> removedPairs = new List<ThingStuffPair>();
    private static FieldInfo allWeaponPairsField;

    public static void Prefix(Pawn pawn)
    {
        var xenotype = pawn.genes?.Xenotype;
        if (xenotype == null)
            return;

        allWeaponPairsField ??= typeof(PawnWeaponGenerator).GetField("allWeaponPairs", BindingFlags.NonPublic | BindingFlags.Static);
        var allWeaponPairs = (List<ThingStuffPair>)allWeaponPairsField.GetValue(null);
        removedPairs = allWeaponPairs.Where(x => !x.thing.CanUseByXenotype(xenotype)).ToList();
        allWeaponPairs.RemoveAll(x => removedPairs.Contains(x));
    }

    public static void Finalizer()
    {
        if (removedPairs.Count == 0)
            return;

        var allWeaponPairs = (List<ThingStuffPair>)allWeaponPairsField.GetValue(null);
        allWeaponPairs.AddRange(removedPairs);
        removedPairs.Clear();
    }
}
