using System.Collections.Generic;
using FCP.Core;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace FCP.Factions;

public class GenStep_SpawnSettlementPawns : GenStep
{
	private static List<Building> tmpBuildings = new List<Building>();

	public override int SeedPart => 123456789;

	public override void Generate(Map map, GenStepParams parms)
	{
		Settlement settlement = map.Parent as Settlement;
		if (settlement == null || settlement.Faction == null || settlement.Faction.defeated)
			return;

		GameComponent_SettlementMapGenerators comp = Current.Game.GetComponent<GameComponent_SettlementMapGenerators>();
		List<CharacterDef> guaranteedCharacters = comp?.GetGuaranteedCharacters(settlement);
		List<PawnKindDef> guaranteedPawns = comp?.GetGuaranteedPawns(settlement);
		int num = Rand.RangeInclusive(8, 16);
		int num2 = 0;
		Lord lord = LordMaker.MakeNewLord(settlement.Faction, new LordJob_DefendBase(settlement.Faction, map.Center, 0, false), map);
		
		if (guaranteedCharacters != null)
		{
			UniqueCharactersTracker tracker = UniqueCharactersTracker.Instance;
			for (int i = 0; i < guaranteedCharacters.Count; i++)
			{
				Pawn pawn = tracker.GetOrGenPawn(guaranteedCharacters[i], null, settlement.Faction);
				if (pawn != null)
				{
					GenSpawn.Spawn(pawn, GetSpawnPosition(map), map);
					lord.AddPawn(pawn);
					num2++;
				}
			}
		}

		if (guaranteedPawns != null)
		{
			for (int j = 0; j < guaranteedPawns.Count; j++)
			{
				Pawn pawn2 = PawnGenerator.GeneratePawn(guaranteedPawns[j], settlement.Faction);
				GenSpawn.Spawn(pawn2, GetSpawnPosition(map), map);
				lord.AddPawn(pawn2);
				num2++;
			}
		}

		for (int k = num2; k < num; k++)
		{
			Pawn pawn3 = PawnGenerator.GeneratePawn(settlement.Faction.RandomPawnKind(), settlement.Faction);
			GenSpawn.Spawn(pawn3, GetSpawnPosition(map), map);
			lord.AddPawn(pawn3);
		}
	}

	IntVec3 GetSpawnPosition(Map map)
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
			if (CellFinder.TryFindRandomCellInsideWith(rect, (IntVec3 c) => c.Standable(map) && !c.Fogged(map), out result))
				return result;
		}
		IntVec3 result2;
		if (CellFinder.TryFindRandomCellNear(map.Center, map, 50, (IntVec3 c) => c.Standable(map) && !c.Fogged(map), out result2))
			return result2;
		return CellFinder.RandomSpawnCellForPawnNear(map.Center, map);
	}
}
