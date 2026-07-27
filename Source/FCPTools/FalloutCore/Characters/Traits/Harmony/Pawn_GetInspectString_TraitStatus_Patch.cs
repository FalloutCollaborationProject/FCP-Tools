using HarmonyLib;
using Verse;

namespace FCP.Core.Traits;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.GetInspectString))]
public static class Pawn_GetInspectString_TraitStatus_Patch
{
    public static void Postfix(Pawn __instance, ref string __result)
    {
        TraitSet traits = __instance.story?.traits;
        if (traits == null)
            return;

        if (traits.HasTrait(TraitsDefOf.FCP_Trait_Night_Person))
            AppendStatus(ref __result, "Night Person", !TraitConditionUtility.IsDayTime(__instance, 6f, 18f));

        if (traits.HasTrait(TraitsDefOf.FCP_Trait_Solar_Powered))
            AppendStatus(ref __result, "Solar Powered", TraitConditionUtility.IsDayTime(__instance, 6f, 18f) && TraitConditionUtility.IsUnroofed(__instance));

        if (traits.HasTrait(TraitsDefOf.FCP_Trait_Rooted))
            AppendStatus(ref __result, "Rooted", TraitConditionUtility.IsStationary(__instance));

        if (traits.HasTrait(TraitsDefOf.FCP_Trait_Serendipity))
            AppendStatus(ref __result, "Serendipity", TraitConditionUtility.IsLowHealth(__instance, 0.3f));
    }

    private static void AppendStatus(ref string result, string traitLabel, bool active)
    {
        string line = traitLabel + ": " + (active ? "active" : "inactive");
        result = string.IsNullOrEmpty(result) ? line : result + "\n" + line;
    }
}
