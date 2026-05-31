using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace FCP.Core.JobDrivers
{
    public class JobDriver_LoadHolotapeIntoPipboy : JobDriver
    {
        private Thing Holotape => job.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Holotape, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            
            Toil loadToil = new Toil();
            loadToil.initAction = () =>
            {
                if (pawn.apparel == null)
                    return;
                
                Apparel pipboy = pawn.apparel.WornApparel.Find(a => a.TryGetComp<Holotapes.CompPipboyHolotapeStorage>() != null);
                if (pipboy == null)
                    return;
                
                Holotapes.CompPipboyHolotapeStorage storage = pipboy.TryGetComp<Holotapes.CompPipboyHolotapeStorage>();
                if (storage != null && storage.TryStoreHolotape(Holotape))
                    Messages.Message(pawn.LabelShort + " loaded " + Holotape.Label + " into Pip-Boy.", pawn, MessageTypeDefOf.NeutralEvent);
            };
            loadToil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return loadToil;
        }
    }
}
