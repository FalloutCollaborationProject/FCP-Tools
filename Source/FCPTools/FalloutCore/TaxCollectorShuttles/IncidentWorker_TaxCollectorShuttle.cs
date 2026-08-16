using FCP.Core.Shuttles;
using FCP.Factions;

namespace FCP.TaxCollectorShuttles;

public class IncidentWorker_TaxCollectorShuttle : IncidentWorker_CaravanArrivalTaxCollector
{
    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        if (!TryResolveParmsGeneral(parms))
            return false;

        var shuttleExtension = parms.faction?.def.GetModExtension<FactionModExtension>();
        if (shuttleExtension?.transportShipDef != null && parms.target is Map map)
        {
            return ExecuteShuttleArrival(parms, map, shuttleExtension);
        }

        return base.TryExecuteWorker(parms);
    }

    private bool ExecuteShuttleArrival(IncidentParms parms, Map map, FactionModExtension shuttleExtension)
    {
        if (!DropCellFinder.FindSafeLandingSpot(out parms.spawnCenter, parms.faction, map,
            size: shuttleExtension.transportShipDef.shipThing.size))
        {
            parms.spawnCenter = CellFinder.RandomEdgeCell(map);
        }

        List<Pawn> pawns = new List<Pawn>();
        foreach (Pawn p in PawnGroupMakerUtility.GeneratePawns(
            IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDef, parms, ensureCanGenerateAtLeastOnePawn: true)))
        {
            pawns.Add(p);
        }

        if (pawns.Count == 0)
            return false;

        ShuttleArrivalAction.Arrive(pawns.Cast<Thing>().ToList(), map, parms.faction, shuttleExtension, parms.spawnCenter);
        SendLetter(parms, pawns, parms.traderKind);

        return true;
    }
}
