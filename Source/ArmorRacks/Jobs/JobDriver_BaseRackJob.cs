using System.Collections.Generic;
using ArmorRacks.Things;
using RimWorld;
using Verse;
using Verse.AI;

namespace ArmorRacks.Jobs
{
    public abstract class JobDriver_BaseRackJob : JobDriver
    {
        public virtual int WaitTicks { get; }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            return pawn.Reserve(TargetThingA, job, errorOnFailed: errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            var destination = TargetThingA.def.hasInteractionCell ? PathEndMode.InteractionCell : PathEndMode.Touch;
            yield return Toils_Goto.GotoThing(TargetIndex.A, destination);
            yield return Toils_General.WaitWith(TargetIndex.A, WaitTicks, true);
        }
    }
}