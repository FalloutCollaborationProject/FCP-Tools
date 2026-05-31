using Verse;
using RimWorld;

namespace FCP.Core.RadiantQuests;

public class MapComponent_VertibirdCrash : MapComponent
{
    public IntVec3 crashLocation = IntVec3.Invalid;
    public ThingDef vertibirdDef;
    private bool spawned;

    public MapComponent_VertibirdCrash(Map map) : base(map)
    {
    }

    public override void FinalizeInit()
    {
        base.FinalizeInit();
        if (vertibirdDef == null)
        {
            GameComponent_QuestVertibirdDefs component = Current.Game?.GetComponent<GameComponent_QuestVertibirdDefs>();
            component?.ApplyToMap(map.Parent);
        }

        if (!spawned && vertibirdDef != null)
        {
            SpawnVertibird();
            spawned = true;
        }
    }

    private void SpawnVertibird()
    {
        CellRect rect = CellRect.CenteredOn(map.Center, 15, 15).ClipInsideMap(map);
        if (!CellFinder.TryFindRandomCellInsideWith(rect, c => c.Standable(map) && !c.Roofed(map) && c.GetFirstBuilding(map) == null, out IntVec3 spot)) return;

        Thing vertibird = ThingMaker.MakeThing(vertibirdDef);
        vertibird.SetFactionDirect(null);
        
        CompExplosive comp = vertibird.TryGetComp<CompExplosive>();
        if (comp != null)
        {
            comp.destroyedThroughDetonation = true;
        }

        GenSpawn.Spawn(vertibird, spot, map, Rot4.North);
        crashLocation = spot;

        int radius = GenRadial.NumCellsInRadius(6f);
        for (int i = 0; i < radius; i++)
        {
            IntVec3 c = spot + GenRadial.RadialPattern[i];
            if (c.InBounds(map) && Rand.Chance(0.3f))
            {
                GenExplosion.DoExplosion(c, map, 2.9f, DamageDefOf.Flame, null);
            }
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref crashLocation, "crashLocation", IntVec3.Invalid);
        Scribe_Defs.Look(ref vertibirdDef, "vertibirdDef");
        Scribe_Values.Look(ref spawned, "spawned");
    }
}
