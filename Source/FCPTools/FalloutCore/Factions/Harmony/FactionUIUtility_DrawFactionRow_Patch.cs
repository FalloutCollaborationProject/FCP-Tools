using HarmonyLib;

namespace FCP.Factions;

[HarmonyPatch(typeof(FactionUIUtility), "DrawFactionRow")]
public static class FactionUIUtility_DrawFactionRow_Patch
{
    // Infix: only the faction.def.LabelCap read inside DrawFactionRow is changed;
    [HarmonyInfix(typeof(Def), nameof(Def.LabelCap), InnerTargetKind.Getter)]
    [HarmonyPostfix]
    public static void LabelCap_Postfix(Def __instance, ref TaggedString __result)
    {
        if (__instance is FactionDef factionDef && FactionExtension_FlavorOverride.TryGetLabel(factionDef) is { } label)
            __result = label;
    }
}