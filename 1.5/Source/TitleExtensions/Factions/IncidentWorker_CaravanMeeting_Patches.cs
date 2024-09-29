using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace FCP.Factions;

[HarmonyDebug, HarmonyPatch]
public static class IncidentWorker_CaravanMeeting_Patches
{
    private static MethodBase TargetMethod()
    {
        var nestedTypes = typeof(IncidentWorker_CaravanMeeting).GetNestedTypes(AccessTools.all);
        
        var method = nestedTypes
            .SelectMany(AccessTools.GetDeclaredMethods)
            .FirstOrDefault(mi =>
                mi.ReturnType == typeof(bool) &&
                mi.GetParameters().ContainsAny(pi => pi.ParameterType == typeof(Faction)));

        if (method == null)
            Log.Error("FCPTools IncidentWorker_CaravanMeeting_Patches failed to find the compiler generated nested class method it was targeting");

        return method;
    }

    [HarmonyDebug]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        var factionGetHiddenMethod = typeof(Faction).PropertyGetter(nameof(Faction.Hidden));

        var matcher = new CodeMatcher(instructions, generator)
            .End()
            .MatchStartBackwards( // Find the last return when the branch fails.
                new CodeMatch(OpCodes.Ldc_I4_0),
                new CodeMatch(OpCodes.Ret)
            ) 
            .CreateLabel(out var endLabel)
            .MatchStartBackwards( // Find the use of get_Hidden so we can modify that branch
                CodeMatch.Calls(factionGetHiddenMethod), 
                CodeMatch.Branches()
            )
            .Advance(1)
            .ThrowIfInvalid("FCPTools Transpiler was unable to find the use of Faction.get_Hidden in the CaravanMeeting Nested Method");

        matcher.CreateLabelAt(matcher.Pos + 1, out var nextConditional)
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Brfalse, nextConditional),
                CodeInstruction.LoadArgument(1), // Load the Faction field
                CodeInstruction.Call(typeof(IncidentWorker_CaravanMeeting_Patches), nameof(FactionHasDefModExtension))
            )
            .SetOpcodeAndAdvance(OpCodes.Brfalse);

        return matcher.Instructions();
    }

    private static bool FactionHasDefModExtension(Faction faction)
    {
        return faction.def.HasModExtension<HiddenFactionCaravanExtension>();
    }
}