using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace FCP.Core.Buildings
{
    public class JobDriver_HackTerminal : JobDriver
    {
        private Building_Terminal Terminal => (Building_Terminal)job.targetA.Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Terminal, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            yield return new Toil
            {
                initAction = delegate
                {
                    var hackComp = Terminal.GetComp<CompTerminalHacking>();
                    if (hackComp != null && hackComp.IsLocked && !hackComp.IsLockedOut)
                    {
                        Find.WindowStack.Add(new Window_TerminalHacking(hackComp));
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}
