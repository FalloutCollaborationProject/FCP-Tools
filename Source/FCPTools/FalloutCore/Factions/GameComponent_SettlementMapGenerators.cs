using System.Collections.Generic;
using FCP.Core;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FCP.Factions;

public class GameComponent_SettlementMapGenerators : GameComponent
{
	private Dictionary<int, MapGeneratorDef> mapGenerators = new Dictionary<int, MapGeneratorDef>();
	private Dictionary<int, IntVec3> mapSizes = new Dictionary<int, IntVec3>();
	private Dictionary<int, PrefabDef> prefabs = new Dictionary<int, PrefabDef>();
	private Dictionary<int, List<PawnKindCount>> guaranteedPawns = new Dictionary<int, List<PawnKindCount>>();
	private Dictionary<int, List<CharacterDef>> guaranteedCharacters = new Dictionary<int, List<CharacterDef>>();
	private Dictionary<int, List<SettlementTrader>> traders = new Dictionary<int, List<SettlementTrader>>();

	public GameComponent_SettlementMapGenerators(Game game)
	{
	}

	public void RegisterSettlement(Settlement settlement, MapGeneratorDef mapGenerator, IntVec3 mapSize)
	{
		if (settlement == null || mapGenerator == null)
			return;

		mapGenerators[settlement.ID] = mapGenerator;
		if (mapSize != IntVec3.Invalid)
			mapSizes[settlement.ID] = mapSize;
	}

	public void RegisterSettlementWithPrefab(Settlement settlement, PrefabDef prefab, IntVec3 mapSize, List<PawnKindCount> guaranteedPawnKinds = null, List<CharacterDef> guaranteedCharacterDefs = null, List<SettlementTrader> settlementTraders = null)
	{
		if (settlement == null || prefab == null)
			return;

		MapGeneratorDef genericMapGen = DefDatabase<MapGeneratorDef>.GetNamed("FCP_MapGen_Settlement_Prefab", false);
		if (genericMapGen != null)
			mapGenerators[settlement.ID] = genericMapGen;
		
		prefabs[settlement.ID] = prefab;
		if (mapSize != IntVec3.Invalid)
			mapSizes[settlement.ID] = mapSize;
		if (guaranteedPawnKinds != null && guaranteedPawnKinds.Count > 0)
			guaranteedPawns[settlement.ID] = guaranteedPawnKinds;
		if (guaranteedCharacterDefs != null && guaranteedCharacterDefs.Count > 0)
			guaranteedCharacters[settlement.ID] = guaranteedCharacterDefs;
		if (settlementTraders != null && settlementTraders.Count > 0)
			traders[settlement.ID] = settlementTraders;
	}

	public MapGeneratorDef GetMapGenerator(Settlement settlement)
	{
		if (settlement == null)
			return null;
		
		mapGenerators.TryGetValue(settlement.ID, out MapGeneratorDef result);
		return result;
	}

	public PrefabDef GetPrefab(Settlement settlement)
	{
		if (settlement == null)
			return null;

		prefabs.TryGetValue(settlement.ID, out PrefabDef result);
		return result;
	}

	public IntVec3 GetMapSize(Settlement settlement)
	{
		if (settlement == null)
			return IntVec3.Invalid;

		IntVec3 result;
		if (mapSizes.TryGetValue(settlement.ID, out result))
			return result;
		return IntVec3.Invalid;
	}

	public List<PawnKindCount> GetGuaranteedPawns(Settlement settlement)
	{
		if (settlement == null)
			return null;

		guaranteedPawns.TryGetValue(settlement.ID, out List<PawnKindCount> result);
		return result;
	}

	public List<CharacterDef> GetGuaranteedCharacters(Settlement settlement)
	{
		if (settlement == null)
			return null;

		guaranteedCharacters.TryGetValue(settlement.ID, out List<CharacterDef> result);
		return result;
	}

	public List<SettlementTrader> GetTraders(Settlement settlement)
	{
		if (settlement == null)
			return null;

		traders.TryGetValue(settlement.ID, out List<SettlementTrader> result);
		return result;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		
		Scribe_Collections.Look(ref mapGenerators, "mapGenerators", LookMode.Value, LookMode.Def);
		Scribe_Collections.Look(ref mapSizes, "mapSizes", LookMode.Value, LookMode.Value);
		Scribe_Collections.Look(ref prefabs, "prefabs", LookMode.Value, LookMode.Def);
		Scribe_Collections.Look(ref guaranteedPawns, "guaranteedPawns", LookMode.Value, LookMode.Deep);
		Scribe_Collections.Look(ref guaranteedCharacters, "guaranteedCharacters", LookMode.Value, LookMode.Def);
		Scribe_Collections.Look(ref traders, "traders", LookMode.Value, LookMode.Deep);

		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			if (mapGenerators == null)
				mapGenerators = new Dictionary<int, MapGeneratorDef>();
			if (mapSizes == null)
				mapSizes = new Dictionary<int, IntVec3>();
			if (prefabs == null)
				prefabs = new Dictionary<int, PrefabDef>();
			if (guaranteedPawns == null)
				guaranteedPawns = new Dictionary<int, List<PawnKindCount>>();
			if (guaranteedCharacters == null)
				guaranteedCharacters = new Dictionary<int, List<CharacterDef>>();
			if (traders == null)
				traders = new Dictionary<int, List<SettlementTrader>>();
		}
	}
}
