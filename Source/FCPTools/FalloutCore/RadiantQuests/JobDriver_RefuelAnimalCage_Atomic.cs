using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;

namespace FCP.Core.RadiantQuests
{
    public class JobDriver_RefuelAnimalCage_Atomic : JobDriver
    {
        private const TargetIndex RefuelableInd = TargetIndex.A;

        private const TargetIndex FuelInd = TargetIndex.B;

        private const TargetIndex FuelPlaceCellInd = TargetIndex.C;
        private const int RefuelingDuration = 240;

        protected Thing Refuelable => job.GetTarget(TargetIndex.A).Thing;

        protected CompAnimalCage RefuelableComp => Refuelable.TryGetComp<CompAnimalCage>();

        protected Thing Fuel => job.GetTarget(TargetIndex.B).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.B), job);
            return pawn.Reserve(Refuelable, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            AddEndCondition(() => (!RefuelableComp.IsFull) ? JobCondition.Ongoing : JobCondition.Succeeded);
            AddFailCondition(() => (!job.playerForced && !RefuelableComp.ShouldAutoRefuelNowIgnoringFuelPct) || !RefuelableComp.allowAutoRefuel);
            AddFailCondition(() => !RefuelableComp.allowAutoRefuel && !job.playerForced);
            yield return Toils_General.DoAtomic(delegate
            {
                job.count = RefuelableComp.GetFuelCountToFullyRefuel();
            });
            Toil getNextIngredient = Toils_General.Label();
            yield return getNextIngredient;
            yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, putRemainderInQueue: false, subtractNumTakenFromJobCount: true).FailOnDestroyedNullOrForbidden(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil findPlaceTarget = Toils_JobTransforms.SetTargetToIngredientPlaceCell(TargetIndex.A, TargetIndex.B, TargetIndex.C);
            yield return findPlaceTarget;
            yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, findPlaceTarget, storageMode: false);
            yield return Toils_Jump.JumpIf(getNextIngredient, () => !job.GetTargetQueue(TargetIndex.B).NullOrEmpty());
            yield return Toils_General.Wait(240).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch)
                .WithProgressBarToilDelay(TargetIndex.A);
            yield return Toils_Refuel.FinalizeRefueling(TargetIndex.A, TargetIndex.None);
        }
    }
}

