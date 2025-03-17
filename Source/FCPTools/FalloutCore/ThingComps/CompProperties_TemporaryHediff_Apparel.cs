namespace FCP.Core;

public class CompProperties_TemporaryHediff_Apparel : CompProperties_CauseHediff_Apparel
{
    public ResearchProjectDef projectRequired = null;

    public CompProperties_TemporaryHediff_Apparel()
    {
        compClass = typeof(CompTemporaryHediff_Apparel);
    }
}
