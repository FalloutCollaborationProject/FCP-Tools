using Verse;

namespace FCP.Core.Buildings
{
    public class PlaceWorker_OnWall : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            if (!loc.InBounds(map))
                return "MustPlaceOnWall".Translate();
            
            IntVec3 wallCell = loc + rot.FacingCell;
            
            if (!wallCell.InBounds(map))
                return "MustPlaceOnWall".Translate();
            
            Building edifice = wallCell.GetEdifice(map);
            if (edifice == null || !edifice.def.IsEdifice())
                return "MustPlaceOnWall".Translate();
            
            if (!edifice.def.building.isNaturalRock && !edifice.def.building.isResourceRock && edifice.def.graphicData?.linkType != LinkDrawerType.CornerFiller)
                return "MustPlaceOnWall".Translate();
            
            return true;
        }
    }
}
