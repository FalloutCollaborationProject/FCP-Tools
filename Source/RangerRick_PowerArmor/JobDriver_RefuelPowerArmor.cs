using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RangerRick_PowerArmor
{
    public class JobDriver_RefuelPowerArmor : JobDriver
    {
        private const TargetIndex RefuelableInd = TargetIndex.A;

        private const TargetIndex FuelInd = TargetIndex.B;

        public const int RefuelingDuration = 240;

        protected Thing Refuelable => job.GetTarget(TargetIndex.A).Thing;

        protected CompRefuelable RefuelableComp => Refuelable.TryGetComp<CompRefuelable>();

        protected Thing Fuel => job.GetTarget(TargetIndex.B).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.Reserve(Refuelable, job, 1, -1, null, errorOnFailed))
            {
                return pawn.Reserve(Fuel, job, 1, -1, null, errorOnFailed);
            }
            return false;
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.C);
            AddEndCondition(() => (!RefuelableComp.IsFull) ? JobCondition.Ongoing : JobCondition.Succeeded);
            AddFailCondition(() => !job.playerForced && !RefuelableComp.ShouldAutoRefuelNowIgnoringFuelPct);
            AddFailCondition(() => !RefuelableComp.allowAutoRefuel && !job.playerForced);
            yield return Toils_General.DoAtomic(delegate
            {
                job.count = RefuelableComp.GetFuelCountToFullyRefuel();
            });
            Toil reserveFuel = Toils_Reserve.Reserve(TargetIndex.B);
            yield return reserveFuel;
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, putRemainderInQueue: false, subtractNumTakenFromJobCount: true).FailOnDestroyedNullOrForbidden(TargetIndex.B);
            yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveFuel, TargetIndex.B, TargetIndex.None, takeFromValidStorage: true);
            if (pawn != TargetPawnC)
            {
                yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.Touch);
            }
            yield return Toils_General.Wait(240).FailOnDestroyedNullOrForbidden(TargetIndex.B)
                .FailOnDestroyedNullOrForbidden(TargetIndex.C)
                .FailOnCannotTouch(TargetIndex.C, PathEndMode.Touch)
                .WithProgressBarToilDelay(TargetIndex.C);
            yield return Toils_Refuel.FinalizeRefueling(TargetIndex.A, TargetIndex.B);
        }
    }
}
