using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

// ReSharper disable InconsistentNaming

namespace FCP.Factions;

[HarmonyPatch]
public static class MaxTitlePermitPatches
{

    [HarmonyPostfix]
    [HarmonyPatch(typeof(RoyalTitlePermitDef), nameof(RoyalTitlePermitDef.AvailableForPawn))]
    private static void AvailableForPawnPostfix(ref bool __result, RoyalTitlePermitDef __instance, Pawn pawn, Faction faction)
    {
        if (__result == false)
            return;
        
        var permitExtension = __instance.GetModExtension<MaxTitlePermitExtension>();
        if (permitExtension == null) 
            return;
        
        var currentTitle = pawn.royalty.GetCurrentTitle(faction);

        if (currentTitle.seniority < __instance.minTitle.seniority ||
            currentTitle.seniority > permitExtension.maxTitle.seniority)
        {
            __result = false;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(RoyalTitleAwardWorker), nameof(RoyalTitleAwardWorker.DoAward))]
    [HarmonyPatch(typeof(RoyalTitleAwardWorker_Instant), nameof(RoyalTitleAwardWorker.DoAward))]
    private static void DoAwardPostfix(Pawn pawn, Faction faction, RoyalTitleDef currentTitle, RoyalTitleDef newTitle)
    {
        foreach (var permit in pawn.royalty.AllFactionPermits.ToList())
        {
            var permitExtension = permit.Permit.GetModExtension<MaxTitlePermitExtension>();

            if (newTitle.seniority > permitExtension?.maxTitle.seniority)
            {
                Messages.Message("FCP_MessagePermitLostOnPromotion".Translate(pawn, currentTitle.GetLabelFor(pawn), permit.Permit),
                    MessageTypeDefOf.NeutralEvent);
                pawn.royalty.AllFactionPermits.Remove(permit);
            }
        }
    }
}
    
