using FCP.Core.Logging;
using FCP.Core.Radio;
using RimWorld.Planet;
using UnityEngine;
using Verse.AI;

namespace FCP.Ranks;

public class GameComponent_Expeditions : GameComponent
{
    private Dictionary<Pawn, ExpeditionData> expeditions = new Dictionary<Pawn, ExpeditionData>();
    private Dictionary<Pawn, RankExpeditionDef> pendingDepartures = new Dictionary<Pawn, RankExpeditionDef>();

    private const float BonusScavengeChance = 0.25f;
    private const int EdgeBuffer = 3;

    public GameComponent_Expeditions(Game game)
    {
    }

    public bool IsAway(Pawn pawn)
    {
        return pawn != null && expeditions.ContainsKey(pawn);
    }

    public IEnumerable<Pawn> AllAway => expeditions.Keys;

    public ExpeditionData GetExpeditionData(Pawn pawn)
    {
        if (pawn == null)
            return null;

        expeditions.TryGetValue(pawn, out ExpeditionData result);
        return result;
    }

    public void Send(Pawn pawn, RankExpeditionDef expedition)
    {
        if (pawn == null || expedition == null || IsAway(pawn) || pendingDepartures.ContainsKey(pawn) || !pawn.Spawned || pawn.Map == null || pawn.jobs == null)
            return;

        PawnRankData rankData = Current.Game.GetComponent<GameComponent_PawnRanks>()?.GetRankData(pawn);
        if (rankData != null && !RankTrackUtility.IsFactionActive(rankData.track))
            return;

        if (!RCellFinder.TryFindBestExitSpot(pawn, out IntVec3 exitCell))
        {
            Messages.Message("FCP_Rank_Expedition_NoExit".Translate(pawn.LabelShortCap), pawn, MessageTypeDefOf.RejectInput, false);
            return;
        }

        pendingDepartures[pawn] = expedition;
        pawn.jobs.StartJob(JobMaker.MakeJob(JobDefOf.Goto, exitCell), JobCondition.InterruptForced);
    }

    public void ForceReturn(Pawn pawn)
    {
        if (!expeditions.TryGetValue(pawn, out ExpeditionData data))
            return;

        expeditions.Remove(pawn);
        Return(pawn, data);
    }

    public override void GameComponentTick()
    {
        if (pendingDepartures.Count > 0)
            ProcessPendingDepartures();

        if (expeditions.Count > 0 && Find.TickManager.TicksGame % 60 == 0)
            ProcessDueReturns();
    }

    private void ProcessDueReturns()
    {
        List<Pawn> due = null;
        foreach (KeyValuePair<Pawn, ExpeditionData> entry in expeditions)
        {
            if (Find.TickManager.TicksGame >= entry.Value.returnTick)
            {
                due ??= new List<Pawn>();
                due.Add(entry.Key);
            }
        }

        if (due == null)
            return;

        foreach (Pawn pawn in due)
        {
            ExpeditionData data = expeditions[pawn];
            expeditions.Remove(pawn);
            Return(pawn, data);
        }
    }

    private void ProcessPendingDepartures()
    {
        List<Pawn> resolved = null;
        foreach (KeyValuePair<Pawn, RankExpeditionDef> entry in pendingDepartures)
        {
            Pawn pawn = entry.Key;
            if (!pawn.Spawned || pawn.jobs?.curJob == null || pawn.jobs.curJob.def != JobDefOf.Goto)
            {
                resolved ??= new List<Pawn>();
                resolved.Add(pawn);
            }
        }

        if (resolved == null)
            return;

        foreach (Pawn pawn in resolved)
        {
            RankExpeditionDef expedition = pendingDepartures[pawn];
            pendingDepartures.Remove(pawn);

            if (pawn.Spawned && IsNearMapEdge(pawn.Position, pawn.Map))
            {
                BeginExpedition(pawn, expedition);
            }
            else if (pawn.Spawned)
            {
                Messages.Message("FCP_Rank_Expedition_DepartureInterrupted".Translate(pawn.LabelShortCap), pawn, MessageTypeDefOf.NeutralEvent);
            }
        }
    }

