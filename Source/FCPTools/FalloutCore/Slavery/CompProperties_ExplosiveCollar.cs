namespace FCP.Core;

public class CompProperties_ExplosiveCollar : CompProperties
{
    public CompProperties_ExplosiveCollar()
    {
        compClass = typeof(CompExplosiveCollar);
    }

    public float explosionRadius = 2.5f;
    public int explosionDamage = 50;
    public DamageDef explosionDamageDef;
}
