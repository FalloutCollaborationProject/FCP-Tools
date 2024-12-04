using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;
using RimWorld;

namespace FCP_RadiantQuests
{
    public class JobDriver_CarryAnimalToCage : JobDriver
    {
        private const TargetIndex TakeeInd = TargetIndex.A;

        private const TargetIndex CasketInd = TargetIndex.B;

        private Pawn Takee => job.GetTarget(TargetIndex.A).Pawn;

        private CompAnimalCage Pod => job.GetTarget(TargetIndex.B).Thing.TryGetComp<CompAnimalCage>();

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Takee, job, 1, -1, null, errorOnFailed) && pawn.Reserve(Pod.parent, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDestroyedOrNull(TargetIndex.B);
            this.FailOnAggroMentalState(TargetIndex.A);
            Toil goToTakee = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnDespawnedNullOrForbidden(TargetIndex.B)
                .FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            Toil startCarryingTakee = Toils_Haul.StartCarryThing(TargetIndex.A);
            Toil goToThing = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell);
            yield return Toils_Jump.JumpIf(goToThing, () => pawn.IsCarryingPawn(Takee));
            yield return goToTakee;
            yield return startCarryingTakee;
            yield return goToThing;
            yield return PrepareToEnterToil(TargetIndex.B);
            yield return new Toil
            {
                initAction = delegate
                {
                    if (Pod.Occupant == null)
                    {
                        Pod.InsertPawn(Takee);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }

        public static Toil PrepareToEnterToil(TargetIndex podIndex)
        {
            Toil toil = Toils_General.Wait(200);
            toil.FailOnCannotTouch(podIndex, PathEndMode.InteractionCell);
            toil.WithProgressBarToilDelay(podIndex);
            return toil;
        }
    }
}
