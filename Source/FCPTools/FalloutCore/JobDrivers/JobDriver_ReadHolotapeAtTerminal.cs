using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace FCP.Core.JobDrivers
{
    public class JobDriver_ReadHolotapeAtTerminal : JobDriver
    {
        private const int ReadTicks = 180;

        private Buildings.Building_Terminal Terminal => (Buildings.Building_Terminal)job.GetTarget(TargetIndex.A).Thing;
        private Thing Holotape => job.GetTarget(TargetIndex.B).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Terminal, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            yield return new Toil
            {
                initAction = delegate
                {
                    var storage = Terminal.GetComp<Buildings.CompHolotapeStorage>();
                    if (storage != null && Holotape != null)
                    {
                        var comp = Holotape.TryGetComp<Holotapes.CompHolotape>();
                        if (comp?.ContentDef != null)
                        {
                            Find.WindowStack.Add(new Holotapes.Dialog_HolotapeReader(comp, storage, pawn));
                        }
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}
