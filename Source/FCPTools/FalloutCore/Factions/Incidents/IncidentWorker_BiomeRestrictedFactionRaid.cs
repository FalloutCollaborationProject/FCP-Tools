using System.Linq;
using RimWorld;
using Verse;

namespace FCP.Factions;

public class IncidentWorker_BiomeRestrictedFactionRaid : IncidentWorker_RaidEnemy
{
	private BiomeRestrictedRaidExtension Extension => def.GetModExtension<BiomeRestrictedRaidExtension>();

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (Extension?.allowedBiomes == null || Extension.targetFaction == null)
			return false;

		if (parms.target is not Map map || !Extension.allowedBiomes.Contains(map.Biome.defName))
			return false;

		FactionDef factionDef = DefDatabase<FactionDef>.GetNamed(Extension.targetFaction, false);
		if (factionDef == null)
			return false;

		Faction faction = Find.FactionManager.AllFactionsListForReading
			.FirstOrDefault(f => f.def == factionDef && !f.defeated);
		if (faction == null)
			return false;

		parms.faction = faction;
		return base.CanFireNowSub(parms);
	}
}
