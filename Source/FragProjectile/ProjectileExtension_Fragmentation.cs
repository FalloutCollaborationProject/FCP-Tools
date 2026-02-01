namespace FragProjectile;

public class ProjectileExtension_Fragmentation : DefModExtension
{
    public int projCount;

    public float projAngle;

    public bool isCone;

    public bool coneFacingIntendedTarget;

    public FloatRange radius;

    public bool isExplodePreemptively;

    public float tickBeforeImpact = 1;

    //public float randomSpread = 0f;

    public ThingDef projectileDef;

    public bool isSureHit;

    public ThingDef sureHitProjectileDef;
}