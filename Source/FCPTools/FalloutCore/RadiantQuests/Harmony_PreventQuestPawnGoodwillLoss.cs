using HarmonyLib;
using RimWorld;
using Verse;

namespace FCP.Core.RadiantQuests;

[HarmonyPatch(typeof(Faction), nameof(Faction.Notify_MemberTookDamage))]
public static class Patch_Faction_Notify_MemberTookDamage
{
    static bool Prefix(Faction __instance, Pawn member, DamageInfo dinfo)
    {
        if (member?.IsQuestLodger() == true)
        {
            return false;
        }
        
        return true; // Run original method
    }
}

[HarmonyPatch(typeof(Faction), nameof(Faction.Notify_MemberDied))]
public static class Patch_Faction_Notify_MemberDied
{
    static bool Prefix(Faction __instance, Pawn member, DamageInfo? dinfo, bool wasWorldPawn, Map map)
    {
        if (member?.IsQuestLodger() == true)
        {
            return false;
        }
        
        return true;
    }
}
