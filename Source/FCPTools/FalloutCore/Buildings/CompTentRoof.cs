using RimWorld;
using Verse;

namespace FCP.Core.Buildings;

public class CompProperties_TentRoof : CompProperties
{
    public RoofDef roofDef;

    public CompProperties_TentRoof()
    {
        compClass = typeof(CompTentRoof);
    }
}

public class CompTentRoof : ThingComp
{
    public CompProperties_TentRoof Props => (CompProperties_TentRoof)props;

    private RoofDef RoofDef => Props.roofDef ?? RoofDefOf.RoofConstructed;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        SetRoof(RoofDef, parent.Map);
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        SetRoof(null, map);
        base.PostDeSpawn(map, mode);
    }

    private void SetRoof(RoofDef roof, Map map)
    {
        if (map == null)
            return;
        foreach (IntVec3 cell in parent.OccupiedRect())
        {
            if (!cell.InBounds(map))
                continue;
            RoofDef existing = cell.GetRoof(map);
            if (existing != null && existing != RoofDef)
                continue;
            map.roofGrid.SetRoof(cell, roof);
        }
    }
}
