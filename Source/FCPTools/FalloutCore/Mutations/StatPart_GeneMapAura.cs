namespace FCP.Mutations;

public class StatPart_GeneMapAura : StatPart
{
    public GeneDef gene;
    public float offset;

    public override void TransformValue(StatRequest req, ref float val)
    {
        if (Applies(req))
            val += offset;
    }

    public override string ExplanationPart(StatRequest req)
    {
        if (!Applies(req))
            return null;
        return gene.label.CapitalizeFirst() + " (nearby ally): " + offset.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Offset);
    }

    private bool Applies(StatRequest req)
    {
        if (req.Thing is not Pawn pawn || pawn.Dead || pawn.Map == null)
            return false;
        if (pawn.genes != null && pawn.genes.HasActiveGene(gene))
            return false;

        foreach (Pawn other in pawn.Map.mapPawns.FreeColonistsSpawned)
        {
            if (other != pawn && other.genes != null && other.genes.HasActiveGene(gene))
                return true;
        }
        return false;
    }
}
