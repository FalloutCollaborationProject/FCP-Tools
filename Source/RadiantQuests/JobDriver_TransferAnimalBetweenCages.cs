using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;
using RimWorld;
using UnityEngine;

namespace FCP_RadiantQuests
{
    public class JobDriver_TransferAnimalBetweenCages : JobDriver
    {
        private const TargetIndex SourceHolderIndex = TargetIndex.A;

        private const TargetIndex DestHolderIndex = TargetIndex.B;

        private const TargetIndex TakeeIndex = TargetIndex.C;

        private Pawn Takee => (Pawn)job.GetTarget(TargetIndex.C).Thing;

        private CompAnimalCage SourceHolder => job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompAnimalCage>();

        private CompAnimalCage DestHolder => job.GetTarget(TargetIndex.B).Thing.TryGetComp<CompAnimalCage>();

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.Reserve(Takee, job, 1, -1, null, errorOnFailed) && pawn.Reserve(SourceHolder.parent, job, 1, -1, null, errorOnFailed))
            {
                return pawn.Reserve(DestHolder.parent, job, 1, -1, null, errorOnFailed);
            }
            return false;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.C);
            this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
            this.FailOn(() => !DestHolder.Accepts(Takee));
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_General.Do(delegate
            {
                SourceHolder.EjectContents(SourceHolder.parent.Map);
            }).FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Haul.StartCarryThing(TargetIndex.C);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell);
            yield return PrepareToEnterToil(TargetIndex.B);
            yield return new Toil
            {
                initAction = delegate
                {
                    DestHolder.InsertPawn(Takee);
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
