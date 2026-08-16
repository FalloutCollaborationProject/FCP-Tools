using HarmonyLib;
using UnityEngine;
using Verse.AI;

namespace FCP.Ranks.Jobs;

[HarmonyPatch(typeof(Pawn), "GetGizmos")]
public static class Pawn_GetGizmos_Ranks_Patch
{
    private const int CooldownTicks = GenDate.TicksPerDay;
    private const string CombatDiscipline = "Combat";
    private const string ScholarlyDiscipline = "Scholarly";

    [HarmonyPostfix]
    public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
    {
        foreach (Gizmo gizmo in __result)
            yield return gizmo;

        if (!__instance.IsColonistPlayerControlled || __instance.Downed || __instance.InMentalState)
            yield break;

        PawnRankData data = Current.Game.GetComponent<GameComponent_PawnRanks>()?.GetRankData(__instance);
        if (data == null || !RankTrackUtility.IsFactionActive(data.track))
            yield break;

        string discipline = data.currentTier?.discipline;
        if (discipline == CombatDiscipline)
        {
            string label = data.track.drillLabel ?? "FCP_Rank_DrillGizmo".Translate().ToString();
            string desc = data.track.drillDesc ?? "FCP_Rank_DrillGizmoDesc".Translate().ToString();
            yield return BuildSessionGizmo(__instance, data, label, desc, RankJobDefOf.FCP_Job_RankDrilling, TexCommand.Attack, data.lastDrilledTick);
        }
        else if (discipline == ScholarlyDiscipline)
        {
            string label = data.track.studyLabel ?? "FCP_Rank_StudyGizmo".Translate().ToString();
            string desc = data.track.studyDesc ?? "FCP_Rank_StudyGizmoDesc".Translate().ToString();
            yield return BuildSessionGizmo(__instance, data, label, desc, RankJobDefOf.FCP_Job_RankStudying, TexCommand.GatherSpotActive, data.lastStudiedTick);
        }

        if (data.currentTier != null && data.currentTier.isCommanderTier)
        {
            if (data.currentTier.rallyHediff != null)
                yield return BuildRallyGizmo(__instance, data);

            yield return BuildFormUpGizmo(__instance, data);

            if (Current.Game.GetComponent<GameComponent_PawnRanks>()?.HasActiveFormation(__instance) == true)
                yield return BuildDismissGizmo(__instance, data);
        }

        if (RankTrackUtility.CanInspect(data))
        {
            yield return BuildInspectionGizmo(__instance, data);
            yield return BuildPunishmentGizmo(__instance, data);
        }
    }

    private static Command_Action BuildFormUpGizmo(Pawn pawn, PawnRankData data)
    {
        int cooldownTicks = data.currentTier.rallyCooldownDays * GenDate.TicksPerDay;

        Command_Action command = new Command_Action
        {
            defaultLabel = data.track.fallInLabel ?? "FCP_Rank_FormUpGizmo".Translate().ToString(),
            defaultDesc = data.track.fallInDesc != null ? string.Format(data.track.fallInDesc, data.track.collectiveNoun) : "FCP_Rank_FormUpGizmoDesc".Translate(data.track.collectiveNoun).ToString(),
            icon = TexCommand.SquadAttack,
            action = delegate
            {
                Current.Game.GetComponent<GameComponent_PawnRanks>()?.FormUp(pawn);
            }
        };

        if (data.lastFormUpTick >= 0 && Find.TickManager.TicksGame - data.lastFormUpTick < cooldownTicks)
            command.Disable("FCP_Rank_SessionOnCooldown".Translate());

        return command;
    }

    private static Command_Action BuildDismissGizmo(Pawn pawn, PawnRankData data)
    {
        return new Command_Action
        {
            defaultLabel = data.track.dismissLabel ?? "FCP_Rank_DismissGizmo".Translate().ToString(),
            defaultDesc = data.track.dismissDesc != null ? string.Format(data.track.dismissDesc, data.track.collectiveNoun) : "FCP_Rank_DismissGizmoDesc".Translate(data.track.collectiveNoun).ToString(),
            icon = TexCommand.HoldOpen,
            action = delegate
            {
                Current.Game.GetComponent<GameComponent_PawnRanks>()?.Dismiss(pawn);
            }
        };
    }

    private static Command_Action BuildRallyGizmo(Pawn pawn, PawnRankData data)
    {
        int cooldownTicks = data.currentTier.rallyCooldownDays * GenDate.TicksPerDay;

        Command_Action command = new Command_Action
        {
            defaultLabel = data.track.rallyLabel ?? "FCP_Rank_RallyGizmo".Translate().ToString(),
            defaultDesc = data.track.rallyDesc != null ? string.Format(data.track.rallyDesc, data.track.collectiveNoun) : "FCP_Rank_RallyGizmoDesc".Translate(data.track.collectiveNoun).ToString(),
            icon = TexCommand.Attack,
            action = delegate
            {
                Current.Game.GetComponent<GameComponent_PawnRanks>()?.Rally(pawn);
            }
        };

        if (data.lastRallyTick >= 0 && Find.TickManager.TicksGame - data.lastRallyTick < cooldownTicks)
            command.Disable("FCP_Rank_SessionOnCooldown".Translate());

        return command;
    }

