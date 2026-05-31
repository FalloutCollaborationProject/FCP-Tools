using RimWorld.Planet;

namespace FCP.Core.RadiantQuests;

public class QuestPart_CacheSecured : QuestPart
{
    public string checkSignal;
    public string outSignal;
    public int mapTile;

    public override void Notify_QuestSignalReceived(Signal signal)
    {
        base.Notify_QuestSignalReceived(signal);

        if (signal.tag == checkSignal)
        {
            MapParent parent = Find.WorldObjects.MapParentAt(mapTile);
            if (parent?.Map != null)
            {
                Map map = parent.Map;
                
                bool anyHostileAlive = map.mapPawns.AllPawnsSpawned.Any(p => 
                    p.HostileTo(Faction.OfPlayer) && 
                    !p.Dead && 
                    !p.Downed);

                if (!anyHostileAlive && !outSignal.NullOrEmpty())
                {
                    Find.SignalManager.SendSignal(new Signal(outSignal, signal.args));
                }
            }
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref checkSignal, "checkSignal");
        Scribe_Values.Look(ref outSignal, "outSignal");
        Scribe_Values.Look(ref mapTile, "mapTile");
    }
}
