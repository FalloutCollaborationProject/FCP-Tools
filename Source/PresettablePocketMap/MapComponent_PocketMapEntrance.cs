using Verse;

namespace FCP.PocketMaps
{
    public class MapComponent_PocketMapEntrance : MapComponent
    {
        public MapPortal portal;
        
        public MapComponent_PocketMapEntrance(Map map) : base(map)
        {
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref portal, "portal");
        }
    }
}
