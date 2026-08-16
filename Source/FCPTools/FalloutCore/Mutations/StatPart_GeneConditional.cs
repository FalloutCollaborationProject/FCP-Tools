namespace FCP.Mutations;

public class StatPart_GeneConditional : StatPart
{
    public GeneDef gene;
    public float offset;
    public GeneConditionType condition;
    public float healthThreshold = 0.3f;

    public override void TransformValue(StatRequest req, ref float val)
    {
        if (Applies(req))
            val += offset;
    }

    public override string ExplanationPart(StatRequest req)
    {
        if (!Applies(req))
            return null;
        return gene.label.CapitalizeFirst() + ": " + offset.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Offset);
    }

    private bool Applies(StatRequest req)
    {
        if (req.Thing is not Pawn pawn || pawn.Dead || pawn.genes == null)
            return false;
        if (!pawn.genes.HasActiveGene(gene))
            return false;

        return condition switch
        {
            GeneConditionType.LowHealth => pawn.health.summaryHealth.SummaryHealthPercent < healthThreshold,
            GeneConditionType.StationaryUnarmored => IsStationary(pawn) && IsUnarmored(pawn),
            GeneConditionType.Grouped => HasAllyOnMap(pawn),
            GeneConditionType.Solo => !HasAllyOnMap(pawn),
            _ => true,
        };
    }

    private static bool IsStationary(Pawn pawn) => pawn.pather == null || !pawn.pather.MovingNow;

    private static bool IsUnarmored(Pawn pawn) => pawn.apparel == null || pawn.apparel.WornApparelCount == 0;

    private static bool HasAllyOnMap(Pawn pawn)
    {
        if (pawn.Map == null)
            return false;
        foreach (Pawn other in pawn.Map.mapPawns.FreeColonistsSpawned)
        {
            if (other != pawn)
                return true;
        }
        return false;
    }
}
