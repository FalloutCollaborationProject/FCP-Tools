using Verse.AI;

namespace FCP.Core.WeaponCondition;

public class JobDriver_LoadApparelToBench : JobDriver
{
    private Building Bench => (Building)job.GetTarget(TargetIndex.A).Thing;
    private Apparel Apparel => (Apparel)job.GetTarget(TargetIndex.B).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed) =>
        pawn.Reserve(Bench, job, errorOnFailed: errorOnFailed);

    protected override IEnumerable<Toil> MakeNewToils()
    {
        CompApparelBench benchComp = Bench.TryGetComp<CompApparelBench>();
        this.FailOnDestroyedOrNull(TargetIndex.A);
        this.FailOn(() => benchComp.LoadedApparel != null);

        Toil pickUp = ToilMaker.MakeToil();
        pickUp.defaultCompleteMode = ToilCompleteMode.Instant;
        pickUp.initAction = () =>
        {
            Apparel a = Apparel;
            if (a.ParentHolder is Pawn_ApparelTracker tracker)
            {
                tracker.Remove(a);
                pawn.carryTracker.innerContainer.TryAdd(a);
            }
            else if (a.ParentHolder is Pawn_InventoryTracker inv)
            {
                inv.innerContainer.Remove(a);
                pawn.carryTracker.innerContainer.TryAdd(a);
            }
            else if (a.Spawned)
            {
                a.DeSpawn();
                pawn.carryTracker.innerContainer.TryAdd(a);
            }
        };
        yield return pickUp;

        yield return Toils_Goto.GotoCell(Bench.InteractionCell, PathEndMode.OnCell);

        Toil deposit = ToilMaker.MakeToil();
        deposit.defaultCompleteMode = ToilCompleteMode.Instant;
        deposit.initAction = () =>
        {
            Thing carried = pawn.carryTracker.CarriedThing;
            if (carried == null) return;
            pawn.carryTracker.innerContainer.Remove(carried);
            benchComp.Load(carried);
        };
        yield return deposit;
    }
}
