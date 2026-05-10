using System.Collections.Generic;

namespace FCP.Core;

public class IngestionOutcomeDoer_SpawnItem : IngestionOutcomeDoer
{
    public List<SpawnThingInfo> thingDefs;
    public bool spawnInInventory = false;

    protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
    {
        if (Rand.Chance(chance))
        {
            ThingDef thingDef = thingDefs.RandomElementByWeight(x => x.weight).thingDef;
            Thing thing = ThingMaker.MakeThing(thingDef);
            GenPlace.TryPlaceThing(thing, pawn.PositionHeld, pawn.MapHeld, ThingPlaceMode.Near);
        }
    }
}
