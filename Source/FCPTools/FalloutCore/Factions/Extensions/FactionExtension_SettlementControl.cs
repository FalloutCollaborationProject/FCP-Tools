using System.Collections.Generic;
using System.Xml;
using FCP.Core;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FCP.Factions;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class PawnKindCount : IExposable
{
	public PawnKindDef pawnKindDef;
	public int count = 1;

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "pawnKindDef", xmlRoot.Name);
		count = ParseHelper.FromString<int>(xmlRoot.InnerText);
	}

	public void ExposeData()
	{
		Scribe_Defs.Look(ref pawnKindDef, "pawnKindDef");
		Scribe_Values.Look(ref count, "count", 1);
	}
}

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
	public List<PawnKindCount> guaranteedPawnKinds;
	public List<CharacterDef> guaranteedCharacters;
	public List<SettlementTrader> traders;

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		foreach (XmlNode childNode in xmlRoot.ChildNodes)
		{
			if (childNode.Name == "name")
			{
				name = childNode.InnerText;
			}
			else if (childNode.Name == "mapGenerator")
			{
				DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "mapGenerator", childNode.InnerText);
			}
			else if (childNode.Name == "prefab")
			{
				DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "prefab", childNode.InnerText);
			}
			else if (childNode.Name == "forcedMapSize")
			{
				forcedMapSize = ParseHelper.FromString<IntVec3>(childNode.InnerText);
			}
			else if (childNode.Name == "guaranteedPawnKinds")
			{
				guaranteedPawnKinds = DirectXmlToObject.ObjectFromXml<List<PawnKindCount>>(childNode, true);
			}
			else if (childNode.Name == "preferredHilliness")
			{
				preferredHilliness = DirectXmlToObject.ObjectFromXml<List<Hilliness>>(childNode, true);
			}
			else if (childNode.Name == "allowedBiomes")
			{
				allowedBiomes = DirectXmlToObject.ObjectFromXml<List<BiomeDef>>(childNode, true);
			}
			else if (childNode.Name == "guaranteedCharacters")
			{
				guaranteedCharacters = DirectXmlToObject.ObjectFromXml<List<CharacterDef>>(childNode, true);
			}
			else if (childNode.Name == "traders")
			{
				traders = DirectXmlToObject.ObjectFromXml<List<SettlementTrader>>(childNode, true);
			}
		}
	}
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
