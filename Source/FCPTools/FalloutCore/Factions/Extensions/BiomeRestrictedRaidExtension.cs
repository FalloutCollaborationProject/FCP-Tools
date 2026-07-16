using System.Collections.Generic;
using Verse;

namespace FCP.Factions;

public class BiomeRestrictedRaidExtension : DefModExtension
{
	public List<string> allowedBiomes;
	public string targetFaction;
}
