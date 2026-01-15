using Verse.AI.Group;

namespace Thek_BuildingArrivalMode
{
    public class RaidStrategyWorker_BuildingArrivalMode : RaidStrategyWorker
    {
        // Copied from ImmediateAttack, replaced all the lordjobs with custom ones
        protected override LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed)
        {
            IntVec3 originCell = PawnsArrivalModeWorker_BuildingArrivalMode.modExtension.tileToSpawn;
            if (parms.attackTargets != null && parms.attackTargets.Count > 0)
            {
                return new LordJob_BuildingArrivalMode_AssaultThings(parms.faction, parms.attackTargets);
            }
            if (parms.faction.HostileTo(Faction.OfPlayer))
            {
                return new LordJob_BuildingArrivalMode_AssaultColony(parms.faction, canKidnap: true, parms.canTimeoutOrFlee);
            }
            return new LordJob_BuildingArrivalMode_AssistColony(parms.faction, originCell);
        }
    }
}
