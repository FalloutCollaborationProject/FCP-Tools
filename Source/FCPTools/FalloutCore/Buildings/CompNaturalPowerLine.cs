using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace FCP.Core.Buildings;

public class CompProperties_NaturalPowerLine : CompProperties
{
    public List<FactionDef> allowedFactions;
    public FloatRange pawnDeadChance = new FloatRange(0.3f, 0.7f);
    public bool alwaysHasPawn = true;
    
    public CompProperties_NaturalPowerLine()
    {
        compClass = typeof(CompNaturalPowerLine);
    }
}

public class CompNaturalPowerLine : ThingComp
{
    private bool initialized;
    
    public CompProperties_NaturalPowerLine Props => (CompProperties_NaturalPowerLine)props;
    
    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        if (initialized || respawningAfterLoad)
            return;
        initialized = true;
        parent.SetFaction(null);
        Building_PowerLine powerLine = parent as Building_PowerLine;
        if (powerLine != null && !powerLine.HasPawn && !powerLine.HasCorpse && (Props.alwaysHasPawn || Rand.Chance(0.6f)))
            TryGeneratePawn();
    }
    
    private void TryGeneratePawn()
    {
        Building_PowerLine powerLine = parent as Building_PowerLine;
        if (powerLine == null || powerLine.HasPawn || powerLine.HasCorpse)
            return;
        List<FactionDef> factions = Props.allowedFactions?.Where(f => f != null).ToList();
        if (factions.NullOrEmpty())
            return;
        Faction faction = Find.FactionManager.FirstFactionOfDef(factions.RandomElement());
        if (faction == null)
            return;
        PawnKindDef pawnKind = faction.def.pawnGroupMakers?.SelectMany(pgm => pgm.options).Where(opt => opt?.kind != null).Select(opt => opt.kind).RandomElementWithFallback() ?? PawnKindDefOf.Slave;
        Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKind, faction, PawnGenerationContext.NonPlayer, forceGenerateNewPawn: true, canGeneratePawnRelations: false));
        CompPowerLine comp = powerLine.GetComp<CompPowerLine>();
        IntVec3 spawnPos = powerLine.Position;
        if (comp?.Props != null)
            spawnPos += new IntVec3((int)comp.Props.drawOffset.x, (int)comp.Props.drawOffset.y, (int)comp.Props.drawOffset.z);
        GenSpawn.Spawn(pawn, spawnPos, powerLine.Map);
        pawn.health.AddHediff(HediffDefOf.FCP_Crucified);
        pawn.health.AddHediff(HediffDefOf.FCP_CrucifixionExhaustion);
        pawn.jobs?.StopAll();
        pawn.pather?.StopDead();
        if (pawn.InMentalState)
            pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
        if (pawn.mindState != null)
            pawn.mindState.canFleeIndividual = false;
        if (Rand.Chance(Props.pawnDeadChance.RandomInRange))
        {
            pawn.Kill(null);
            if (pawn.Corpse != null)
            {
                pawn.Corpse.Rotation = powerLine.Rotation.Opposite;
                powerLine.TryAcceptCorpse(pawn.Corpse);
            }
        }
        else
        {
            pawn.Rotation = powerLine.Rotation.Opposite;
            powerLine.TryAcceptPawn(pawn);
        }
    }
    
    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref initialized, "initialized", false);
    }
}
