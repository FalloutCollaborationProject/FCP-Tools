namespace FCP.Core.GECK;

public class CompProperties_GECKActivator : CompProperties
{
    public CompProperties_GECKActivator()
    {
        compClass = typeof(CompGECKActivator);
    }

    public float radius = 24f;
    public TerrainDef targetTerrain;
    public SoundDef activationSound;
    public LetterDef activationLetterDef;
}
