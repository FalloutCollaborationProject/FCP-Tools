using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace FCP.Core.RadiantQuests;

public class QuestNode_GenerateSiteSimple : QuestNode
{
	[NoTranslate]
	public SlateRef<string> storeAs;
	public SlateRef<PlanetTile> tile;
	public SlateRef<Faction> faction;
	public SlateRef<string> factionDefName;
	public SlateRef<float?> threatPoints;
	public List<SitePartDef> sitePartDefs;

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		PlanetTile tileValue = tile.GetValue(slate);
		if (!tileValue.Valid)
		{
			Log.Warning("[QuestNode_GenerateSiteSimple] Invalid tile");
			return;
		}
		Faction factionValue = faction.GetValue(slate);
		if (factionValue == null)
		{
			string defName = factionDefName.GetValue(slate);
			if (!string.IsNullOrEmpty(defName))
			{
				FactionDef factionDef = DefDatabase<FactionDef>.GetNamedSilentFail(defName);
				if (factionDef != null)
				{
					factionValue = Find.FactionManager.FirstFactionOfDef(factionDef);
				}
			}
			if (factionValue == null)
			{
				Log.Warning("[QuestNode_GenerateSiteSimple] Could not resolve faction");
				return;
			}
		}
		float points = threatPoints.GetValue(slate) ?? 1000f;
		
		List<SitePartDefWithParams> list = new List<SitePartDefWithParams>();
		if (sitePartDefs != null)
		{
			for (int i = 0; i < sitePartDefs.Count; i++)
			{
				SitePartParams partParams = new SitePartParams();
				partParams.threatPoints = points;
				SitePartDefWithParams partWithParams = new SitePartDefWithParams(sitePartDefs[i], partParams);
				list.Add(partWithParams);
			}
		}
		
		Site site = QuestGen_Sites.GenerateSite(list, tileValue, factionValue);
		site.SetFaction(factionValue);
		slate.Set(storeAs.GetValue(slate), site);
	}
}
