using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace FCP.Core.Buildings
{
    public class JobDriver_ExtractHolotape : JobDriver
    {
        private Building_Terminal Terminal => (Building_Terminal)job.targetA.Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Terminal, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            var extract = new Toil
            {
                initAction = delegate
                {
                    ticksLeftThisToil = 300;
                },
                tickAction = delegate
                {
                    pawn.rotationTracker.FaceTarget(Terminal);
                },
                defaultCompleteMode = ToilCompleteMode.Delay
            };
            extract.WithProgressBarToilDelay(TargetIndex.A);
            extract.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            yield return extract;

            yield return new Toil
            {
                initAction = delegate
                {
                    Terminal.TryExtractHolotape(pawn);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}