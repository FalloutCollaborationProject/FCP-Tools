using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace FCP.Core.JobDrivers
{
    public class JobDriver_InsertHolotape : JobDriver
    {
        private Thing Holotape => job.GetTarget(TargetIndex.A).Thing;
        private Buildings.Building_Terminal Terminal => (Buildings.Building_Terminal)job.GetTarget(TargetIndex.B).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Holotape, job, 1, -1, null, errorOnFailed) &&
                   pawn.Reserve(Terminal, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDestroyedOrNull(TargetIndex.B);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(TargetIndex.A);

            yield return Toils_Haul.StartCarryThing(TargetIndex.A);

            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell);

            yield return Toils_General.Wait(120).WithProgressBarToilDelay(TargetIndex.B);

            yield return new Toil
            {
                initAction = delegate
                {
                    var storage = Terminal.GetComp<Buildings.CompHolotapeStorage>();
                    if (storage != null && Holotape != null)
                    {
                        if (storage.TryStoreHolotape(Holotape))
                        {
                            Messages.Message($"{pawn.LabelShort} inserted {Holotape.Label} into terminal.", 
                                Terminal, MessageTypeDefOf.TaskCompletion);
                        }
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}
