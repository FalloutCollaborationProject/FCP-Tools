using HarmonyLib;

namespace FCP.Factions;

[HarmonyPatch(typeof(PawnGroupKindWorker_Trader), "GeneratePawns")]
public static class PawnGroupKindWorker_Trader_Patches
{
    public static void Prefix(PawnGroupMakerParms parms, PawnGroupMaker groupMaker)
    {
        if (groupMaker is not GroupMakerWithTraderKind groupMakerWithTrader)
        {
            return;
        }
        if (groupMakerWithTrader.traderKinds.Empty())
        {
            Log.Warning($"FCPTools : A GroupMakerWithTraderKind was defined without any traderKindDefs assigned");
            return;
        }
        
        parms.traderKind = groupMakerWithTrader.traderKinds.RandomElement();
    }
}