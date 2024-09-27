using System.Reflection.Emit;
using HarmonyLib;

namespace FCP.Factions;

[HarmonyDebug]
[HarmonyPatch(typeof(IncidentWorker_NeutralGroup))]
public static class IncidentWorker_NeutralGroup_Patches
{
    
    [HarmonyDebug]
    [HarmonyTranspiler]
    [HarmonyPatch("FactionCanBeGroupSource")]
    private static IEnumerable<CodeInstruction> FactionCanBeGroupSource_Transpiler(
        IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var factionGetHiddenMethod = typeof(Faction).PropertyGetter(nameof(Faction.Hidden));
        
        
        var matcher = new CodeMatcher(instructions, generator)
            .MatchEndForward(
                CodeMatch.Calls(factionGetHiddenMethod),
                CodeMatch.Branches()
            )
            .ThrowIfInvalid("FCPTools : FactionCanBeGroupSource_Transpiler couldn't find a valid insertion point");
        
        var failurePoint = matcher.Operand;
        
        matcher.CreateLabelAt(matcher.Pos + 1, out var nextCheckPoint)
            .SetAndAdvance(OpCodes.Brfalse, nextCheckPoint)
            .Insert(
                CodeInstruction.LoadArgument(1), // Load Faction
                CodeInstruction.Call(typeof(IncidentWorker_NeutralGroup_Patches), nameof(FactionHasDefModExtension)),
                new CodeInstruction(OpCodes.Brfalse, failurePoint)
            );

        return matcher.Instructions();
    }

    private static bool FactionHasDefModExtension(Faction faction)
    {
        return faction.def.HasModExtension<HiddenFactionCaravanExtension>();
    }
    
}
