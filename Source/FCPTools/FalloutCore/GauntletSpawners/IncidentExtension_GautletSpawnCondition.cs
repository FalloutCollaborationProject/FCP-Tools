namespace FCP.Core;

public class IncidentExtension_GautletSpawnCondition : DefModExtension
{
    public List<TerrainDef> disallowedTerrains;
    public List<TerrainDef> allowedTerrains;
    public bool isAllowRoofed;
    public bool isAllowFogged;
    public float radius = 1f;
    public FactionDef factionDef;
    public ThingDef thingDef;
    public int spawnerPoint;
    public int count;
}
