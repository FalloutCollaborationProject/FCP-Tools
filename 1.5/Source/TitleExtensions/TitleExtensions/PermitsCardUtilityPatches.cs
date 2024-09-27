using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace FCP.TitleExtensions;

// ReSharper disable InconsistentNaming

[HarmonyPatch]
public static class PermitsCardUtilityPatches
{
    
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(PermitsCardUtility), "DoLeftRect")]
    private static IEnumerable<CodeInstruction> PermitsCardUtility_LeftRect_Transpiler(
        IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        // Fields:
        // - PermitsCardUtility
        var f_selectedPermit = AccessTools.Field(typeof(PermitsCardUtility), "selectedPermit");
        // - RoyalTitlePermitDef
        var f_minTitle = AccessTools.Field(typeof(RoyalTitlePermitDef), "minTitle");
        var f_prerequisite = AccessTools.Field(typeof(RoyalTitlePermitDef), "prerequisite");
        
        // Methods
        // - MaxTitlePermisPatches (this)
        var m_appendMaxTitleStatus = AccessTools.Method(typeof(MaxTitlePermitPatches), nameof(AppendMaxTitleStatus));

        // Transpiler Procedure:
        // 1 - Find the minTitle check and the branching point after
        // 2 - Find the next branch which uses permitdef prerequisite and selectedPermit
        // 3 - Create a label to jump back to.
        // 4 - Go back one to the end of the previous branch to check it.
        // 5 - Move back and load the required variables
        // 6 - Run the code and store the result

        // 1 - 4
        var matcher = new CodeMatcher(instructions, generator)
            // 1
            .MatchEndForward(
                CodeMatch.LoadsField(f_minTitle),
                CodeMatch.Branches()
            )
            .ThrowIfInvalid("PermitsCardUtility_LeftRect_Transpiler: FCPTools couldn't find the correct branch (seq 1)")
            // 2
            .MatchStartForward(
                CodeMatch.LoadsField(f_selectedPermit),
                CodeMatch.LoadsField(f_prerequisite)
            )
            .ThrowIfInvalid("PermitsCardUtility_LeftRect_Transpiler: FCPTools couldn't find the second branch (seq 2)")
            // 3
            .CreateLabel(out var prereqCheckLabel)
            // 4
            .Advance(-1) // temporarily move back
            .ThrowIfNotMatch(
                "PermitsCardUtility_LeftRect_Transpiler: instruction prior to end of second branch is not as expected (seq 4)",
                CodeMatch.StoresLocal("storeText")
            );

        // Get the index for storing text
        int textIndex = (matcher.NamedMatch("storeText").operand as LocalBuilder)!.LocalIndex;
        
        // 5 - 6
        matcher.Advance(1) // move back up to the insertion point
            .Insert(
                // 5
                CodeInstruction.LoadLocal(textIndex), // text Field
                CodeInstruction.LoadArgument(1), // Pawn
                // 6
                CodeInstruction.Call(typeof(MaxTitlePermitPatches), nameof(AppendMaxTitleStatus)),
                CodeInstruction.StoreLocal(textIndex)
            );

        return matcher.Instructions();
    }
    // ReSharper restore InconsistentNaming

    /// <summary>
    /// Retrieves the Extension and if nessecary, append the max title text.
    /// </summary>
    private static string AppendMaxTitleStatus(string text, Pawn pawn)
    {
        var permitExtension = PermitsCardUtility.selectedPermit.GetModExtension<MaxTitlePermitExtension>();
        
        if (permitExtension?.maxTitle != null)
        {
            var meetsMaxTitleRequirements = pawn.royalty.GetCurrentTitle(PermitsCardUtility.selectedFaction).seniority 
                                            <= permitExtension.maxTitle.seniority;
            
            return text + "\n" + "Maximum Title: " + permitExtension.maxTitle.GetLabelForBothGenders()
                .Colorize(meetsMaxTitleRequirements ? Color.white : ColorLibrary.RedReadable);
        }
        
        return text;
    }
}