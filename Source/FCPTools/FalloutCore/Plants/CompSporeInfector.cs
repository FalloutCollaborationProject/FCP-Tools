using RimWorld;
using Verse;

namespace FCP.Core;

public class CompSporeInfector : ThingComp
{
    private int tickCounter;

    public CompProperties_SporeInfector Props => (CompProperties_SporeInfector)props;

    public override void CompTickInterval(int delta)
    {
        base.CompTickInterval(delta);

        if (!parent.Spawned || parent.Map == null)
            return;

        tickCounter += delta;
        if (tickCounter >= Props.tickInterval)
        {
            tickCounter = 0;
            ReleaseSpores();
        }
    }

    private void ReleaseSpores()
    {
        foreach (Thing thing in GenRadial.RadialDistinctThingsAround(parent.Position, parent.Map, Props.radius, true))
        {
            Pawn pawn = thing as Pawn;
            if (pawn == null || pawn.Dead || !pawn.RaceProps.IsFlesh || pawn.RaceProps.Insect)
                continue;

            if (pawn.health.hediffSet.HasHediff(Props.hediffDef))
                continue;

            if (!Rand.Chance(Props.infectionChance))
                continue;

            pawn.health.AddHediff(Props.hediffDef);
        }
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref tickCounter, "tickCounter", 0);
    }
}

public class CompProperties_SporeInfector : CompProperties
{
    public float radius = 2f;
    public int tickInterval = 2000;
    public float infectionChance = 0.15f;
    public HediffDef hediffDef;

    public CompProperties_SporeInfector()
    {
        compClass = typeof(CompSporeInfector);
    }
}
