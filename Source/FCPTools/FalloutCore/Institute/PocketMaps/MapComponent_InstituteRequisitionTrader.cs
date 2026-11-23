namespace FCP.Synths;

public class MapComponent_InstituteRequisitionTrader : MapComponent
{
    public MapComponent_InstituteRequisitionTrader(Map map) : base(map)
    {
    }

    public override void FinalizeInit()
    {
        base.FinalizeInit();

        foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned)
        {
            if (pawn.kindDef != SynthDefOf.FCP_Pawnkind_Institute_RequisitionOfficer) continue;

            PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn, true);
            pawn.trader.traderKind = SynthDefOf.FCP_TraderKind_Institute_Requisition;
            pawn.mindState.wantsToTradeWithColony = true;
        }
    }
}
