using System.Collections.Generic;
using Verse;

namespace FCP.Factions;

public class RandomSiteIncidentExtension : DefModExtension
{
	public string targetFaction;
	public string sitePartDef;
	public int durationDays = 15;
	public int maxActiveSites = 1;
	public bool requiresPlayerVisible = true;
	public int searchRadius = 30;
	public bool clustered = false;
	public int clusterRadius = 20;
	public NamedSettlement siteTemplate;
}
