using RimWorld;
using RimWorld.Planet;

namespace BiomesKit;

public class UniversalBiomeWorker : BiomeWorker
{
    public override float GetScore(BiomeDef biome, Tile tile, PlanetTile planetTile)
    {
        return 0f;
    }
}
