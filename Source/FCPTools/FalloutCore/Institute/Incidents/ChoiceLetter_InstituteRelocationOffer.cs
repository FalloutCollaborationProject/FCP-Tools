using RimWorld.Planet;

namespace FCP.Synths;

public class ChoiceLetter_InstituteRelocationOffer : ChoiceLetter
{
    public Pawn target;
    public Map map;
    public int rewardMarketValue;
    public int goodwillGain;

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

            DiaOption accept = new("FCP_InstituteRelocation_Accept".Translate())
            {
                action = Accept,
                resolveTree = true
            };
            yield return accept;

            yield return Option_Reject;
            yield return Option_Postpone;
        }
    }

    private void Accept()
    {
        Faction institute = relatedFaction;

        if (target.Spawned)
            target.DeSpawn();

        target.GetCaravan()?.RemovePawn(target);
        if (!target.IsWorldPawn())
            Find.WorldPawns.PassToWorld(target, PawnDiscardDecideMode.KeepForever);
        target.SetFaction(institute);

        Map dropMap = map != null && Find.Maps.Contains(map) ? map : Find.AnyPlayerHomeMap;
        LookTargets lookTargets = null;
        if (dropMap != null)
        {
            IntVec3 dropSpot = DropCellFinder.TradeDropSpot(dropMap);
            ThingSetMakerParams rewardParams = default;
            rewardParams.totalMarketValueRange = new FloatRange(rewardMarketValue * 0.8f, rewardMarketValue * 1.2f);
            List<Thing> reward = ThingSetMakerDefOf.Reward_ItemsStandard.root.Generate(rewardParams);
            DropPodUtility.DropThingsNear(dropSpot, dropMap, reward);
            lookTargets = new TargetInfo(dropSpot, dropMap);
        }

        institute.TryAffectGoodwillWith(Faction.OfPlayer, goodwillGain, canSendHostilityLetter: false);

        Find.LetterStack.ReceiveLetter(
            "FCP_InstituteRelocation_ConfirmTitle".Translate(),
            "FCP_InstituteRelocation_ConfirmText".Translate(target.LabelShortCap),
            LetterDefOf.NeutralEvent,
            lookTargets);

        Find.LetterStack.RemoveLetter(this);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref target, "target");
        Scribe_References.Look(ref map, "map");
        Scribe_Values.Look(ref rewardMarketValue, "rewardMarketValue");
        Scribe_Values.Look(ref goodwillGain, "goodwillGain");
    }
}
