using HarmonyLib;
using Verse;
// ReSharper disable InconsistentNaming

namespace FCP.Factions;

[HarmonyPatch(typeof(RoyalTitleUtility))]
public static class RoyalTitleUtility_Branching_Patches
{
    [HarmonyPrefix, HarmonyPatch(nameof(RoyalTitleUtility.GetTitleProgressionInfo))]
    public static bool GetTitleProgressionInfo_Prefix(Faction faction, Pawn pawn, ref string __result)
    {
        List<RoyalTitleDef> awardableTitles = faction.def.RoyalTitlesAwardableInSeniorityOrderForReading;

        // Check if any awardable title has branching
        bool hasBranching = false;
        foreach (RoyalTitleDef title in awardableTitles)
        {
            if (title.GetModExtension<TitleExtension_BranchTitle>() != null)
            {
                hasBranching = true;
                break;
            }
        }

        if (!hasBranching)
            return true; // use the original, then.
        
        List<RoyalTitleDef> unbranchedTitles = [];
        Dictionary<TitleBranchDef, List<RoyalTitleDef>> branchedTitles = [];

        foreach (RoyalTitleDef title in awardableTitles)
        {
            var ext = title.GetModExtension<TitleExtension_BranchTitle>();
            if (ext == null)
            {
                unbranchedTitles.Add(title);
            }
            else
            {
                // titles with the extension get sorted, keyed by branch.
                // ensure each branch gets its own list created on first encounter and is added to it afterwards
                if (!branchedTitles.TryGetValue(ext.branchDef, out List<RoyalTitleDef> list))
                {
                    list = [];
                    branchedTitles[ext.branchDef] = list;
                }
                list.Add(title);
            }
        }

        TaggedString result = "RoyalTitleTooltipTitlesEarnable".Translate(faction.Named("FACTION")) + ":";

        // Shared progression
        int sharedCost = 0;
        foreach (RoyalTitleDef title in unbranchedTitles)
        {
            sharedCost += title.favorCost;
            result += FormatTitleLine(title, pawn, sharedCost, faction);
        }

        // Per-branch progression
        foreach ((TitleBranchDef branch, List<RoyalTitleDef> titles) in branchedTitles)
        {
            result += "\n\n  " + branch.LabelCap + ":";
            int branchCost = sharedCost;
            foreach (RoyalTitleDef title in titles)
            {
                branchCost += title.favorCost;
                result += FormatTitleLine(title, pawn, branchCost, faction);
            }
        }

        // Non-awardable titles, filtered to only show branchless or matching branches
        List<RoyalTitleDef> nonAwardable = faction.def.RoyalTitlesAllInSeniorityOrderForReading
            .Where(def => !def.Awardable)
            .ToList();

        result += "\n\n" + "RoyalTitleTooltipTitlesNonEarnable".Translate(faction.Named("FACTION")) + ":";
        foreach (RoyalTitleDef title in nonAwardable)
        {
            result += "\n  - " + title.GetLabelCapForBothGenders();
        }

        __result = result.Resolve();
        return false;
    }

    private static string FormatTitleLine(RoyalTitleDef title, Pawn pawn, int totalCost, Faction faction)
    {
        string label = pawn != null ? title.GetLabelCapFor(pawn) : title.GetLabelCapForBothGenders();
        return "\n  - " + label + ": "
            + "RoyalTitleTooltipRoyalFavorAmount".Translate(title.favorCost, faction.def.royalFavorLabel)
            + " (" + "RoyalTitleTooltipRoyalFavorTotal".Translate(totalCost.ToString()) + ")";
    }
}
