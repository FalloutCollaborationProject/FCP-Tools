using HarmonyLib;

namespace FCP.Ranks;

[HarmonyPatch(typeof(Pawn_HealthTracker), "MakeDowned")]
public static class Pawn_HealthTracker_MakeDowned_Patch
{
    [HarmonyPostfix]
    public static void Postfix(Pawn ___pawn)
    {
        if (___pawn == null)
            return;

        GameComponent_PawnRanks comp = Current.Game.GetComponent<GameComponent_PawnRanks>();
        if (comp == null)
            return;

        foreach (KeyValuePair<Pawn, PawnRankData> entry in comp.AllTracked.ToList())
        {
            if (entry.Value.mentor != ___pawn || entry.Key == null || entry.Key.Dead)
                continue;

            Pawn mentee = entry.Key;
            if (mentee.health != null && !mentee.health.hediffSet.HasHediff(RankHediffDefOf.FCP_Hediff_MentorDown_Duty))
                mentee.health.AddHediff(RankHediffDefOf.FCP_Hediff_MentorDown_Duty);

            entry.Value.AddHistory("FCP_Rank_History_MentorDown".Translate(___pawn.LabelShortCap));

            Messages.Message("FCP_Rank_MentorDown_Message".Translate(mentee.LabelShortCap, ___pawn.LabelShortCap), mentee, MessageTypeDefOf.NegativeEvent);
        }
    }
}

[HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
public static class Pawn_Kill_MentorDied_Patch
{
    [HarmonyPostfix]
    public static void Postfix(Pawn __instance)
    {
        if (__instance == null)
            return;

        GameComponent_PawnRanks comp = Current.Game.GetComponent<GameComponent_PawnRanks>();
        if (comp == null)
            return;

        foreach (KeyValuePair<Pawn, PawnRankData> entry in comp.AllTracked.ToList())
        {
            if (entry.Value.mentor != __instance || entry.Key == null || entry.Key.Dead)
                continue;

            entry.Value.mentor = null;
            entry.Value.AddHistory("FCP_Rank_History_MentorDied".Translate(__instance.LabelShortCap));
            entry.Key.needs?.mood?.thoughts.memories.TryGainMemory(RankThoughtDefOf.FCP_Thought_MentorDied);
        }
    }
}
