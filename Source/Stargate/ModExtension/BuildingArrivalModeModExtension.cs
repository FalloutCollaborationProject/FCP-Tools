namespace Thek_BuildingArrivalMode
{
    public class BuildingArrivalModeModExtension : DefModExtension
    {
        public ThingDef buildingDefToSpawnFrom; // The building this will look for to spawn the pawns at.
        public int cooldownBetweenPawnsInTicks = 30; // The time that it'll have to wait between the last pawn the current pawn, in ticks.
        public SoundDef soundWhenSpawning; // If you wanna give it a sound when they appear because we're balenciaga
        public FleckDef fleckWhenSpawning; // If you wanna give it an effect when they appear because we're balenciaga
        public bool shouldOverrideFleeToil = true; // This is for them to flee to a portal, if set to false they'll use the vanilla flee.
        public LinkableSettings linkableSettings = new(); // to be fair this isn't needed i just wanted to mess with it, this works like this:
        /* <linkableSettings>
         *      <requiresLinkable>bool</requiresLinkable>
         *      <LinkableThingDefRequired>defName</LinkableThingDefRequired>
         * </linkableSettings>
         */
        public class LinkableSettings
        {
            public bool requiresLinkable = false; // If you want a linkable, just put true in the xml, no need to write false
            public ThingDef LinkableThingDefRequired; // Specify the linkable here
        }
        internal IntVec3 tileToSpawn; // This is not an XML field, just a place to save where the spot where the pawns come and go is at
    }

    // EXAMPLE XML
    /*
     *<modExtensions>
     *    <li Class="Thek_BuildingArrivalMode.BuildingArrivalModeModExtension">
     *        <buildingDefToSpawnFrom>buildingDefName</buildingDefToSpawnFrom>
     *        <soundWhenSpawning>soundDefName</soundWhenSpawning>
     *        <fleckWhenSpawning>fleckDefName</fleckWhenSpawning>
     *        <cooldownBetweenPawnsInTicks>int</cooldownBetweenPawnsInTicks>
     *        <shouldOverrideFleeToil>false</shouldOverrideFleeToil>
     *        <linkableSettings>
     *            <requiresLinkable>true or false</requiresLinkable>
     *            <LinkableThingDefRequired>buildingDefName</LinkableThingDefRequired>
     *        </linkableSettings>
     *    </li>
     *</modExtensions>
     */

    // EXAMPLE XML OF THE COMP

    /*
     *<comps>
     *    <li>
     *        <compClas>Thek_BuildingArrivalMode.Comp_TickBuildingSpawn</compClass>
     *    </li>
     *</comps>
     */
}