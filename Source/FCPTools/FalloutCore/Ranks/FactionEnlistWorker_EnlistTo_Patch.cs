using FCP.Enlist;
using HarmonyLib;

namespace FCP.Ranks;

[HarmonyPatch(typeof(FactionEnlistWorker), nameof(FactionEnlistWorker.EnlistTo))]
public static class FactionEnlistWorker_EnlistTo_Patch
{
    [HarmonyPostfix]
    public static void Postfix(Faction toEnlist)
    {
        FactionExtension_RankTracks extension = toEnlist?.def?.GetModExtension<FactionExtension_RankTracks>();
        if (extension?.rankTracks == null)
            return;

        GameComponent_PawnRanks comp = Current.Game.GetComponent<GameComponent_PawnRanks>();
        if (comp == null)
            return;

        foreach (RankTrackDef track in extension.rankTracks)
        {
            foreach (Pawn pawn in PawnsFinder.AllMaps_FreeColonists)
            {
                comp.Enroll(pawn, track);
            }
        }

        RankTrackUtility.ResumeHediffsForFaction(toEnlist);
    }
}
