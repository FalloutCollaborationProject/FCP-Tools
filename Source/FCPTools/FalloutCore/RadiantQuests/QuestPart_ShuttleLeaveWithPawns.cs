using RimWorld;
using RimWorld.Planet;
using FCP.Core.Shuttles;
using Verse;

namespace FCP.Core.RadiantQuests;

public class QuestPart_ShuttleLeaveWithPawns : QuestPart
{
    public string inSignal;
    public List<Pawn> pawns;
    public MapParent mapParent;
    public string outSignalSuccess;
    public Faction faction;

    public override void Notify_QuestSignalReceived(Signal signal)
    {
        base.Notify_QuestSignalReceived(signal);
        if (signal.tag != inSignal || pawns == null || mapParent?.Map == null) return;

        Map map = mapParent.Map;
        IntVec3 spot;
        
        if (!DropCellFinder.TryFindShipLandingArea(map, out spot, out Thing _))
        {
            DropCellFinder.FindSafeLandingSpot(out spot, null, map, 35, 15, 25);
        }

        List<Pawn> livingPawns = pawns.Where(p => p != null && !p.Dead && p.Spawned).ToList();
        if (livingPawns.Count == 0) return;
        
        TransportShipDef shipDef = TransportShipDefOf.Ship_Shuttle;
        FactionModExtension ext = faction?.def?.GetModExtension<FactionModExtension>();
        if (ext?.transportShipDef != null)
        {
            shipDef = ext.transportShipDef;
        }

        foreach (Pawn pawn in livingPawns)
        {
            pawn.DeSpawn();
        }
        
        TransportShip ship = TransportShipMaker.MakeTransportShip(shipDef, livingPawns, null);
        ship.ArriveAt(spot, mapParent);
        ship.AddJobs(ShipJobDefOf.FlyAway);
        
        if (!outSignalSuccess.NullOrEmpty())
        {
            Find.SignalManager.SendSignal(new Signal(outSignalSuccess));
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignal, "inSignal");
        Scribe_Values.Look(ref outSignalSuccess, "outSignalSuccess");
        Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
        Scribe_References.Look(ref mapParent, "mapParent");
        Scribe_References.Look(ref faction, "faction");
    }
}