    private static Command_Target BuildSessionGizmo(Pawn pawn, PawnRankData data, string label, string desc, JobDef jobDef, Texture2D icon, int lastTick)
    {
        Command_Target command = new Command_Target
        {
            defaultLabel = label,
            defaultDesc = desc,
            icon = icon,
            targetingParams = new TargetingParameters
            {
                canTargetPawns = true,
                canTargetSelf = false,
                canTargetBuildings = false,
                canTargetAnimals = false,
                canTargetItems = false,
                validator = (TargetInfo t) => IsValidPartner(data, t.Thing as Pawn)
            },
            action = delegate(LocalTargetInfo target)
            {
                if (target.Pawn != null)
                    pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(jobDef, target.Pawn));
            }
        };

        if (lastTick >= 0 && Find.TickManager.TicksGame - lastTick < CooldownTicks)
            command.Disable("FCP_Rank_SessionOnCooldown".Translate());

        return command;
    }

    private static Command_Target BuildInspectionGizmo(Pawn pawn, PawnRankData data)
    {
        return new Command_Target
        {
            defaultLabel = data.track.inspectLabel ?? "FCP_Rank_InspectGizmo".Translate().ToString(),
            defaultDesc = data.track.inspectDesc ?? "FCP_Rank_InspectGizmoDesc".Translate().ToString(),
            icon = TexCommand.Draft,
            targetingParams = new TargetingParameters
            {
                canTargetPawns = true,
                canTargetSelf = false,
                canTargetBuildings = false,
                canTargetAnimals = false,
                canTargetItems = false,
                validator = (TargetInfo t) => IsValidInspectionTarget(data, t.Thing as Pawn)
            },
            action = delegate(LocalTargetInfo target)
            {
                if (target.Pawn != null)
                    pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(RankJobDefOf.FCP_Job_RankInspection, target.Pawn));
            }
        };
    }

    private static Command_Target BuildPunishmentGizmo(Pawn pawn, PawnRankData data)
    {
        return new Command_Target
        {
            defaultLabel = data.track.punishLabel ?? "FCP_Rank_PunishGizmo".Translate().ToString(),
            defaultDesc = data.track.punishDesc ?? "FCP_Rank_PunishGizmoDesc".Translate().ToString(),
            icon = TexCommand.ForbidOn,
            targetingParams = new TargetingParameters
            {
                canTargetPawns = true,
                canTargetSelf = false,
                canTargetBuildings = false,
                canTargetAnimals = false,
                canTargetItems = false,
                validator = (TargetInfo t) => IsValidPunishmentTarget(data, t.Thing as Pawn)
            },
            action = delegate(LocalTargetInfo target)
            {
                Pawn subordinate = target.Pawn;
                if (subordinate?.jobs == null)
                    return;

                subordinate.jobs.StartJob(JobMaker.MakeJob(RankJobDefOf.FCP_Job_PunishmentDetail), JobCondition.InterruptForced, resumeCurJobAfterwards: true);

                GameComponent_PawnRanks comp = Current.Game.GetComponent<GameComponent_PawnRanks>();
                PawnRankData subordinateData = comp?.GetRankData(subordinate);
                if (subordinateData != null)
                    subordinateData.lastPunishedTick = Find.TickManager.TicksGame;

                comp?.GetRankData(pawn)?.AddHistory("FCP_Rank_History_Punished".Translate(subordinate.LabelShortCap));
                Messages.Message("FCP_Rank_PunishmentAssigned".Translate(pawn.LabelShortCap, subordinate.LabelShortCap), subordinate, MessageTypeDefOf.NegativeEvent);
            }
        };
    }

    private static bool IsValidPunishmentTarget(PawnRankData data, Pawn candidate)
    {
        if (candidate == null || !candidate.IsColonistPlayerControlled || candidate.Downed || candidate.InMentalState)
            return false;

        PawnRankData candidateData = Current.Game.GetComponent<GameComponent_PawnRanks>()?.GetRankData(candidate);
        if (candidateData == null || candidateData.track != data.track || !RankTrackUtility.IsMilitaryTier(candidateData.currentTier))
            return false;

        if (!RankTrackUtility.IsSeniorTier(data.track, data.currentTier, candidateData.currentTier))
            return false;

        return candidateData.lastPunishedTick < 0 || Find.TickManager.TicksGame - candidateData.lastPunishedTick >= CooldownTicks;
    }

    private static bool IsValidInspectionTarget(PawnRankData data, Pawn candidate)
    {
        if (candidate == null || !candidate.IsColonistPlayerControlled || candidate.Downed || candidate.InMentalState)
            return false;

        PawnRankData candidateData = Current.Game.GetComponent<GameComponent_PawnRanks>()?.GetRankData(candidate);
        if (candidateData == null || candidateData.track != data.track || !RankTrackUtility.IsMilitaryTier(candidateData.currentTier))
            return false;

        if (!RankTrackUtility.IsSeniorTier(data.track, data.currentTier, candidateData.currentTier))
            return false;

        return candidateData.lastInspectedTick < 0 || Find.TickManager.TicksGame - candidateData.lastInspectedTick >= CooldownTicks;
    }

    private static bool IsValidPartner(PawnRankData data, Pawn candidate)
    {
        if (candidate == null || !candidate.IsColonistPlayerControlled || candidate.Downed || candidate.InMentalState)
            return false;

        PawnRankData candidateData = Current.Game.GetComponent<GameComponent_PawnRanks>()?.GetRankData(candidate);

        bool isDrilling = data.currentTier.discipline == CombatDiscipline;
        if (!isDrilling && candidateData?.currentTier?.discipline != data.currentTier.discipline)
            return false;

        int candidateLastTick = isDrilling ? (candidateData?.lastDrilledTick ?? -1) : (candidateData?.lastStudiedTick ?? -1);
        return candidateLastTick < 0 || Find.TickManager.TicksGame - candidateLastTick >= CooldownTicks;
    }
}
