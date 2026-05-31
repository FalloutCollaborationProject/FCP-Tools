using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace FCP.Core.GenSteps;

public class GenStep_SitePartPawns : GenStep
{
	public FloatRange defaultPointsRange = new FloatRange(300f, 2500f);

	public override int SeedPart => 341176078;

	public override void Generate(Map map, GenStepParams parms)
	{
		Log.Message("[GenStep_SitePartPawns] Generate called for map " + map.uniqueID);
		if (parms.sitePart == null || parms.sitePart.def == null)
		{
			Log.Warning("[GenStep_SitePartPawns] No site part in params");
			return;
		}
		Log.Message("[GenStep_SitePartPawns] Site part: " + parms.sitePart.def.defName);
		if (!parms.sitePart.def.wantsThreatPoints)
		{
			Log.Warning("[GenStep_SitePartPawns] Site part does not want threat points");
			return;
		}
		Faction faction = parms.sitePart.site.Faction;
		Log.Message("[GenStep_SitePartPawns] Site faction: " + (faction != null ? faction.Name : "NULL"));
		if (faction != null)
		{
			Log.Message("[GenStep_SitePartPawns] Faction hostile to player: " + faction.HostileTo(Faction.OfPlayer));
		}
		if (faction == null || !faction.HostileTo(Faction.OfPlayer))
		{
			Log.Warning("[GenStep_SitePartPawns] No hostile faction - cannot spawn pawns");
			return;
		}
		float points = parms.sitePart.parms.threatPoints;
		if (points <= 0f)
		{
			points = defaultPointsRange.RandomInRange;
		}
		Log.Message($"[GenStep_SitePartPawns] Generating {points} points of pawns for {faction.Name}");
		PawnGroupMakerParms groupParms = new PawnGroupMakerParms();
		groupParms.groupKind = PawnGroupKindDefOf.Combat;
		groupParms.tile = map.Tile;
		groupParms.faction = faction;
		groupParms.points = points;
		groupParms.generateFightersOnly = true;
		IEnumerable<Pawn> enumerable = PawnGroupMakerUtility.GeneratePawns(groupParms);
		List<Pawn> list = new List<Pawn>();
		foreach (Pawn p in enumerable)
		{
			list.Add(p);
		}
		Log.Message($"[GenStep_SitePartPawns] Generated {list.Count} pawns");
		if (list.Count == 0)
		{
			return;
		}
		IntVec3 center = CellFinderLoose.TryFindCentralCell(map, 8, 15, (IntVec3 c) => c.Standable(map) && !c.Roofed(map));
		if (!center.IsValid)
		{
			center = CellFinder.RandomCell(map);
		}
		for (int i = 0; i < list.Count; i++)
		{
			IntVec3 loc = CellFinder.RandomSpawnCellForPawnNear(center, map, 10);
			GenSpawn.Spawn(list[i], loc, map);
			list[i].mindState.Active = false;
		}
		Log.Message($"[GenStep_SitePartPawns] Spawned {list.Count} pawns at {center}, created lord");
		LordMaker.MakeNewLord(faction, new LordJob_DefendPoint(center, 28f), map, list);
	}
}