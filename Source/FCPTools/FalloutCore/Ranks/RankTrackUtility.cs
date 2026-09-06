using FCP.Enlist;

namespace FCP.Ranks;

public static class RankTrackUtility
{
    public const string CombatDiscipline = "Combat";
    public const string ScholarlyDiscipline = "Scholarly";

    public static bool IsMilitaryTier(RankTierDef tier)
    {
        return tier != null && tier.discipline != ScholarlyDiscipline;
    }

    public static bool MeetsRequirements(Pawn pawn, PawnRankData data, RankTierDef candidate)
    {
        if (pawn == null || data == null || candidate == null)
            return false;

        if (!IsFactionActive(data.track))
            return false;

        if (AlreadyHoldsVanillaRank(pawn, data, candidate))
            return true;

        int daysInRank = (Find.TickManager.TicksGame - data.rankStartTick) / GenDate.TicksPerDay;
        if (daysInRank < candidate.minDaysInRank)
            return false;

        if (!EnlistUtils.PawnSatisfiesSkillRequirements(pawn, candidate.skillRequirements))
            return false;

        if (data.squiresTrained < candidate.minSquiresTrained)
            return false;

        if (data.missionsCompleted < candidate.minMissionsCompleted)
            return false;

        return true;
    }

    private static bool AlreadyHoldsVanillaRank(Pawn pawn, PawnRankData data, RankTierDef tier)
    {
        Faction grantingFaction = GetOwningFaction(data.track);
        if (tier.correspondingRoyalTitle != null && ModsConfig.RoyaltyActive && pawn.royalty != null && grantingFaction != null)
        {
            RoyalTitleDef current = pawn.royalty.GetCurrentTitle(grantingFaction);
            if (current != null && current.seniority >= tier.correspondingRoyalTitle.seniority)
                return true;
        }

        if (tier.correspondingIdeoRole != null && ModsConfig.IdeologyActive && pawn.Ideo != null)
        {
            if (pawn.Ideo.GetPrecept(tier.correspondingIdeoRole) is Precept_Role role && role.IsAssigned(pawn))
                return true;
        }

        return false;
    }

    private static Faction GetOwningFaction(RankTrackDef track)
    {
        FactionDef factionDef = track?.owningFaction;
        return factionDef == null ? null : Find.FactionManager.FirstFactionOfDef(factionDef);
    }

    private static IEnumerable<RankTierDef> AllTiersInTrack(RankTrackDef track)
    {
        if (track?.rootTier == null)
            yield break;

        var visited = new HashSet<RankTierDef>();
        var stack = new Stack<RankTierDef>();
        stack.Push(track.rootTier);

        while (stack.Count > 0)
        {
            RankTierDef tier = stack.Pop();
            if (tier == null || !visited.Add(tier))
                continue;

            yield return tier;

            if (tier.nextTierOptions != null)
                foreach (RankTierDef next in tier.nextTierOptions)
                    stack.Push(next);
        }
    }

    public static RankTierDef FindPreviousTier(RankTrackDef track, RankTierDef tier)
    {
        if (track == null || tier == null)
            return null;

        return AllTiersInTrack(track).FirstOrDefault(t => t.nextTierOptions != null && t.nextTierOptions.Contains(tier));
    }

    public static RankTrackDef FindTrackForTier(RankTierDef tier)
    {
        if (tier == null)
            return null;

        return DefDatabase<RankTrackDef>.AllDefsListForReading.FirstOrDefault(track => AllTiersInTrack(track).Contains(tier));
    }

    public static string GetPathName(RankTrackDef track, RankTierDef tier)
    {
        for (RankTierDef current = tier; current != null; current = FindPreviousTier(track, current))
        {
            if (!current.pathName.NullOrEmpty())
                return current.pathName;
        }

        return null;
    }

    public static int GetTierDepth(RankTrackDef track, RankTierDef tier)
    {
        int depth = 0;
        for (RankTierDef current = tier; current != null; current = FindPreviousTier(track, current))
            depth++;

        return depth;
    }

    public static bool IsSeniorTier(RankTrackDef track, RankTierDef inspector, RankTierDef target)
    {
        if (inspector == null || target == null || inspector == target)
            return false;

        if (ModsConfig.RoyaltyActive && inspector.correspondingRoyalTitle != null && target.correspondingRoyalTitle != null)
            return inspector.correspondingRoyalTitle.seniority > target.correspondingRoyalTitle.seniority;

        return GetTierDepth(track, inspector) > GetTierDepth(track, target);
    }

