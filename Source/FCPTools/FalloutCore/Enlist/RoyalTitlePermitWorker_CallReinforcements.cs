namespace FCP.Enlist;

public class RoyalTitlePermitWorker_CallReinforcements : RoyalTitlePermitWorker
{
    public static bool CanCall(Pawn caller, Faction faction, RoyalTitlePermitDef permit)
    {
        return caller.royalty != null
               && permit != null
               && caller.royalty.HasPermit(permit, faction)
               && caller.royalty.GetFavor(faction) >= permit.royalAid.favorCost;
    }

    public static bool TryCall(Pawn caller, Faction faction, FactionEnlistOptionsDef options, RoyalTitlePermitDef permit, IntVec3 cell)
    {
        if (!CanCall(caller, faction, permit))
            return false;
        if (!WorldEnlistTracker.Instance.CanCallReinforcementFrom(faction, options))
            return false;

        caller.royalty.TryRemoveFavor(faction, permit.royalAid.favorCost);
        caller.royalty.GetPermit(permit, faction).Notify_Used();
        WorldEnlistTracker.Instance.CallReinforcement(faction, options, caller, cell);
        return true;
    }

    public static RoyalTitlePermitDef FindPermitFor(Faction faction)
    {
        return DefDatabase<RoyalTitlePermitDef>.AllDefsListForReading
            .FirstOrDefault(p => p.faction == faction.def && p.workerClass == typeof(RoyalTitlePermitWorker_CallReinforcements));
    }
}
