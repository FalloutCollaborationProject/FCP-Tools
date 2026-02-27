using Verse.AI;

namespace FCP.Core.PowerArmor;

public class FloatMenuOptionProvider_PowerArmorRefuel : FloatMenuOptionProvider
{
    protected override bool Drafted => false;
    protected override bool Undrafted => true;
    protected override bool Multiselect => false;

    public override bool TargetPawnValid(Pawn pawn, FloatMenuContext context)
    {
        Pawn selPawn = context.FirstSelectedPawn;
        return pawn != selPawn
            && pawn.RaceProps.Humanlike
            && pawn.Faction == selPawn.Faction;
    }

    public override IEnumerable<FloatMenuOption> GetOptionsFor(Pawn clickedPawn, FloatMenuContext context)
    {
        Pawn selPawn = context.FirstSelectedPawn;
        
        if (PowerArmorDefOf.Refuel.Worker is not WorkGiver_Scanner scanner) 
            yield break;

        foreach (Apparel apparel in clickedPawn.apparel.WornApparel)
        {
            if (apparel.GetComp<CompPowerArmor>() == null)
                continue;

            JobFailReason.Clear();

            if (JobGiver_Reload_TryGiveJob_Patch.CanRefuel(selPawn, apparel, forced: true))
            {
                Job job = JobGiver_Reload_TryGiveJob_Patch.RefuelJob(selPawn, apparel, clickedPawn);
                string label = "PrioritizeGeneric".Translate(scanner.PostProcessedGerund(job), apparel.Label).CapitalizeFirst();
                yield return new FloatMenuOption(label, () => selPawn.jobs.TryTakeOrderedJob(job));
            }
            else if (JobFailReason.HaveReason)
            {
                string label = "CannotGenericWork".Translate(scanner.def.verb, apparel.Label) + ": " + JobFailReason.Reason.CapitalizeFirst();
                yield return new FloatMenuOption(label, null);
            }
        }
    }
}
