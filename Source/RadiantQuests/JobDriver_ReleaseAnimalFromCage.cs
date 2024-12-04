using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;

namespace FCP_RadiantQuests
{
    public class JobDriver_ReleaseAnimalFromCage : JobDriver
    {
        private const TargetIndex CasketInd = TargetIndex.A;

        public CompAnimalCage Cage => job.targetA.Thing.TryGetComp<CompAnimalCage>();

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Cage.parent, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            yield return PrepareToReleaseToil(TargetIndex.A);
            Toil enter = ToilMaker.MakeToil("MakeNewToils");
            enter.initAction = delegate
            {
                Cage.EjectContents(Map);
            };
            enter.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return enter;
        }

        public static Toil PrepareToReleaseToil(TargetIndex CageIndex)
        {
            Toil toil = Toils_General.Wait(70);
            toil.FailOnCannotTouch(CageIndex, PathEndMode.InteractionCell);
            toil.WithProgressBarToilDelay(CageIndex);
            return toil;
        }
    }
}
