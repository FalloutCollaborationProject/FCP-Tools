using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace FCP.Core.Buildings;

public class JobDriver_StrapToPowerLine : JobDriver
{
    private const float WitnessRadius = 20f;
    
    private Pawn Victim => (Pawn)job.GetTarget(TargetIndex.A).Thing;
    private Building_PowerLine PowerLine => (Building_PowerLine)job.GetTarget(TargetIndex.B).Thing;
    
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.A), job);
        return pawn.Reserve(Victim, job, 1, -1, null, errorOnFailed) && pawn.Reserve(PowerLine, job, 1, -1, null, errorOnFailed);
    }
    
    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDestroyedOrNull(TargetIndex.A);
        this.FailOnDestroyedOrNull(TargetIndex.B);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell).FailOnDespawnedNullOrForbidden(TargetIndex.A);
        Toil checkResistance = new Toil();
        checkResistance.initAction = delegate
        {
            Pawn victim = Victim;
            if (victim.Downed || (victim.guest != null && victim.IsPrisonerOfColony))
                return;
            float resistChance = 0.5f;
            if (victim.IsColonist)
                resistChance = 0.3f;
            if (Rand.Value < resistChance)
            {
                Messages.Message("FCP_StrappingResisted".Translate(victim.LabelShort), victim, MessageTypeDefOf.NegativeEvent);
                victim.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.PanicFlee);
                pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
            }
        };
        checkResistance.defaultCompleteMode = ToilCompleteMode.Instant;
        yield return checkResistance;
        Toil prepare = new Toil();
        prepare.initAction = delegate { job.count = 1; };
        prepare.defaultCompleteMode = ToilCompleteMode.Instant;
        yield return prepare;
        yield return Toils_Haul.StartCarryThing(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell);
        Toil strap = new Toil();
        strap.initAction = delegate
        {
            Pawn victim = Victim;
            Building_PowerLine powerLine = PowerLine;
            pawn.carryTracker.TryDropCarriedThing(powerLine.Position, ThingPlaceMode.Direct, out _);
            if (!powerLine.TryAcceptPawn(victim))
                return;
            if (victim.guest == null)
                victim.guest = new Pawn_GuestTracker(victim);
            victim.guest.SetGuestStatus(Faction.OfPlayer, GuestStatus.Guest);
            CompPowerLine comp = powerLine.GetComp<CompPowerLine>();
            if (comp?.Props != null)
            {
                victim.Position = powerLine.Position + new IntVec3((int)comp.Props.drawOffset.x, (int)comp.Props.drawOffset.y, (int)comp.Props.drawOffset.z);
                victim.Notify_Teleported(false, false);
            }
            victim.Rotation = powerLine.Rotation.Opposite;
            victim.health.AddHediff(HediffDefOf.FCP_Crucified);
            victim.health.AddHediff(HediffDefOf.FCP_CrucifixionExhaustion);
            if (victim.InMentalState)
                victim.mindState.mentalStateHandler.CurState.RecoverFromState();
            if (victim.mindState != null)
                victim.mindState.canFleeIndividual = false;
            victim.jobs?.StopAll();
            victim.pather?.StopDead();
                victim.mindState.canFleeIndividual = false;
            if (victim.drafter != null)
                victim.drafter.Drafted = false;
            pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDef.Named("FCP_Legion_CrucifiedSomeone"));
            foreach (Pawn colonist in powerLine.Map.mapPawns.FreeColonistsSpawned)
            {
                if (colonist.Position.InHorDistOf(powerLine.Position, WitnessRadius))
                    colonist.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDef.Named("FCP_Legion_WitnessedCrucifixion"));
            }
            Messages.Message("FCP_PawnStrapped".Translate(victim.LabelShort), powerLine, MessageTypeDefOf.NegativeEvent);
        };
        strap.defaultCompleteMode = ToilCompleteMode.Instant;
        yield return strap;
    }
}
