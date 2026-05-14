using Verse.AI;

namespace FCP.Core.WeaponCondition;

public class JobDriver_LoadWeaponToBench : JobDriver
{
    private Building Bench => (Building)job.GetTarget(TargetIndex.A).Thing;
    private ThingWithComps Weapon => (ThingWithComps)job.GetTarget(TargetIndex.B).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed) =>
        pawn.Reserve(Bench, job, errorOnFailed: errorOnFailed);

    protected override IEnumerable<Toil> MakeNewToils()
    {
        CompWeaponBench benchComp = Bench.TryGetComp<CompWeaponBench>();
        this.FailOnDestroyedOrNull(TargetIndex.A);
        this.FailOn(() => benchComp.LoadedWeapon != null);

        Toil pickUp = ToilMaker.MakeToil("PickUp");
        pickUp.initAction = () =>
        {
            ThingWithComps w = Weapon;
            if (w.ParentHolder is Pawn_EquipmentTracker eq)
            {
                eq.GetDirectlyHeldThings().Remove(w);
                pawn.carryTracker.innerContainer.TryAdd(w);
            }
            else if (w.ParentHolder is Pawn_InventoryTracker inv)
            {
                inv.innerContainer.Remove(w);
                pawn.carryTracker.innerContainer.TryAdd(w);
            }
            else if (w.Spawned)
            {
                w.DeSpawn();
                pawn.carryTracker.innerContainer.TryAdd(w);
            }
        };
        pickUp.defaultCompleteMode = ToilCompleteMode.Instant;
        yield return pickUp;

        yield return Toils_Goto.GotoCell(Bench.InteractionCell, PathEndMode.OnCell);

        Toil deposit = ToilMaker.MakeToil("Deposit");
        deposit.initAction = () =>
        {
            Thing carried = pawn.carryTracker.CarriedThing;
            if (carried == null) return;
            pawn.carryTracker.innerContainer.Remove(carried);
            benchComp.Load(carried);
        };
        deposit.defaultCompleteMode = ToilCompleteMode.Instant;
        yield return deposit;
    }
}
