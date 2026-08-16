using HarmonyLib;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace FCP.PocketMaps;

[HarmonyPatch(typeof(JobGiver_ExitMap), "TryGiveJob")]
public static class JobGiver_ExitMap_StationedResidents_Patch
{
    public static bool Prefix(Pawn pawn, ref Job __result)
    {
        if (pawn.GetLord()?.LordJob is LordJob_StationedResidents)
        {
            __result = null;
            return false;
        }
        return true;
    }
}
