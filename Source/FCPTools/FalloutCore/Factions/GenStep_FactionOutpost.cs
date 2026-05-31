using System.Collections.Generic;
using FCP.Core;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace FCP.Factions;

public class GenStep_FactionOutpost : GenStep
{
	private static List<Building> tmpBuildings = new List<Building>();

	public override int SeedPart => 987654321;

	public override void Generate(Map map, GenStepParams parms)
	{
		Site site = map.Parent as Site;
		if (site == null || site.Faction == null || site.Faction.defeated)
			return;

		GameComponent_FactionOutpostTemplates templateComp = Current.Game?.GetComponent<GameComponent_FactionOutpostTemplates>();
		if (templateComp == null)
			return;

		NamedSettlement template = templateComp.GetTemplate(site.Tile);
		if (template == null)
			return;

		MapComponent_FactionOutpost comp = map.GetComponent<MapComponent_FactionOutpost>();
		if (comp == null)
		{
			comp = new MapComponent_FactionOutpost(map);
			map.components.Add(comp);
		}

		comp.prefab = template.prefab;
		comp.guaranteedPawnKinds = template.guaranteedPawnKinds;
		comp.guaranteedCharacters = template.guaranteedCharacters;
		comp.traders = template.traders;

		templateComp.RemoveTemplate(site.Tile);

		if (comp.prefab != null)
			PrefabUtility.SpawnPrefab(comp.prefab, map, map.Center, Rot4.North);

		SpawnPawns(map, site.Faction, comp);
	}

	private void SpawnPawns(Map map, Faction faction, MapComponent_FactionOutpost comp)
	{
		Lord lord = LordMaker.MakeNewLord(faction, new LordJob_DefendBase(faction, map.Center, 0, false), map);

		if (comp.guaranteedCharacters != null)
		{
			UniqueCharactersTracker tracker = UniqueCharactersTracker.Instance;
			for (int i = 0; i < comp.guaranteedCharacters.Count; i++)
			{
				Pawn pawn = tracker.GetOrGenPawn(comp.guaranteedCharacters[i], null, faction);
				if (pawn != null)
				{
					GenSpawn.Spawn(pawn, FindSpawnLocation(map), map);
					lord.AddPawn(pawn);
				}
			}
		}

		if (comp.guaranteedPawnKinds != null)
		{
			for (int i = 0; i < comp.guaranteedPawnKinds.Count; i++)
			{
				PawnKindCount pawnKindCount = comp.guaranteedPawnKinds[i];
				if (pawnKindCount.pawnKindDef != null)
				{
					for (int j = 0; j < pawnKindCount.count; j++)
					{
						Pawn pawn = PawnGenerator.GeneratePawn(pawnKindCount.pawnKindDef, faction);
						GenSpawn.Spawn(pawn, FindSpawnLocation(map), map);
						lord.AddPawn(pawn);
					}
				}
			}
		}

		if (comp.traders != null && comp.traders.Count > 0)
			SetupTraders(map, comp.traders);
	}

	private IntVec3 FindSpawnLocation(Map map)
	{
		List<Building> allBuildings = map.listerBuildings.allBuildingsNonColonist;
		tmpBuildings.Clear();
		for (int i = 0; i < allBuildings.Count; i++)
		{
			Building building = allBuildings[i];
			if (building.def.building != null && !building.def.building.isNaturalRock && building.Position.Roofed(map))
				tmpBuildings.Add(building);
		}
		if (tmpBuildings.Count > 0)
		{
			CellRect rect = tmpBuildings.RandomElement().OccupiedRect();
			IntVec3 result;
			if (CellFinder.TryFindRandomCellInsideWith(rect, (IntVec3 c) => c.Standable(map), out result))
				return result;
		}
		IntVec3 result2;
		if (CellFinder.TryFindRandomCellNear(map.Center, map, 50, (IntVec3 c) => c.Standable(map), out result2))
			return result2;
		return CellFinder.RandomSpawnCellForPawnNear(map.Center, map);
	}

	private void SetupTraders(Map map, List<SettlementTrader> traders)
	{
		WorldObjectComp_SettlementTraders comp = map.Parent.GetComponent<WorldObjectComp_SettlementTraders>();
		if (comp == null)
		{
			comp = new WorldObjectComp_SettlementTraders();
			comp.parent = map.Parent;
			map.Parent.AllComps.Add(comp);
		}
		comp.SetTraders(traders);
	}
}