    private static bool IsNearMapEdge(IntVec3 cell, Map map)
    {
        return cell.x <= EdgeBuffer || cell.z <= EdgeBuffer
            || cell.x >= map.Size.x - EdgeBuffer - 1 || cell.z >= map.Size.z - EdgeBuffer - 1;
    }

    private void BeginExpedition(Pawn pawn, RankExpeditionDef expedition)
    {
        Map map = pawn.Map;
        pawn.DeSpawn();
        Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.KeepForever);

        expeditions[pawn] = new ExpeditionData
        {
            expedition = expedition,
            originMap = map,
            returnTick = Find.TickManager.TicksGame + expedition.durationDays.RandomInRange * GenDate.TicksPerDay
        };
    }

    private void Return(Pawn pawn, ExpeditionData data)
    {
        Map map = (data.originMap != null && Find.Maps.Contains(data.originMap)) ? data.originMap : Find.AnyPlayerHomeMap;
        if (map == null)
            return;

        if (!RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 entryCell, map, CellFinder.EdgeRoadChance_Ignore))
            entryCell = CellFinder.RandomEdgeCell(map);

        Find.WorldPawns.RemovePawn(pawn);
        if (pawn.Faction != Faction.OfPlayer)
        {
            FCPLog.Warning($"Expedition return: {pawn.LabelShortCap} had drifted to faction " +
                $"'{pawn.Faction?.Name ?? "null"}' while away on expedition '{data.expedition?.defName}'. Restoring to player faction.");
            pawn.SetFaction(Faction.OfPlayer);
        }
        GenSpawn.Spawn(pawn, entryCell, map);

        RankExpeditionDef expedition = data.expedition;
        GameComponent_PawnRanks rankComp = Current.Game.GetComponent<GameComponent_PawnRanks>();
        PawnRankData rankData = rankComp?.GetRankData(pawn);

        float competence = GetCompetenceFactor(pawn, rankData);
        float effectiveFailureChance = Mathf.Clamp01(expedition.failureChance / competence);

        bool failed = Rand.Chance(effectiveFailureChance);
        bool injured = Rand.Chance(effectiveFailureChance * 0.6f);
        string gearLostLabel = Rand.Chance(effectiveFailureChance * 0.4f) ? ApplyGearLoss(pawn) : null;

        if (injured)
        {
            ApplyExpeditionInjury(pawn);
            Messages.Message("FCP_Rank_Expedition_Injured".Translate(pawn.LabelShortCap), pawn, MessageTypeDefOf.NegativeEvent);
        }

        if (gearLostLabel != null)
            Messages.Message("FCP_Rank_Expedition_GearLost".Translate(pawn.LabelShortCap, gearLostLabel), pawn, MessageTypeDefOf.NegativeEvent);

        if (failed)
        {
            bool wasDemoted = rankData != null && RankTrackUtility.FindPreviousTier(rankData.track, rankData.currentTier) != null;
            if (rankData != null)
                RankTrackUtility.Demote(pawn, rankData, "FCP_Rank_Demoted_Expedition");

            if (!wasDemoted)
            {
                string failText = "FCP_Rank_Expedition_FailText".Translate(pawn.LabelShortCap, expedition.LabelCap);
                rankData?.AddHistory(failText);
            }

            Find.LetterStack.ReceiveLetter(
                "FCP_Rank_Expedition_FailTitle".Translate(pawn.LabelShortCap),
                "FCP_Rank_Expedition_FailText".Translate(pawn.LabelShortCap, expedition.LabelCap),
                LetterDefOf.NegativeEvent,
                pawn);

            WalkInFromEdge(pawn, map);
            return;
        }

        if (expedition.xpReward != null && pawn.skills != null)
        {
            foreach (SkillGain gain in expedition.xpReward)
                pawn.skills.Learn(gain.skill, gain.amount);
        }

        if (expedition.lootMaker != null)
        {
            ThingSetMakerParams lootParams = default;
            lootParams.totalMarketValueRange = expedition.lootMarketValueRange;
            List<Thing> loot = expedition.lootMaker.root.Generate(lootParams);
            foreach (Thing thing in loot)
                GenPlace.TryPlaceThing(thing, pawn.Position, map, ThingPlaceMode.Near);
        }

        if (expedition.lootMaker != null && Rand.Chance(BonusScavengeChance))
            GrantBonusScavenge(pawn, expedition, map);

        if (Rand.Chance(expedition.siteRevealChance))
            RadioUtility.TryRevealNearbySite(pawn);

        if (rankData != null)
            rankData.missionsCompleted++;

        if (!expedition.returnFlavorKeys.NullOrEmpty())
        {
            string key = expedition.returnFlavorKeys.RandomElement();
            string returnText = key.Translate(pawn.Named("PAWN"));
            rankData?.AddHistory(returnText);

            Find.LetterStack.ReceiveLetter(
                "FCP_Rank_Expedition_ReturnTitle".Translate(pawn.LabelShortCap),
                returnText,
                LetterDefOf.PositiveEvent,
                pawn);
        }

        WalkInFromEdge(pawn, map);
    }

    private static float GetCompetenceFactor(Pawn pawn, PawnRankData rankData)
    {
        SkillDef relevantSkill = rankData?.currentTier?.discipline == RankTrackUtility.CombatDiscipline
            ? SkillDefOf.Shooting
            : SkillDefOf.Intellectual;

        int level = pawn.skills?.GetSkill(relevantSkill)?.Level ?? 0;
        return Mathf.Clamp(level / 10f, 0.4f, 2f);
    }

    private static void ApplyExpeditionInjury(Pawn pawn)
    {
        if (pawn.health == null)
            return;

        DamageDef damageDef = Rand.Bool ? DamageDefOf.Cut : DamageDefOf.Blunt;
        int amount = Rand.RangeInclusive(6, 16);
        pawn.TakeDamage(new DamageInfo(damageDef, amount));
    }

    private static string ApplyGearLoss(Pawn pawn)
    {
        List<Thing> candidates = new List<Thing>();
        if (pawn.equipment?.Primary != null)
            candidates.Add(pawn.equipment.Primary);
        if (pawn.apparel != null)
            candidates.AddRange(pawn.apparel.WornApparel);

        if (candidates.Count == 0)
            return null;

        Thing lost = candidates.RandomElement();
        string label = lost.LabelCap;

        if (lost is Apparel apparel)
        {
            pawn.apparel.Remove(apparel);
            apparel.Destroy();
        }
        else if (pawn.equipment.Primary == lost)
        {
            pawn.equipment.DestroyEquipment((ThingWithComps)lost);
        }
        else
        {
            return null;
        }

        return label;
    }

    private static void GrantBonusScavenge(Pawn pawn, RankExpeditionDef expedition, Map map)
    {
        ThingSetMakerParams lootParams = default;
        lootParams.totalMarketValueRange = new FloatRange(10f, 40f);
        List<Thing> loot = expedition.lootMaker.root.Generate(lootParams);
        foreach (Thing thing in loot)
            GenPlace.TryPlaceThing(thing, pawn.Position, map, ThingPlaceMode.Near);
    }

    private static void WalkInFromEdge(Pawn pawn, Map map)
    {
        if (!pawn.Spawned || pawn.jobs == null)
            return;

        IntVec3 walkTo = CellFinder.RandomClosewalkCellNear(map.Center, map, 10);
        pawn.jobs.StartJob(JobMaker.MakeJob(JobDefOf.Goto, walkTo), JobCondition.InterruptForced);
    }

    private List<Pawn> expeditionKeys;
    private List<ExpeditionData> expeditionValues;
    private List<Pawn> pendingDepartureKeys;
    private List<RankExpeditionDef> pendingDepartureValues;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref expeditions, "expeditions", LookMode.Reference, LookMode.Deep, ref expeditionKeys, ref expeditionValues);
        Scribe_Collections.Look(ref pendingDepartures, "pendingDepartures", LookMode.Reference, LookMode.Def, ref pendingDepartureKeys, ref pendingDepartureValues);

        if (Scribe.mode == LoadSaveMode.LoadingVars && expeditions == null)
            expeditions = new Dictionary<Pawn, ExpeditionData>();

        if (Scribe.mode == LoadSaveMode.LoadingVars && pendingDepartures == null)
            pendingDepartures = new Dictionary<Pawn, RankExpeditionDef>();
    }
}
