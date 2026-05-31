using System.Collections.Generic;
using FCP.Core;
using RimWorld;
using Verse;

namespace FCP.Factions;

public class MapComponent_FactionOutpost : MapComponent
{
	public PrefabDef prefab;
	public List<PawnKindCount> guaranteedPawnKinds = new List<PawnKindCount>();
	public List<CharacterDef> guaranteedCharacters = new List<CharacterDef>();
	public List<SettlementTrader> traders = new List<SettlementTrader>();

	public MapComponent_FactionOutpost(Map map) : base(map)
	{
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref prefab, "prefab");
		Scribe_Collections.Look(ref guaranteedPawnKinds, "guaranteedPawnKinds", LookMode.Deep);
		Scribe_Collections.Look(ref guaranteedCharacters, "guaranteedCharacters", LookMode.Def);
		Scribe_Collections.Look(ref traders, "traders", LookMode.Deep);
	}
}
