namespace FCP.Core;

public class CompExplosiveCollar : ThingComp
{
    private const int CheckInterval = 60;

    public CompProperties_ExplosiveCollar Props => (CompProperties_ExplosiveCollar)props;

    private Pawn Wearer => (parent as Apparel)?.Wearer;

    public override void CompTick()
    {
        base.CompTick();

        if (!parent.IsHashIntervalTick(CheckInterval))
            return;

        Pawn wearer = Wearer;
        if (wearer == null || !wearer.IsSlave)
            return;

        if (wearer.MentalState?.def == MentalStateDefOf.Rebellion)
            Detonate(wearer);
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        Pawn wearer = Wearer;
        if (wearer == null || !wearer.IsSlave || wearer.Faction != Faction.OfPlayer)
            yield break;

        yield return new Command_Action
        {
            defaultLabel = "FCP_Collar_Detonate".Translate(),
            defaultDesc = "FCP_Collar_Detonate_Desc".Translate(),
            icon = TexCommand.Attack,
            action = () => Detonate(wearer)
        };
    }

    private void Detonate(Pawn wearer)
    {
        if (wearer.Map == null)
            return;

        GenExplosion.DoExplosion(wearer.Position, wearer.Map, Props.explosionRadius,
            Props.explosionDamageDef ?? DamageDefOf.Bomb, wearer, Props.explosionDamage);
    }
}
