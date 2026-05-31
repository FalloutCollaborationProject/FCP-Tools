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
	public SlateRef<int?> tile;
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
		int tileInt = tile.GetValue(slate) ?? -1;
		if (tileInt < 0)
		{
			Log.Warning("[QuestNode_GenerateSiteSimple] Invalid tile");
			return;
		}
		Faction factionValue = faction.GetValue(slate);
		if (factionValue == null)
		{
			string defName = factionDefName.GetValue(slate);
			Log.Message("[QuestNode_GenerateSiteSimple] Faction is null, trying factionDefName: " + defName);
			if (!string.IsNullOrEmpty(defName))
			{
				FactionDef factionDef = DefDatabase<FactionDef>.GetNamedSilentFail(defName);
				if (factionDef != null)
				{
					factionValue = Find.FactionManager.FirstFactionOfDef(factionDef);
					Log.Message("[QuestNode_GenerateSiteSimple] Resolved faction from defName: " + (factionValue != null ? factionValue.Name : "NULL"));
				}
			}
			if (factionValue == null)
			{
				Log.Warning("[QuestNode_GenerateSiteSimple] Could not resolve faction");
				return;
			}
		}
		float points = threatPoints.GetValue(slate) ?? 1000f;
		Log.Message("[QuestNode_GenerateSiteSimple] Generating site at tile " + tileInt + " for " + factionValue.Name + " with " + points + " threat points");
		
		List<SitePartDefWithParams> list = new List<SitePartDefWithParams>();
		if (sitePartDefs != null)
		{
			for (int i = 0; i < sitePartDefs.Count; i++)
			{
				SitePartParams partParams = new SitePartParams();
				partParams.threatPoints = points;
				SitePartDefWithParams partWithParams = new SitePartDefWithParams(sitePartDefs[i], partParams);
				list.Add(partWithParams);
				Log.Message("[QuestNode_GenerateSiteSimple] Added site part: " + sitePartDefs[i].defName + " with " + points + " threat points");
			}
		}
		
		Site site = QuestGen_Sites.GenerateSite(list, tileInt, factionValue);
		site.SetFaction(factionValue);
		Log.Message("[QuestNode_GenerateSiteSimple] Generated site with " + site.parts.Count + " parts");
		Log.Message("[QuestNode_GenerateSiteSimple] Site faction: " + (site.Faction != null ? site.Faction.Name : "NULL") + ", hostile: " + (site.Faction != null && site.Faction.HostileTo(Faction.OfPlayer)));
		if (site.parts.Count > 0)
		{
			Log.Message("[QuestNode_GenerateSiteSimple] First part: " + site.parts[0].def.defName + ", threatPoints: " + site.parts[0].parms.threatPoints);
		}
		slate.Set(storeAs.GetValue(slate), site);
	}
}
