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
        if (parms.sitePart == null || parms.sitePart.def == null)
            return;

        if (!parms.sitePart.def.wantsThreatPoints)
            return;

        Faction faction = parms.sitePart.site.Faction;
        if (faction == null || !faction.HostileTo(Faction.OfPlayer))
            return;

        float points = parms.sitePart.parms.threatPoints;
        if (points <= 0f)
            points = defaultPointsRange.RandomInRange;

        PawnGroupMakerParms groupParms = new PawnGroupMakerParms
        {
            groupKind = PawnGroupKindDefOf.Combat,
            tile = map.Tile,
            faction = faction,
            points = points,
            generateFightersOnly = true,
        };

        List<Pawn> pawns = new List<Pawn>(PawnGroupMakerUtility.GeneratePawns(groupParms));
        if (pawns.Count == 0)
            return;

        IntVec3 center = CellFinderLoose.TryFindCentralCell(map, 8, 15, (IntVec3 c) => c.Standable(map) && !c.Roofed(map));
        if (!center.IsValid)
            center = CellFinder.RandomCell(map);

        for (int i = 0; i < pawns.Count; i++)
        {
            IntVec3 loc = CellFinder.RandomSpawnCellForPawnNear(center, map, 10);
            GenSpawn.Spawn(pawns[i], loc, map);
            pawns[i].mindState.Active = false;
        }

        LordMaker.MakeNewLord(faction, new LordJob_DefendPoint(center, 28f), map, pawns);
    }
}
