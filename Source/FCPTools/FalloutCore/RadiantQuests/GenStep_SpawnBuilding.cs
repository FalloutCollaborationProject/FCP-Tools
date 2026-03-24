namespace FCP.Core.RadiantQuests;

public class GenStep_SpawnBuildingInRoom : GenStep
{
    public ThingDef buildingDef;


    public override int SeedPart => 1123123;

    public override void Generate(Map map, GenStepParams parms)
    {
        CellRect rect = MapGenerator.GetVar<List<CellRect>>("UsedRects").Last();

        Building building = (Building)ThingMaker.MakeThing(buildingDef);

        GenPlace.TryPlaceThing(building, rect.CenterCell, map, ThingPlaceMode.Near, rot: Rot4.East);
    }

}