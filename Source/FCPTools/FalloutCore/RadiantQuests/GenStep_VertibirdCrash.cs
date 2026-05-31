using RimWorld;
using Verse;

namespace FCP.Core.RadiantQuests;

public class GenStep_VertibirdCrash : GenStep
{
    public override int SeedPart => 987654321;

    public override void Generate(Map map, GenStepParams parms)
    {
        MapComponent_VertibirdCrash component = map.GetComponent<MapComponent_VertibirdCrash>();
        if (component?.vertibirdDef == null) return;

        CellRect rect = CellRect.CenteredOn(map.Center, 15, 15).ClipInsideMap(map);
        if (!CellFinder.TryFindRandomCellInsideWith(rect, c => c.Standable(map) && !c.Roofed(map) && c.GetFirstBuilding(map) == null, out IntVec3 spot)) return;

        Thing vertibird = ThingMaker.MakeThing(component.vertibirdDef);
        vertibird.SetFactionDirect(null);
        
        CompExplosive comp = vertibird.TryGetComp<CompExplosive>();
        if (comp != null)
        {
            comp.destroyedThroughDetonation = true;
        }

        GenSpawn.Spawn(vertibird, spot, map, Rot4.Random);
        component.crashLocation = spot;

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
}
