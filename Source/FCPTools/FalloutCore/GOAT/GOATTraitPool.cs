namespace FCP.Core.GOAT;

public static class GOATTraitPool
{
    private static readonly Dictionary<SkillDef, string[]> CandidateDefNames = new Dictionary<SkillDef, string[]>
    {
        { SkillDefOf.Shooting, new[] { "FCP_Trait_Trigger_Discipline", "FCP_Trait_Quick_Draw", "FCP_Trait_Loose_Cannon" } },
        { SkillDefOf.Melee, new[] { "FCP_Trait_Heavy_Handed", "FCP_Trait_Finesse", "FCP_Trait_Iron_Fist" } },
        { SkillDefOf.Construction, new[] { "FCP_Trait_Skilled", "FCP_Trait_Bruiser" } },
        { SkillDefOf.Mining, new[] { "FCP_Trait_Scrapper", "FCP_Trait_Bruiser" } },
        { SkillDefOf.Cooking, new[] { "FCP_Trait_Lead_Belly", "FCP_Trait_Good_Natured" } },
        { SkillDefOf.Plants, new[] { "FCP_Trait_Scrounger", "FCP_Trait_Rad_Resistant" } },
        { SkillDefOf.Animals, new[] { "FCP_Trait_Animal_Friend", "FCP_Trait_Rad_Resistant" } },
        { SkillDefOf.Crafting, new[] { "FCP_Trait_JuryRigging", "FCP_Trait_Built_To_Destroy", "FCP_Trait_Scrapper" } },
        { SkillDefOf.Artistic, new[] { "FCP_Trait_Sex_Appeal", "FCP_Trait_Good_Natured" } },
        { SkillDefOf.Medicine, new[] { "FCP_Trait_Good_Natured", "FCP_Trait_Lead_Belly" } },
        { SkillDefOf.Social, new[] { "FCP_Trait_Sex_Appeal", "FCP_Trait_Intimidation", "FCP_Trait_Fortune_Finder" } },
        { SkillDefOf.Intellectual, new[] { "FCP_Trait_Infiltrator", "FCP_Trait_Skilled", "FCP_Trait_Swift_Learner" } },
    };

    public static TraitDef TryGrantTrait(Pawn pawn, SkillDef resultSkill)
    {
        if (!CandidateDefNames.TryGetValue(resultSkill, out string[] defNames))
        {
            return null;
        }

        List<TraitDef> candidates = defNames
            .Select(DefDatabase<TraitDef>.GetNamedSilentFail)
            .Where(def => def != null)
            .Where(def => !pawn.story.traits.HasTrait(def))
            .Where(def => !pawn.story.traits.allTraits.Any(def.ConflictsWith))
            .Where(def => !def.ConflictsWithPassion(resultSkill))
            .ToList();

        if (candidates.Count == 0)
        {
            return null;
        }

        TraitDef chosen = candidates.RandomElement();
        pawn.story.traits.GainTrait(new Trait(chosen, 0));
        return chosen;
    }

    public static SkillDef TryDowngradePassionElsewhere(Pawn pawn, SkillDef excludeSkill)
    {
        List<SkillRecord> candidates = pawn.skills.skills
            .Where(record => record.def != excludeSkill && record.passion > Passion.None)
            .ToList();

        if (candidates.Count == 0)
        {
            return null;
        }

        SkillRecord chosen = candidates.RandomElement();
        chosen.passion = (Passion)((int)chosen.passion - 1);
        return chosen.def;
    }
}
