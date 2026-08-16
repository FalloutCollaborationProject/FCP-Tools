namespace FCP.Unity;

public class RitualOutcomeEffectWorker_CommunionOfUnity : RitualOutcomeEffectWorker_FromQuality
{
    public RitualOutcomeEffectWorker_CommunionOfUnity()
    {
    }

    public RitualOutcomeEffectWorker_CommunionOfUnity(RitualOutcomeEffectDef def) : base(def)
    {
    }

    protected override void ApplyExtraOutcome(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
    {
        extraOutcomeDesc = null;

        XenotypeDef childOfCathedral = DefDatabase<XenotypeDef>.GetNamedSilentFail("FCP_Xenotype_ChildOfCathedral");
        if (childOfCathedral == null)
        {
            return;
        }

        List<Pawn> eligible = totalPresence.Keys
            .Where(pawn => pawn.RaceProps.Humanlike && pawn.genes != null && pawn.genes.Xenotype == XenotypeDefOf.Baseliner)
            .ToList();

        if (eligible.Count == 0)
        {
            return;
        }

        Pawn convert = eligible.RandomElement();
        GeneUtility.UpdateXenogermReplication(convert);
        convert.genes.SetXenotype(childOfCathedral);

        extraOutcomeDesc = "FCP_CommunionConvert".Translate(convert.LabelShortCap);
        letterLookTargets = new LookTargets(convert);
    }
}