    public static bool CanInspect(PawnRankData data)
    {
        return data?.currentTier != null && data.currentTier != data.track?.rootTier && IsMilitaryTier(data.currentTier);
    }

    public static bool IsFactionActive(RankTrackDef track)
    {
        if (track?.owningFaction == null)
            return true;

        Faction faction = GetOwningFaction(track);
        if (faction == null)
            return false;

        return WorldEnlistTracker.Instance?.EnlistedFactions().Contains(faction) == true;
    }

    public static void SuspendHediffsForFaction(Faction faction)
    {
        FactionExtension_RankTracks extension = faction?.def?.GetModExtension<FactionExtension_RankTracks>();
        if (extension?.rankTracks == null)
            return;

        GameComponent_PawnRanks comp = Current.Game.GetComponent<GameComponent_PawnRanks>();
        if (comp == null)
            return;

        foreach (KeyValuePair<Pawn, PawnRankData> entry in comp.AllTracked)
        {
            if (!extension.rankTracks.Contains(entry.Value.track))
                continue;

            HediffDef grantHediff = entry.Value.currentTier?.grantHediff;
            if (grantHediff == null || entry.Key?.health == null)
                continue;

            Hediff existing = entry.Key.health.hediffSet.hediffs.FirstOrDefault(h => h.def == grantHediff);
            if (existing != null)
                entry.Key.health.RemoveHediff(existing);
        }
    }

    public static void ResumeHediffsForFaction(Faction faction)
    {
        FactionExtension_RankTracks extension = faction?.def?.GetModExtension<FactionExtension_RankTracks>();
        if (extension?.rankTracks == null)
            return;

        GameComponent_PawnRanks comp = Current.Game.GetComponent<GameComponent_PawnRanks>();
        if (comp == null)
            return;

        foreach (KeyValuePair<Pawn, PawnRankData> entry in comp.AllTracked)
        {
            if (!extension.rankTracks.Contains(entry.Value.track))
                continue;

            HediffDef grantHediff = entry.Value.currentTier?.grantHediff;
            if (grantHediff == null || entry.Key?.health == null)
                continue;

            if (!entry.Key.health.hediffSet.HasHediff(grantHediff))
                entry.Key.health.AddHediff(grantHediff);
        }
    }

    public static void DemoteAllForFaction(Faction faction, string reasonTextKey)
    {
        FactionExtension_RankTracks extension = faction?.def?.GetModExtension<FactionExtension_RankTracks>();
        if (extension?.rankTracks == null)
            return;

        GameComponent_PawnRanks comp = Current.Game.GetComponent<GameComponent_PawnRanks>();
        if (comp == null)
            return;

        foreach (KeyValuePair<Pawn, PawnRankData> entry in comp.AllTracked.ToList())
        {
            if (extension.rankTracks.Contains(entry.Value.track))
                Demote(entry.Key, entry.Value, reasonTextKey);
        }
    }

    public static void Promote(Pawn pawn, PawnRankData data, RankTierDef newTier)
    {
        if (pawn == null || data == null || newTier == null)
            return;

        bool isInitial = data.currentTier == null;

        if (!isInitial && data.currentTier.grantHediff != null && pawn.health != null)
        {
            Hediff existing = pawn.health.hediffSet.hediffs.FirstOrDefault(h => h.def == data.currentTier.grantHediff);
            if (existing != null)
                pawn.health.RemoveHediff(existing);
        }

        data.currentTier = newTier;
        data.rankStartTick = Find.TickManager.TicksGame;

        if (newTier.grantHediff != null && pawn.health != null && !pawn.health.hediffSet.HasHediff(newTier.grantHediff))
            pawn.health.AddHediff(newTier.grantHediff);

        SyncVanillaRank(pawn, data, newTier);

        if (isInitial)
        {
            data.AddHistory("FCP_Rank_History_Enrolled".Translate(newTier.label));
        }
        else
        {
            if (data.mentor != null && !data.mentor.Dead)
            {
                PawnRankData mentorData = Current.Game.GetComponent<GameComponent_PawnRanks>()?.GetRankData(data.mentor);
                if (mentorData != null)
                    mentorData.squiresTrained++;
            }

            string historyText = "FCP_Rank_Promoted_Text".Translate(pawn.LabelShortCap, newTier.label);
            data.AddHistory(historyText);

            Find.LetterStack.ReceiveLetter(
                "FCP_Rank_Promoted_Title".Translate(newTier.LabelCap),
                historyText,
                LetterDefOf.PositiveEvent,
                pawn);

            HoldPromotionCeremony(pawn, data);
        }
    }

