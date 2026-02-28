using System.Reflection;
using HarmonyLib;

namespace FCP.Factions;

[HarmonyPatch(typeof(FactionUIUtility), "DrawFactionRow")]
public static class FactionUIUtility_DrawFactionRow_Patch
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        MethodInfo labelCapGetter = AccessTools.PropertyGetter(typeof(Def), nameof(Def.LabelCap));

        foreach (CodeInstruction instruction in instructions)
        {
            // Both use FactionDef from the Stack
            if (instruction.Calls(labelCapGetter))
            {
                yield return CodeInstruction.Call(typeof(FactionUIUtility_DrawFactionRow_Patch), nameof(GetLabel));
            }
            else
            {
                yield return instruction;
            }
        }
    }

    private static TaggedString GetLabel(FactionDef def)
    {
        TaggedString label = FactionExtension_FlavorOverride.TryGetLabel(def) ?? def.LabelCap;
        return label;
    }
}