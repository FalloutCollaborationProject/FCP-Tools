using RimWorld;
using Verse;
using Verse.AI;

namespace FCP.Core;

public class HediffCompProperties_HostileAnimal : HediffCompProperties
{
    public MentalStateDef animalMentalState;

    public HediffCompProperties_HostileAnimal()
    {
        compClass = typeof(HediffComp_HostileAnimal);
    }
}

public class HediffComp_HostileAnimal : HediffComp
{
    private bool applied;

    public HediffCompProperties_HostileAnimal HostileProps => (HediffCompProperties_HostileAnimal)props;

    public override void CompPostTick(ref float severityAdjustment)
    {
        if (applied) return;
        applied = true;
        Pawn pawn = Pawn;
        MentalStateDef stateDef = HostileProps.animalMentalState;
        if (stateDef == null || !pawn.RaceProps.Animal || pawn.MentalStateDef == stateDef) return;
        pawn.mindState.mentalStateHandler.TryStartMentalState(
            stateDef,
            reason: null,
            forced: true,
            forceWake: true,
            transitionSilently: true);
    }
}
