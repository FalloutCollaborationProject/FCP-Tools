using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FCP.Factions;

public class SitePartWorker_FactionOutpost : SitePartWorker
{
	public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
	{
		string baseLabel = base.GetPostProcessedThreatLabel(site, sitePart);
		if (site.Faction != null)
			return baseLabel + " (" + site.Faction.Name + ")";
		return baseLabel;
	}
}
