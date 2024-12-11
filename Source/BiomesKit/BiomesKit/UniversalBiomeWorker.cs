using RimWorld;
using RimWorld.Planet;

namespace BiomesKit;

public class UniversalBiomeWorker : BiomeWorker
{
	public override float GetScore(Tile tile, int tileID)
	{
		return 0f;
	}
}
