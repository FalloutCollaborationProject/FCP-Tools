using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FCP.Factions;

public class GenStep_DynamicPrefab : GenStep
{
	public override int SeedPart => 987654321;

	public override void Generate(Map map, GenStepParams parms)
	{
		Settlement settlement = map.Parent as Settlement;
		if (settlement == null)
			return;

		GameComponent_SettlementMapGenerators comp = Current.Game?.GetComponent<GameComponent_SettlementMapGenerators>();
		PrefabDef prefab = comp?.GetPrefab(settlement);
		if (prefab == null)
			return;

		PrefabUtility.SpawnPrefab(prefab, map, map.Center, Rot4.Random);
	}
}
