using HarmonyLib;
// ReSharper disable InconsistentNaming

namespace FCP.Factions;

[HarmonyPatch(typeof(RoyalTitleDefExt))]
public static class RoyalTitleDefExt_Branching_Patches
{
	[HarmonyPatch(nameof(GetNextTitle)), HarmonyPrefix]
	public static bool GetNextTitle_Prefix(RoyalTitleDef currentTitle, Faction faction, ref RoyalTitleDef __result)
	{
		var ext = currentTitle?.GetModExtension<TitleExtension_BranchTitle>();
		if (ext == null) 
			return true;
		
		__result = GetNextTitle(currentTitle, faction, ext);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(RoyalTitleDefExt), nameof(GetPreviousTitle))]
	[HarmonyPatch(typeof(RoyalTitleDefExt), nameof(GetPreviousTitle_IncludeNonRewardable))]
	public static bool GetPreviousTitle_Prefix(RoyalTitleDef currentTitle, Faction faction, ref RoyalTitleDef __result)
	{
		var ext = currentTitle?.GetModExtension<TitleExtension_BranchTitle>();
		if (ext == null) 
			return true;
		
		__result = GetPreviousTitle(currentTitle, faction, ext);
		return false;
	}

	#region Replacement Methods
	
	public static RoyalTitleDef GetNextTitle(RoyalTitleDef currentTitle, Faction faction, TitleExtension_BranchTitle ext)
	{
		List<RoyalTitleDef> awardableTitles = ext.branchDef.GetAwardableTitles(faction.def);
		int index = awardableTitles.IndexOf(currentTitle);

		if (index == -1) 
			return null;
		
		int next = index + 1;
		return awardableTitles.Count <= next ? null : awardableTitles[next];
	}

	public static RoyalTitleDef GetPreviousTitle(RoyalTitleDef currentTitle, Faction faction, TitleExtension_BranchTitle ext)
	{
		if (currentTitle == null)
			return null;

		List<RoyalTitleDef> awardableTitles = ext.branchDef.GetAwardableTitles(faction.def);
		
		int prev = awardableTitles.IndexOf(currentTitle) - 1;
		if (prev >= awardableTitles.Count || prev < 0)
			return null;

		return awardableTitles[prev];
	}

	public static RoyalTitleDef GetPreviousTitle_IncludeNonRewardable(RoyalTitleDef currentTitle, Faction faction, TitleExtension_BranchTitle ext)
	{
		return GetPreviousTitle(currentTitle, faction, ext);
	}

	/*
	 public static bool TryInherit(RoyalTitleDef title, Pawn from, Faction faction, out RoyalTitleInheritanceOutcome outcome)
	{
		outcome = default;

		if (title.GetInheritanceWorker(faction) == null) 
			return false;

		Pawn heir = from.royalty.GetHeir(faction);
		if (heir.DestroyedOrNull())
			return false;

		RoyalTitleDef currentTitle = heir.royalty?.GetCurrentTitle(faction);
		outcome = new RoyalTitleInheritanceOutcome
		{
			heir = heir,
			heirCurrentTitle = currentTitle,
			heirTitleHigher = currentTitle != null && currentTitle.seniority >= title.seniority
		};
		return true;
	}
	*/
	
	#endregion
}