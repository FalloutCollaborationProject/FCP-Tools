using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace FCP.Core.RadiantQuests;

public class QuestPart_SpawnPawns : QuestPart
{
    public string inSignal;
    public List<Pawn> pawns;
    public MapParent mapParent;

    public override void Notify_QuestSignalReceived(Signal signal)
    {
        base.Notify_QuestSignalReceived(signal);
        
        if (signal.tag != inSignal || pawns == null || mapParent?.Map == null)
        {
            return;
        }

        Map map = mapParent.Map;
        IntVec3 spawnCenter = CellFinder.RandomEdgeCell(map);

        foreach (Pawn pawn in pawns)
        {
            if (pawn == null || pawn.Dead) continue;

            if (CellFinder.TryFindRandomSpawnCellForPawnNear(spawnCenter, map, out IntVec3 c))
            {
                GenSpawn.Spawn(pawn, c, map);
                pawn.jobs?.CheckForJobOverride();
            }
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignal, "inSignal");
        Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
        Scribe_References.Look(ref mapParent, "mapParent");
    }
}
