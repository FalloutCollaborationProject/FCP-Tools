using HarmonyLib;
using RimWorld;
using Verse;
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace FCP.Core;

/// <summary>
/// Added Utility for defining if a pawn is purchasable from a trader.
/// </summary>
public class PawnKindProperties : DefModExtension
{
    public bool purchasableFromTrader = false;

    public static PawnKindProperties Get(Def def)
    {
        return def.GetModExtension<PawnKindProperties>();
    }
}

[HarmonyPatch]
public static class RecruitFromTrader_Patches
{
    /// <summary>
    /// Makes it so that custom pawnKinds can be sold as 'slaves'
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(TraderCaravanUtility), "GetTraderCaravanRole")]
    public static void TraderCaravanUtility_GetTraderCaravanRole_Postfix(Pawn p, ref TraderCaravanRole __result)
    {
        var props = PawnKindProperties.Get(p.kindDef);
        if (props != null && props.purchasableFromTrader)
        {
            __result = TraderCaravanRole.Chattel;
        }
    }

    /// <summary>
    /// Ensures recruit pawn kinds are not sold into slavery
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Pawn_GuestTracker), "RandomizeJoinStatus")]
    public static void Pawn_GuestTracker_RandomizeJoinStatus_Postfix(ref Pawn ___pawn, ref JoinStatus ___joinStatus)
    {
        if (___joinStatus != JoinStatus.JoinAsColonist && CanRecruit(___pawn))
        {
            ___joinStatus = JoinStatus.JoinAsColonist;
        }
    }

    /// <summary>
    /// Remove the thought as recruits are not slaves
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Pawn), "PreTraded")]
    public static void Pawn_PreTraded_Postfix(ref Pawn __instance)
    {
        if (CanRecruit(__instance))
        {
            var moodNeed = __instance.needs.mood;
            moodNeed?.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.FreedFromSlavery);
        }
    }

    /// <summary>
    /// Added Mod Extension
    /// </summary>
    public static bool CanRecruit(Pawn pawn)
    {
        var props = PawnKindProperties.Get(pawn.kindDef);
        if (props != null && props.purchasableFromTrader)
        {
            return true;
        }

        return false;
    }
}
