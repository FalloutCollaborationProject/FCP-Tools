namespace FCP.Railroad;

public class ChoiceLetter_RailroadLiberation : ChoiceLetter
{
    public Pawn target;
    public Map map;

    public override bool CanShowInLetterStack
    {
        get
        {
            if (!base.CanShowInLetterStack)
                return false;

            return target != null && !target.Dead && target.Faction == Faction.OfPlayer;
        }
    }

    public override IEnumerable<DiaOption> Choices
    {
        get
        {
            if (ArchivedOnly)
            {
                yield return Option_Close;
                yield break;
            }

            DiaOption confirm = new("FCP_RailroadLiberation_Confirm".Translate())
            {
                action = Confirm,
                resolveTree = true
            };
            yield return confirm;

            yield return Option_Reject;
            yield return Option_Postpone;
        }
    }

    private void Confirm()
    {
        Find.LetterStack.ReceiveLetter(
            "FCP_RailroadLiberation_RevealTitle".Translate(),
            "FCP_RailroadLiberation_RevealText".Translate(target.LabelShortCap),
            LetterDefOf.PositiveEvent,
            target);

        if (relatedFaction != null)
            relatedFaction.TryAffectGoodwillWith(Faction.OfPlayer, 10, canSendHostilityLetter: false);

        target.needs?.mood?.thoughts?.memories?.TryGainMemory(RailroadDefOf.FCP_Thought_RailroadLiberated);

        Find.LetterStack.RemoveLetter(this);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref target, "target");
        Scribe_References.Look(ref map, "map");
    }
}
