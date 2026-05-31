using System.Collections.Generic;
using FCP.Core;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FCP.Factions;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class FactionExtension_SettlementControl : DefModExtension
{
	public int maxSettlements;
	public List<NamedSettlement> namedSettlements;
	public List<Hilliness> preferredHilliness;
	public List<BiomeDef> allowedBiomes;
	public int searchRadius = 40;
	public bool clustered;
	public int clusterRadius = 30;
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class NamedSettlement
{
	public string name;
	public MapGeneratorDef mapGenerator;
	public PrefabDef prefab;
	public List<Hilliness> preferredHilliness;
	public List<BiomeDef> allowedBiomes;
	public IntVec3 forcedMapSize = IntVec3.Invalid;
	public List<PawnKindDef> guaranteedPawnKinds;
	public List<CharacterDef> guaranteedCharacters;
	public List<SettlementTrader> traders;
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class SettlementTrader : IExposable
{
	public string traderName;
	public ThingDef characterDef;
	public TraderKindDef traderKind;

	public void ExposeData()
	{
		Scribe_Values.Look(ref traderName, "traderName");
		Scribe_Defs.Look(ref characterDef, "characterDef");
		Scribe_Defs.Look(ref traderKind, "traderKind");
	}
}
