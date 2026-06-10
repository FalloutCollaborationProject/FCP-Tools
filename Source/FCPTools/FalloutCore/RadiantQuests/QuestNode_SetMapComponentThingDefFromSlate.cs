using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace FCP.Core.RadiantQuests;

public class QuestNode_SetMapComponentVertibirdDef : QuestNode
{
    public SlateRef<MapParent> map;
    public SlateRef<ThingDef> thingDef;

    protected override bool TestRunInt(Slate slate)
    {
        return true;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        MapParent mapParent = map.GetValue(slate);
        ThingDef def = thingDef.GetValue(slate);
        if (mapParent == null || def == null) return;

        GameComponent_QuestVertibirdDefs component = Current.Game.GetComponent<GameComponent_QuestVertibirdDefs>();
        if (component == null)
        {
            component = new GameComponent_QuestVertibirdDefs(Current.Game);
            Current.Game.components.Add(component);
        }
        component.SetVertibirdDef(mapParent, def);
    }
}

public class GameComponent_QuestVertibirdDefs : GameComponent
{
    private Dictionary<MapParent, ThingDef> defs = new Dictionary<MapParent, ThingDef>();

    public GameComponent_QuestVertibirdDefs()
    {
    }

    public GameComponent_QuestVertibirdDefs(Game game)
    {
    }

    public void SetVertibirdDef(MapParent parent, ThingDef def)
    {
        defs[parent] = def;
        if (parent.Map != null)
        {
            MapComponent_VertibirdCrash component = parent.Map.GetComponent<MapComponent_VertibirdCrash>();
            if (component != null)
            {
                component.vertibirdDef = def;
            }
        }
    }

    public void ApplyToMap(MapParent parent)
    {
        if (defs.TryGetValue(parent, out ThingDef def) && parent.Map != null)
        {
            MapComponent_VertibirdCrash component = parent.Map.GetComponent<MapComponent_VertibirdCrash>();
            if (component != null)
            {
                component.vertibirdDef = def;
            }
        }
    }

    public override void FinalizeInit()
    {
        base.FinalizeInit();
        if (defs == null)
        {
            defs = new Dictionary<MapParent, ThingDef>();
            return;
        }
        defs.RemoveAll((KeyValuePair<MapParent, ThingDef> kvp) => kvp.Key == null || !kvp.Key.Spawned);
        foreach (KeyValuePair<MapParent, ThingDef> kvp in defs)
        {
            ApplyToMap(kvp.Key);
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            defs.RemoveAll((KeyValuePair<MapParent, ThingDef> kvp) => kvp.Key == null || !kvp.Key.Spawned);
        }
        Scribe_Collections.Look(ref defs, "defs", LookMode.Reference, LookMode.Def, ref defsKeys, ref defsValues);
        if (Scribe.mode == LoadSaveMode.LoadingVars && defs == null)
        {
            defs = new Dictionary<MapParent, ThingDef>();
        }
    }

    private List<MapParent> defsKeys;
    private List<ThingDef> defsValues;
}


