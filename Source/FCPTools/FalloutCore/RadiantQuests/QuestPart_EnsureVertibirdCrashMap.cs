using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FCP.Core.RadiantQuests;

public class QuestPart_EnsureVertibirdCrashMap : QuestPart
{
    public string inSignal;
    public MapParent mapParent;
    public ThingDef vertibirdDef;

    public override void Notify_QuestSignalReceived(Signal signal)
    {
        base.Notify_QuestSignalReceived(signal);
        if (signal.tag != inSignal || mapParent?.Map == null) return;

        MapComponent_VertibirdCrash component = mapParent.Map.GetComponent<MapComponent_VertibirdCrash>();
        if (component == null)
        {
            component = new MapComponent_VertibirdCrash(mapParent.Map);
            mapParent.Map.components.Add(component);
        }

        if (component.vertibirdDef == null)
        {
            component.vertibirdDef = vertibirdDef;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignal, "inSignal");
        Scribe_References.Look(ref mapParent, "mapParent");
        Scribe_Defs.Look(ref vertibirdDef, "vertibirdDef");
    }
}
