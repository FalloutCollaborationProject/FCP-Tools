namespace FCP.Core;

public class ProjectileExtension_Fragmentation : DefModExtension
{
    public int projCount;
    public float projAngle;
    public bool isCone;
    public bool coneFacingIntendedTarget;
    public FloatRange radius;
    public bool isExplodePreemptively;
    public float tickBeforeImpact = 1;
    public ThingDef projectileDef;
    public bool isSureHit;
    public ThingDef sureHitProjectileDef;
}
