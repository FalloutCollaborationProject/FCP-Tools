using HarmonyLib;
// ReSharper disable InconsistentNaming

namespace FCP.Factions;

[HarmonyPatch(typeof(Faction), nameof(Faction.LeaderTitle), MethodType.Getter)]
public class Faction_LeaderTitle_FlavorOverride_Patch
{
    public bool Prefix(Faction __instance, ref string __result)
    {
        var ext = __instance.def?.GetModExtension<FactionExtension_FlavorOverride>();

        if (ext?.preferFactionLeaderTitle != true)
            return true;

        if (__instance.leader.gender == Gender.Female && !__instance.def.leaderTitleFemale.NullOrEmpty())
            __result = __instance.def.leaderTitleFemale;
        else
            __result = __instance.def.leaderTitle;

        return false;
    }
}