    private const float CeremonyWitnessRadius = 24f;

    private static void HoldPromotionCeremony(Pawn pawn, PawnRankData data)
    {
        pawn.needs?.mood?.thoughts.memories.TryGainMemory(RankThoughtDefOf.FCP_Thought_PromotionCeremony);

        if (pawn.Map == null)
            return;

        foreach (Pawn other in pawn.Map.mapPawns.FreeColonists)
        {
            if (other == pawn)
                continue;

            PawnRankData otherData = Current.Game.GetComponent<GameComponent_PawnRanks>()?.GetRankData(other);
            if (otherData == null || otherData.track != data.track)
                continue;

            if (!other.Position.InHorDistOf(pawn.Position, CeremonyWitnessRadius))
                continue;

            other.needs?.mood?.thoughts.memories.TryGainMemory(RankThoughtDefOf.FCP_Thought_WitnessedPromotion);
        }
    }

    private static void SyncVanillaRank(Pawn pawn, PawnRankData data, RankTierDef tier)
    {
        Faction grantingFaction = GetOwningFaction(data.track);
        if (tier.correspondingRoyalTitle != null && ModsConfig.RoyaltyActive && pawn.royalty != null && grantingFaction != null)
        {
            RoyalTitleDef current = pawn.royalty.GetCurrentTitle(grantingFaction);
            if (current == null || current.seniority < tier.correspondingRoyalTitle.seniority)
                pawn.royalty.SetTitle(grantingFaction, tier.correspondingRoyalTitle, grantRewards: true, rewardsOnlyForNewestTitle: false, sendLetter: false);
        }

        if (tier.correspondingIdeoRole != null && ModsConfig.IdeologyActive && pawn.Ideo != null)
        {
            if (pawn.Ideo.GetPrecept(tier.correspondingIdeoRole) is Precept_Role role && !role.IsAssigned(pawn))
                role.Assign(pawn, true);
        }
    }

    public static void Demote(Pawn pawn, PawnRankData data, string reasonTextKey)
    {
        if (pawn == null || data == null || data.currentTier == null)
            return;

        RankTierDef demotedFrom = data.currentTier;
        RankTierDef previous = FindPreviousTier(data.track, demotedFrom);
        if (previous == null)
            return;

        if (demotedFrom.grantHediff != null && pawn.health != null)
        {
            Hediff existing = pawn.health.hediffSet.hediffs.FirstOrDefault(h => h.def == demotedFrom.grantHediff);
            if (existing != null)
                pawn.health.RemoveHediff(existing);
        }

        data.currentTier = previous;
        data.rankStartTick = Find.TickManager.TicksGame;

        if (previous.grantHediff != null && pawn.health != null && !pawn.health.hediffSet.HasHediff(previous.grantHediff))
            pawn.health.AddHediff(previous.grantHediff);

        SyncVanillaRankDemotion(pawn, data, demotedFrom, previous);

        string historyText = reasonTextKey.Translate(pawn.LabelShortCap, demotedFrom.label, previous.label);
        data.AddHistory(historyText);

        Find.LetterStack.ReceiveLetter(
            "FCP_Rank_Demoted_Title".Translate(demotedFrom.LabelCap),
            historyText,
            LetterDefOf.NegativeEvent,
            pawn);
    }

    private static void SyncVanillaRankDemotion(Pawn pawn, PawnRankData data, RankTierDef demotedFrom, RankTierDef newTier)
    {
        Faction grantingFaction = GetOwningFaction(data.track);
        if (newTier.correspondingRoyalTitle != null && ModsConfig.RoyaltyActive && pawn.royalty != null && grantingFaction != null)
            pawn.royalty.SetTitle(grantingFaction, newTier.correspondingRoyalTitle, grantRewards: false, rewardsOnlyForNewestTitle: false, sendLetter: false);

        if (demotedFrom.correspondingIdeoRole != null && demotedFrom.correspondingIdeoRole != newTier.correspondingIdeoRole
            && ModsConfig.IdeologyActive && pawn.Ideo != null)
        {
            if (pawn.Ideo.GetPrecept(demotedFrom.correspondingIdeoRole) is Precept_Role role && role.IsAssigned(pawn))
                role.Unassign(pawn, true);
        }
    }
}
