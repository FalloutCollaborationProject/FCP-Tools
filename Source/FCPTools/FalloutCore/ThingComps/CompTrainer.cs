// Decompiled with JetBrains decompiler
// Type: RangerRick_PowerArmor.CompTrainer
// Assembly: RangerRick_PowerArmor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2F9A23E8-63EA-4090-93FB-DECA3711786E
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\FCP-Tools\1.5\Assemblies\RangerRick_PowerArmor.dll

using FCP.Core;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace FCP.Core
{
    public class CompTrainer : ThingComp
    {
        public CompPowerTrader compPower;

        public CompProperties_Trainer Props => props as CompProperties_Trainer;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compPower = parent.GetComp<CompPowerTrader>();
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            CompTrainer compTrainer = this;
            AcceptanceReport acceptanceReport = compTrainer.CanAcceptPawn(selPawn);
            TaggedString label = "RR.TrainTrait".Translate((NamedArgument)compTrainer.Props.givesTrait.degreeDatas[0].label);
            if (acceptanceReport.Accepted)
                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption((string)label, () => selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(RR_DefOf.RR_TrainTrait, (LocalTargetInfo)parent))), selPawn, (LocalTargetInfo)compTrainer.parent);
            else if (!acceptanceReport.Reason.NullOrEmpty())
                yield return new FloatMenuOption((string)(label + ": " + acceptanceReport.Reason.CapitalizeFirst()), null);
        }

        public AcceptanceReport CanAcceptPawn(Pawn pawn)
        {
            if (!compPower.PowerOn)
                return (AcceptanceReport)"NoPower".Translate();
            return pawn.story.traits.GetTrait(Props.givesTrait) != null ? (AcceptanceReport)"RR.TrainTraitAlreadyHasIt".Translate() : (AcceptanceReport)true;
        }
    }
}
