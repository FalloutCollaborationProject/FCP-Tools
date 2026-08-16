namespace Thek_BuildingArrivalMode
{
    public class BuildingArrivalModeModExtension : DefModExtension
    {
        public ThingDef buildingDefToSpawnFrom; 
        public int cooldownBetweenPawnsInTicks = 30; 
        public SoundDef soundWhenSpawning; 
        public FleckDef fleckWhenSpawning; 
        public bool shouldOverrideFleeToil = true; 
        public LinkableSettings linkableSettings = new(); 
        public class LinkableSettings
        {
            public bool requiresLinkable = false; 
            public ThingDef LinkableThingDefRequired; 
        }
        internal IntVec3 tileToSpawn; 
    }

}