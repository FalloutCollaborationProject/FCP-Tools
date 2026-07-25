using FCP.Currencies;
using Verse.AI;

namespace FCP.Core.Radio;

public class JobDriver_RadioCallCaravan : JobDriver
{
    public int optionIndex = -1;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref optionIndex, "optionIndex", -1);
    }

    public override bool TryMakePreToilReservations(bool errorOnFailed)
        => pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

        Toil call = ToilMaker.MakeToil("MakeNewToils");
        call.initAction = () =>
        {
            CompRadio comp = job.GetTarget(TargetIndex.A).Thing?.TryGetComp<CompRadio>();
            RadioCaravanOption option = comp != null && optionIndex >= 0 && optionIndex < comp.Props.caravanOptions.Count
                ? comp.Props.caravanOptions[optionIndex]
                : null;
            if (option == null)
                return;

            if (option.characterDef != null && !RadioUtility.CanSpawnNamedCaravan(option.characterDef, out string blockedReason))
            {
                if (!blockedReason.NullOrEmpty())
                    Messages.Message(blockedReason, MessageTypeDefOf.RejectInput, false);
                return;
            }

            ThingDef currency = option.currencyDef ?? CurrencyManager.defaultCurrencyDef;
            if (!RadioUtility.TryConsumeCurrency(pawn.Map, currency, option.cost))
                return;

            if (option.characterDef != null)
            {
                RadioUtility.TrySpawnNamedCaravan(pawn.Map, option.characterDef, option.traderKind, out _);
                return;
            }

            Faction faction = option.factionDef != null ? Find.FactionManager.FirstFactionOfDef(option.factionDef) : null;
            IncidentParms parms = new IncidentParms
            {
                target = pawn.Map,
                faction = faction,
                traderKind = option.traderKind
            };
            IncidentDefOf.TraderCaravanArrival.Worker.TryExecute(parms);
        };
        call.defaultCompleteMode = ToilCompleteMode.Instant;
        yield return call;
    }
}
