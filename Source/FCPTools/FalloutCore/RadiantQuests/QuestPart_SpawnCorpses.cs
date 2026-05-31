using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FCP.Core.RadiantQuests;

public class QuestPart_SpawnCorpses : QuestPart
{
    public string inSignal;
    public List<Pawn> pawns;
    public int radius = 10;
    public PlanetTile mapTile;
    private bool spawned;

    public override void Notify_QuestSignalReceived(Signal signal)
    {
        base.Notify_QuestSignalReceived(signal);
        if (spawned || signal.tag != inSignal) return;

        MapParent mapParent = Find.World.worldObjects.MapParentAt(mapTile);
        if (mapParent?.HasMap != true || pawns.NullOrEmpty())
        {
            spawned = true;
            return;
        }

        Map map = mapParent.Map;
        IntVec3 center = map.Center;

        foreach (Pawn p in pawns)
        {
            if (p == null) continue;
            if (!p.Dead) p.Kill(null);

            Corpse corpse = p.Corpse;
            if (corpse != null && RCellFinder.TryFindRandomCellNearWith(center, c => c.Standable(map), map, out IntVec3 cell, radius))
            {
                GenSpawn.Spawn(corpse, cell, map);
            }
        }
        spawned = true;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignal, "inSignal");
        Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
        Scribe_Values.Look(ref radius, "radius", 10);
        Scribe_Values.Look(ref mapTile, "mapTile");
        Scribe_Values.Look(ref spawned, "spawned");
    }
}
