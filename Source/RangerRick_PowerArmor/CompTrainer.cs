namespace RangerRick_PowerArmor
{
    public class CompProperties_Trainer : CompProperties
    {
        public TraitDef givesTrait;
        public int trainDuration;
        public CompProperties_Trainer()
        {
            this.compClass = typeof(CompTrainer);
        }
    }

    public class CompTrainer : ThingComp
    {
        public CompProperties_Trainer Props => base.props as CompProperties_Trainer;

        public CompPowerTrader compPower;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compPower = parent.GetComp<CompPowerTrader>();
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            AcceptanceReport acceptanceReport = CanAcceptPawn(selPawn);
            var label = "RR.TrainTrait".Translate(Props.givesTrait.degreeDatas[0].label);
            if (acceptanceReport.Accepted)
            {
                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, delegate
                {
                    selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(RR_DefOf.RR_TrainTrait, parent));
                }), selPawn, parent);
            }
            else if (!acceptanceReport.Reason.NullOrEmpty())
            {
                yield return new FloatMenuOption(label + ": " + acceptanceReport.Reason.CapitalizeFirst(), null);
            }
        }

        public AcceptanceReport CanAcceptPawn(Pawn pawn)
        {
            if (!compPower.PowerOn)
            {
                return "NoPower".Translate();
            }
            if (pawn.story.traits.GetTrait(Props.givesTrait) != null)
            {
                return "RR.TrainTraitAlreadyHasIt".Translate();
            }
            return true;
        }
    }
}
