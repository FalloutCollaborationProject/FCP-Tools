using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace FCP.Core.Buildings;

public class JobDriver_RemoveFromPowerLine : JobDriver
{
    private Building_PowerLine PowerLine => (Building_PowerLine)job.GetTarget(TargetIndex.A).Thing;
    
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        if (PowerLine.HasPawn && PowerLine.ContainedPawn != null)
            return pawn.Reserve(PowerLine, job, 1, -1, null, errorOnFailed) && pawn.Reserve(PowerLine.ContainedPawn, job, 1, -1, null, errorOnFailed);
        return pawn.Reserve(PowerLine, job, 1, -1, null, errorOnFailed);
    }
    
    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDestroyedOrNull(TargetIndex.A);
        this.FailOn(() => !PowerLine.HasPawn && !PowerLine.HasCorpse);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
        Toil remove = new Toil();
        remove.initAction = delegate
        {
            Building_PowerLine powerLine = PowerLine;
            if (powerLine.HasCorpse)
            {
                Corpse corpse = powerLine.EjectCorpse();
                if (corpse != null)
                {
                    if (!corpse.Spawned)
                        GenSpawn.Spawn(corpse, powerLine.Position, powerLine.Map);
                    Messages.Message("FCP_PawnRemovedFromPowerLine".Translate(corpse.InnerPawn.LabelShort), corpse, MessageTypeDefOf.NeutralEvent);
                }
            }
            else if (powerLine.HasPawn)
            {
                Pawn victim = powerLine.EjectContents();
                if (victim != null)
                {
                    if (!victim.Spawned)
                        GenSpawn.Spawn(victim, powerLine.Position, powerLine.Map);
                    Hediff crucified = victim.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.FCP_Crucified);
                    if (crucified != null)
                        victim.health.RemoveHediff(crucified);
                    Hediff exhaustion = victim.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.FCP_CrucifixionExhaustion);
                    if (exhaustion != null)
                        victim.health.RemoveHediff(exhaustion);
                    victim.health.AddHediff(HediffDefOf.FCP_WasCrucified);
                    victim.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDef.Named("FCP_Legion_WasCrucified"));
                    pawn.carryTracker.TryStartCarry(victim);
                    Messages.Message("FCP_PawnRemovedFromPowerLine".Translate(victim.LabelShort), victim, MessageTypeDefOf.NeutralEvent);
                }
            }
        };
        remove.defaultCompleteMode = ToilCompleteMode.Instant;
        yield return remove;
    }
}
