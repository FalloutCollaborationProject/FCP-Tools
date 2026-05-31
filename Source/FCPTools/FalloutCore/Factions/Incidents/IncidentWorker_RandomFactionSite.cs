using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FCP.Factions;

public class IncidentWorker_RandomFactionSite : IncidentWorker
{
	private RandomSiteIncidentExtension Extension => def.GetModExtension<RandomSiteIncidentExtension>();
	
	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (!base.CanFireNowSub(parms))
			return false;
		
		if (Extension == null)
			return false;
		
		Faction faction = GetTargetFaction();
		if (faction == null)
			return false;
		
		if (Extension.requiresPlayerVisible && !faction.def.CanEverBeNonHostile)
			return false;
		
		int activeSites = CountActiveSites(faction);
		return activeSites < Extension.maxActiveSites;
	}
	
	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		if (Extension == null)
			return false;
		
		Faction faction = GetTargetFaction();
		if (faction == null)
			return false;
		
		if (!TryFindTile(faction, out int tile))
			return false;
		
		Site site = CreateSite(faction, tile);
		if (site == null)
			return false;
		
		Find.WorldObjects.Add(site);
		
		Find.LetterStack.ReceiveLetter(
			def.letterLabel,
			def.letterText,
			def.letterDef ?? LetterDefOf.NeutralEvent,
			site,
			faction);
		
		return true;
	}
	
	private Faction GetTargetFaction()
	{
		FactionDef factionDef = DefDatabase<FactionDef>.GetNamed(Extension.targetFaction, false);
		if (factionDef == null)
			return null;
		
		return Find.FactionManager.FirstFactionOfDef(factionDef);
	}
	
	private bool TryFindTile(Faction faction, out int tile)
	{
		tile = TileFinder.RandomSettlementTileFor(faction, false, null);
		return tile >= 0;
	}
	
	private Site CreateSite(Faction faction, int tile)
	{
		SitePartDef partDef = DefDatabase<SitePartDef>.GetNamed(Extension.sitePartDef, false);
		if (partDef == null)
			return null;
		
		Site site = (Site)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Site);
		site.Tile = tile;
		site.SetFaction(faction);
		
		SitePart part = new SitePart(site, partDef, partDef.Worker.GenerateDefaultParams(StorytellerUtility.DefaultSiteThreatPointsNow(), tile, faction));
		site.parts.Add(part);
		
		int durationTicks = Extension.durationDays * GenDate.TicksPerDay;
		site.GetComponent<TimeoutComp>()?.StartTimeout(durationTicks);
		
		RegisterSiteTemplate(site.Tile);
		
		return site;
	}
	
	private void RegisterSiteTemplate(int tile)
	{
		if (Extension.siteTemplate == null)
			return;
		
		GameComponent_FactionOutpostTemplates comp = Current.Game.GetComponent<GameComponent_FactionOutpostTemplates>();
		if (comp == null)
		{
			comp = new GameComponent_FactionOutpostTemplates(Current.Game);
			Current.Game.components.Add(comp);
		}
		
		comp.RegisterSite(tile, Extension.siteTemplate);
	}
	
	private int CountActiveSites(Faction faction)
	{
		int count = 0;
		List<WorldObject> allObjects = Find.WorldObjects.AllWorldObjects;
		
		for (int i = 0; i < allObjects.Count; i++)
		{
			Site site = allObjects[i] as Site;
			if (site != null && site.Faction == faction && !site.Destroyed)
				count++;
		}
		
		return count;
	}
	
	private static List<Settlement> tmpFactionBases = new List<Settlement>();
}
