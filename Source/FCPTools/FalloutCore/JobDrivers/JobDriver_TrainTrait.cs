// Decompiled with JetBrains decompiler
// Type: RangerRick_PowerArmor.JobDriver_TrainTrait
// Assembly: RangerRick_PowerArmor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2F9A23E8-63EA-4090-93FB-DECA3711786E
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\FCP-Tools\1.5\Assemblies\RangerRick_PowerArmor.dll

using FCP.Core;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

#nullable disable
namespace FCP.Core
{
    public class JobDriver_TrainTrait : JobDriver
    {
        private int duration;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref duration, "duration");
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetA, job, errorOnFailed: errorOnFailed);
        }

        public override void Notify_Starting()
        {
            base.Notify_Starting();
            duration = TargetA.Thing.TryGetComp<CompTrainer>().Props.trainDuration;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnBurningImmobile(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDespawnedNullOrForbidden(TargetIndex.A);
            Toil toil = ToilMaker.MakeToil("MakeNewToils");
            toil.WithProgressBarToilDelay(TargetIndex.A);
            toil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = duration;
            yield return toil;
            yield return Toils_General.Do(delegate
            {
                pawn.story.traits.GainTrait(new Trait(base.TargetA.Thing.TryGetComp<CompTrainer>().Props.givesTrait));
            });
        }
    }
}
