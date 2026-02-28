using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
// ReSharper disable InconsistentNaming

namespace FCP.Factions;

[HarmonyPatch(typeof(Pawn_RoyaltyTracker))]
public static class Pawn_RoyaltyTracker_Branching_Patches
{
	/// <summary>
	/// Replaces the <c>RoyalTitlesAllInSeniorityOrderForReading</c> call in <c>OnFavorChanged</c>
	/// </summary>
	[HarmonyTranspiler, HarmonyPatch("OnFavorChanged")]
	private static IEnumerable<CodeInstruction> OnFavorChanged_Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		MethodInfo targetGetter = AccessTools.PropertyGetter(
			typeof(FactionDef), nameof(FactionDef.RoyalTitlesAllInSeniorityOrderForReading));
		MethodInfo replacement = AccessTools.Method(
			typeof(Pawn_RoyaltyTracker_Branching_Patches), nameof(GetBranchAllTitles));

		foreach (CodeInstruction instruction in instructions)
		{
			if (instruction.Calls(targetGetter))
			{
				// titleAwardedWhenUpdating2 is local 1 — the title for the new favor amount
				yield return new CodeInstruction(OpCodes.Ldloc_1);
				yield return new CodeInstruction(OpCodes.Call, replacement);
			}
			else
			{
				yield return instruction;
			}
		}
	}

	private static List<RoyalTitleDef> GetBranchAllTitles(FactionDef factionDef, RoyalTitleDef newTitle)
	{
		var ext = newTitle?.GetModExtension<TitleExtension_BranchTitle>();
		if (ext == null)
			return factionDef.RoyalTitlesAllInSeniorityOrderForReading;

		TitleBranchDef branchDef = ext.branchDef;
		return factionDef.RoyalTitlesAllInSeniorityOrderForReading
			.Where(def =>
			{
				var tExt = def.GetModExtension<TitleExtension_BranchTitle>();
				return tExt == null || tExt.branchDef == branchDef;
			}).ToList();
	}

	/// <summary>
	/// Replaces calls to <c>RoyalTitlesAwardableInSeniorityOrderForReading</c> with a branch-filtered list
	/// </summary>
	[HarmonyTranspiler, HarmonyPatch(nameof(Pawn_RoyaltyTracker.ApplyRewardsForTitle))]
	private static IEnumerable<CodeInstruction> ApplyRewardsForTitle_Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		MethodInfo targetGetter = AccessTools.PropertyGetter(
			typeof(FactionDef), nameof(FactionDef.RoyalTitlesAwardableInSeniorityOrderForReading));
		MethodInfo replacement = AccessTools.Method(
			typeof(Pawn_RoyaltyTracker_Branching_Patches), nameof(GetBranchAwardableTitles));

		foreach (CodeInstruction instruction in instructions)
		{
			if (instruction.Calls(targetGetter))
			{
				yield return new CodeInstruction(OpCodes.Ldarg_3); // newTitle
				yield return new CodeInstruction(OpCodes.Call, replacement);
			}
			else
			{
				yield return instruction;
			}
		}
	}

	private static List<RoyalTitleDef> GetBranchAwardableTitles(FactionDef factionDef, RoyalTitleDef newTitle)
	{
		var ext = newTitle?.GetModExtension<TitleExtension_BranchTitle>();
		return ext == null
			? factionDef.RoyalTitlesAwardableInSeniorityOrderForReading
			: ext.branchDef.GetAwardableTitles(factionDef);
	}
}
