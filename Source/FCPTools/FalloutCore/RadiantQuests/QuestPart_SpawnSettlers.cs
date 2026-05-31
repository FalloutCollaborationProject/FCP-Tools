using RimWorld.Planet;
using Verse;

namespace FCP.Core.RadiantQuests;

public class QuestPart_SpawnSettlers : QuestPart
{
    public string inSignal;
    public List<Pawn> pawns;
    public int radius = 20;
    public PlanetTile mapTile;

    public override void Notify_QuestSignalReceived(Signal signal)
    {
        base.Notify_QuestSignalReceived(signal);

        if (signal.tag == inSignal && pawns != null)
        {
            MapParent parent = Find.WorldObjects.MapParentAt(mapTile);
            if (parent?.Map != null)
            {
                IntVec3 center = parent.Map.Center;
                foreach (Pawn pawn in pawns)
                {
                    IntVec3 loc = CellFinder.RandomClosewalkCellNear(center, parent.Map, radius);
                    GenSpawn.Spawn(pawn, loc, parent.Map);
                }
            }
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignal, "inSignal");
        Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
        Scribe_Values.Look(ref radius, "radius", 20);
        Scribe_Values.Look(ref mapTile, "mapTile");
    }
}
