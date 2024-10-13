using HarmonyLib;

namespace FCP.Factions;

[UsedImplicitly]
public class BannedArrivalModesExtension : DefModExtension
{
    public List<PawnsArrivalModeDef> arrivalModes;
}

[HarmonyPatch(typeof(PawnsArrivalModeWorker), nameof(PawnsArrivalModeWorker.CanUseWith))]
public static class PawnsArrivalModeWorker_CanUseWith_Patches {
    public static void Postfix(IncidentParms parms, ref bool __result, PawnsArrivalModeDef ___def)
    {
        if (__result == false)
            return;

        var extension = parms.faction?.def.GetModExtension<BannedArrivalModesExtension>();
        if (extension != null && extension.arrivalModes.NotNullAndContains(___def))
        {
            __result = false;
        }
    }
}