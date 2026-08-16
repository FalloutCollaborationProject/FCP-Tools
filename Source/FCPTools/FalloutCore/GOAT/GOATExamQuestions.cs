namespace FCP.Core.GOAT;

public static class GOATExamQuestions
{
    public static readonly List<string> Questions = new List<string>
    {
        "Raiders are climbing over the wall. What do you do?",
        "A wounded trader collapses at the gate. What's your move?",
        "The water pump seizes up in the middle of the night. How do you handle it?",
        "Rations are running low. Where do you look first?",
        "You find a stack of pre-war paintings in the ruins. What do you do with them?",
        "Something is moving out past the wire at dusk. How do you respond?",
        "The colony wants to build something new. What's your role?"
    };

    public static List<(string text, SkillDef skill)> AnswersFor(int index)
    {
        return index switch
        {
            0 => new List<(string, SkillDef)>
            {
                ("Grab a rifle and start shooting.", SkillDefOf.Shooting),
                ("Charge them with a blade.", SkillDefOf.Melee),
                ("Rig a trap out of scrap and wire.", SkillDefOf.Crafting)
            },
            1 => new List<(string, SkillDef)>
            {
                ("Patch the wound yourself.", SkillDefOf.Medicine),
                ("Talk them through the panic.", SkillDefOf.Social),
                ("Ask what happened, purely for the story.", SkillDefOf.Intellectual)
            },
            2 => new List<(string, SkillDef)>
            {
                ("Tear it down and rebuild it properly.", SkillDefOf.Construction),
                ("Dig a new well by hand.", SkillDefOf.Mining),
                ("Jury-rig something that'll hold until morning.", SkillDefOf.Crafting)
            },
            3 => new List<(string, SkillDef)>
            {
                ("Check on the crops.", SkillDefOf.Plants),
                ("See what the animals can spare.", SkillDefOf.Animals),
                ("Stretch what's left into something edible.", SkillDefOf.Cooking)
            },
            4 => new List<(string, SkillDef)>
            {
                ("Figure out what they're worth to the right buyer.", SkillDefOf.Social),
                ("Try painting something like it yourself.", SkillDefOf.Artistic),
                ("Wonder how they even made it.", SkillDefOf.Intellectual)
            },
            5 => new List<(string, SkillDef)>
            {
                ("Get a rifle sighted on it.", SkillDefOf.Shooting),
                ("Grab a knife and go take a look.", SkillDefOf.Melee),
                ("Watch and wait. No need to be first.", SkillDefOf.Intellectual)
            },
            6 => new List<(string, SkillDef)>
            {
                ("Draft it properly this time.", SkillDefOf.Construction),
                ("Just start building and adjust as you go.", SkillDefOf.Crafting),
                ("Argue about whether it's worth doing at all.", SkillDefOf.Social)
            },
            _ => new List<(string, SkillDef)>()
        };
    }
}
