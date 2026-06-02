using RimWorld;
using Verse;

namespace FCP.Core;

public class CompRadioactiveBarrel : ThingComp
{
    private int tickCounter;
    
    public CompProperties_RadioactiveBarrel Props => (CompProperties_RadioactiveBarrel)props;

    public override void CompTick()
    {
        base.CompTick();

        if (!parent.Spawned || parent.Map == null)
            return;

        tickCounter++;
        if (tickCounter >= Props.tickInterval)
        {
            tickCounter = 0;
            EmitRadiation();
        }
    }

    private void EmitRadiation()
    {
        foreach (Thing thing in GenRadial.RadialDistinctThingsAround(parent.Position, parent.Map, Props.radius, true))
        {
            Pawn pawn = thing as Pawn;
            if (pawn == null || pawn.Dead || !pawn.RaceProps.IsFlesh)
                continue;

            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ToxicBuildup);
            if (hediff != null)
            {
                hediff.Severity += Props.toxicBuildupAmount;
            }
            else
            {
                hediff = pawn.health.AddHediff(HediffDefOf.ToxicBuildup);
                hediff.Severity = Props.toxicBuildupAmount;
            }
        }
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref tickCounter, "tickCounter", 0);
    }
}

public class CompProperties_RadioactiveBarrel : CompProperties
{
    public float radius = 3f;
    public int tickInterval = 600;
    public float toxicBuildupAmount = 0.01f;

    public CompProperties_RadioactiveBarrel()
    {
        compClass = typeof(CompRadioactiveBarrel);
    }
}
