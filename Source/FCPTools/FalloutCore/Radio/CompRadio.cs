using FCP.Currencies;
using FCP.Enlist;
using UnityEngine;
using Verse.AI;

namespace FCP.Core.Radio;

public class CompRadio : ThingComp
{
    private int lastGatherIntelTick = -999999;

    public CompProperties_Radio Props => (CompProperties_Radio)props;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref lastGatherIntelTick, "lastGatherIntelTick", -999999);
    }

    public void Notify_GatherIntelUsed()
    {
        lastGatherIntelTick = Find.TickManager.TicksGame;
    }

    public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
    {
        if (!selPawn.CanReserveAndReach(parent, PathEndMode.InteractionCell, Danger.Some))
        {
            yield return new FloatMenuOption("FCP_Radio_Use".Translate() + " (" + "NoPath".Translate() + ")", null);
            yield break;
        }

        foreach (FloatMenuOption option in TuneOptions(selPawn))
            yield return option;

        foreach (FloatMenuOption option in CaravanOptions(selPawn))
            yield return option;

        foreach (FloatMenuOption option in ReinforcementOptions(selPawn))
            yield return option;

        yield return GatherIntelOption(selPawn);
    }

    private IEnumerable<FloatMenuOption> TuneOptions(Pawn selPawn)
    {
        if (Props.stations.NullOrEmpty())
            yield break;

        yield return new FloatMenuOption("FCP_Radio_TuneStation".Translate(), () =>
        {
            selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(RadioDefOf.FCP_RadioTuneStation, parent), JobTag.Misc);
        });
    }

    private IEnumerable<FloatMenuOption> CaravanOptions(Pawn selPawn)
    {
        if (Props.caravanOptions.NullOrEmpty())
            yield break;

        for (int i = 0; i < Props.caravanOptions.Count; i++)
        {
            RadioCaravanOption option = Props.caravanOptions[i];
            ThingDef currency = option.currencyDef ?? CurrencyManager.defaultCurrencyDef;
            string label = "FCP_Radio_CallCaravan".Translate(option.label, option.cost, currency.label);

            if (option.characterDef != null && !RadioUtility.CanSpawnNamedCaravan(option.characterDef, out string blockedReason))
            {
                yield return new FloatMenuOption(label + " (" + blockedReason + ")", null);
                continue;
            }

            if (RadioUtility.CountOnMap(selPawn.Map, currency) < option.cost)
            {
                yield return new FloatMenuOption(label + " (" + "NotEnoughSilver".Translate() + ")", null);
                continue;
            }

            int chosenIndex = i;
            yield return new FloatMenuOption(label, () =>
            {
                Job job = JobMaker.MakeJob(RadioDefOf.FCP_RadioCallCaravan, parent);
                if (selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc) && selPawn.jobs.curDriver is JobDriver_RadioCallCaravan driver)
                    driver.optionIndex = chosenIndex;
            });
        }
    }

    private IEnumerable<FloatMenuOption> ReinforcementOptions(Pawn selPawn)
    {
        if (selPawn.royalty == null)
            yield break;

        foreach (var group in RadioUtility.EnlistedFactionsWithOptions().GroupBy(x => x.faction))
        {
            var (faction, options) = group.First();
            int goodwill = faction.GoodwillWith(Faction.OfPlayer);
            if (goodwill < Props.minGoodwillToCallReinforcements)
                continue;

            RoyalTitlePermitDef permit = RoyalTitlePermitWorker_CallReinforcements.FindPermitFor(faction);
            if (permit == null)
                continue;

            string label = "FCP_Radio_CallReinforcements".Translate(faction.Name);
            if (!selPawn.royalty.HasPermit(permit, faction) || selPawn.royalty.GetFavor(faction) < permit.royalAid.favorCost)
            {
                yield return new FloatMenuOption(label + " (" + "CommandCallRoyalAidFavorOption".Translate(0f, permit.royalAid.favorCost, faction.Named("FACTION")) + ")", null);
                continue;
            }

            Faction targetFaction = faction;
            FactionEnlistOptionsDef targetOptions = options;
            yield return new FloatMenuOption(label, () =>
            {
                Find.Targeter.BeginTargeting(new TargetingParameters
                {
                    canTargetLocations = true,
                    canTargetPawns = false,
                    canTargetBuildings = false,
                    canTargetItems = false,
                    validator = t => t.Cell.InBounds(selPawn.Map) && t.Cell.Standable(selPawn.Map)
                }, target =>
                {
                    Job job = JobMaker.MakeJob(RadioDefOf.FCP_RadioCallReinforcements, parent, target.Cell);
                    if (selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc) && selPawn.jobs.curDriver is JobDriver_RadioCallReinforcements driver)
                    {
                        driver.targetFaction = targetFaction;
                        driver.enlistOptions = targetOptions;
                        driver.permit = permit;
                    }
                }, selPawn);
            });
        }
    }

    private FloatMenuOption GatherIntelOption(Pawn selPawn)
    {
        int ticksSince = Find.TickManager.TicksGame - lastGatherIntelTick;
        if (ticksSince < Props.gatherIntelCooldownTicks)
        {
            int daysLeft = Mathf.CeilToInt((Props.gatherIntelCooldownTicks - ticksSince) / (float)GenDate.TicksPerDay);
            return new FloatMenuOption("FCP_Radio_GatherIntel".Translate() + " (" + "FCP_Radio_OnCooldown".Translate(daysLeft) + ")", null);
        }

        return new FloatMenuOption("FCP_Radio_GatherIntel".Translate(), () =>
        {
            selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(RadioDefOf.FCP_RadioGatherIntel, parent), JobTag.Misc);
        });
    }
}
