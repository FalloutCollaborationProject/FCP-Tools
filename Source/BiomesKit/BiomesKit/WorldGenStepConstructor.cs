using Verse;

namespace BiomesKit;

[StaticConstructorOnStartup]
internal static class WorldGenStepConstructor
{
	static WorldGenStepConstructor()
	{
		DefDatabase<WorldGenStepDef>.Add(new WorldGenStepDef
		{
			defName = "BiomesKitWorldGenStep",
			order = 999f,
			worldGenStep = new LateBiomeWorker()
		});
	}
}
