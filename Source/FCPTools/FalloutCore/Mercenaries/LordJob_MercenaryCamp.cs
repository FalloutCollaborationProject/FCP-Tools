using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FCP.Core
{
    public class LordJob_MercenaryCamp : LordJob
    {
        public Faction faction;
        public MercenaryExtension extension;
        private IntVec3 campCenter;
        private List<Thing> placedBlueprints = new List<Thing>();
        public int tributeRequestedTick = -1;
        public int tributeDeadlineTick = -1;
        public ThingDef requestedTributeItem = null;
        public int requestedTributeAmount = 0;
        public bool failedTributeHostile = false;
        private int nextReinforcementTick = -1;

        public LordJob_MercenaryCamp() { }

        public LordJob_MercenaryCamp(Faction faction, IntVec3 campCenter)
        {
            this.faction = faction;
            this.campCenter = campCenter;
            this.extension = faction?.def?.GetModExtension<MercenaryExtension>();

            // Log the camp center for debugging
            Log.Message($"FCP: Created LordJob_MercenaryCamp with camp center at {campCenter}");
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();

            LordToil_SetupCamp toilSetupCamp = new LordToil_SetupCamp(campCenter, faction, extension, placedBlueprints);
            stateGraph.StartingToil = toilSetupCamp;

            LordToil_IdleInCamp toilIdleInCamp = new LordToil_IdleInCamp(campCenter, extension);
            stateGraph.AddToil(toilIdleInCamp);

            Transition transitionSetupToIdle = new Transition(toilSetupCamp, toilIdleInCamp);
            transitionSetupToIdle.AddTrigger(new Trigger_MercCampBlueprintsPlaced());
            stateGraph.AddTransition(transitionSetupToIdle);

            LordToil_AssaultColony toilAssault = new LordToil_AssaultColony(false);
            stateGraph.AddToil(toilAssault);
            Transition transitionSetupToAssault = new Transition(toilSetupCamp, toilAssault);
            transitionSetupToAssault.AddTrigger(new Trigger_BecamePlayerEnemy());
            stateGraph.AddTransition(transitionSetupToAssault);
            Transition transitionIdleToAssault = new Transition(toilIdleInCamp, toilAssault);
            transitionIdleToAssault.AddTrigger(new Trigger_BecamePlayerEnemy());
            stateGraph.AddTransition(transitionIdleToAssault);

            Transition cleanupTransitionSetup = new Transition(toilSetupCamp, null);
            cleanupTransitionSetup.AddTrigger(new Trigger_TickCondition(() => lord.ownedPawns.Count == 0, 1000));
            stateGraph.AddTransition(cleanupTransitionSetup);

            Transition cleanupTransitionIdle = new Transition(toilIdleInCamp, null);
            cleanupTransitionIdle.AddTrigger(new Trigger_TickCondition(() => lord.ownedPawns.Count == 0, 1000));
            stateGraph.AddTransition(cleanupTransitionIdle);

            Transition cleanupTransitionAssault = new Transition(toilAssault, null);
            cleanupTransitionAssault.AddTrigger(new Trigger_TickCondition(() => lord.ownedPawns.Count == 0, 1000));
            stateGraph.AddTransition(cleanupTransitionAssault);

            return stateGraph;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref faction, "faction");
            Scribe_Values.Look(ref campCenter, "campCenter");
            Scribe_Collections.Look(ref placedBlueprints, "placedBlueprints", LookMode.Reference);

            Scribe_Values.Look(ref tributeRequestedTick, "tributeRequestedTick", -1);
            Scribe_Values.Look(ref tributeDeadlineTick, "tributeDeadlineTick", -1);
            Scribe_Defs.Look(ref requestedTributeItem, "requestedTributeItem");
            Scribe_Values.Look(ref requestedTributeAmount, "requestedTributeAmount", 0);
            Scribe_Values.Look(ref failedTributeHostile, "failedTributeHostile", false);
            Scribe_Values.Look(ref nextReinforcementTick, "nextReinforcementTick", -1);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (faction != null)
                {
                    this.extension = faction.def?.GetModExtension<MercenaryExtension>();
                }
                placedBlueprints ??= new List<Thing>();
                placedBlueprints.RemoveAll(bp => bp == null);
            }
        }

        public override string GetReport(Pawn pawn)
        {
            return "LordReportMercenaryCamp".Translate(faction.Name);
        }

        public List<Thing> GetPlacedBlueprints() => placedBlueprints;

        public bool CampHasEnoughFood(int forDays)
        {
            if (lord == null || !lord.ownedPawns.Any()) return true;
            Map map = lord.Map;
            if (map == null) return true;

            float requiredNutrition = 0f;
            foreach (Pawn p in lord.ownedPawns)
            {
                requiredNutrition += Need_Food.BaseFoodFallPerTick * GenDate.TicksPerDay * forDays;
            }

            float availableNutrition = 0f;
            float radius = extension?.mercenaryCampRadius ?? 20f;
            List<Thing> thingsInRadius = map.listerThings.ThingsInGroup(ThingRequestGroup.FoodSourceNotPlantOrTree);

            foreach (Thing foodSource in thingsInRadius)
            {
                if (foodSource.Position.InHorDistOf(campCenter, radius) && !foodSource.IsForbidden(faction))
                {
                    if (foodSource.def.IsIngestible)
                    {
                        availableNutrition += foodSource.GetStatValue(StatDefOf.Nutrition) * foodSource.stackCount;
                    }
                }
            }
            return availableNutrition >= requiredNutrition;
        }

        public void TryRequestTribute()
        {
            if (faction == null || extension == null) return;
            if (tributeRequestedTick != -1 || extension.tributeRequestItems.NullOrEmpty() || Rand.Value > extension.tributeRequestChance)
            {
                return;
            }

            requestedTributeItem = extension.tributeRequestItems.RandomElement();
            requestedTributeAmount = Rand.RangeInclusive(10, 50);
            tributeRequestedTick = Find.TickManager.TicksGame;
            tributeDeadlineTick = tributeRequestedTick + extension.tributeDeadlineDays * GenDate.TicksPerDay;

            string letterLabel = "LetterLabelTributeDemand".Translate(faction.Name);
            string letterText = "LetterTributeDemand".Translate(
                faction.Name,
                requestedTributeAmount,
                requestedTributeItem.label,
                GenDate.ToStringTicksToDays(tributeDeadlineTick - Find.TickManager.TicksGame)
            );

            Pawn letterTarget = lord.ownedPawns.FirstOrDefault(p => !p.Downed) ?? lord.ownedPawns.FirstOrDefault();
            Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NeutralEvent, letterTarget ?? (LookTargets)Map.mapPawns.FreeColonists.FirstOrDefault());
            Log.Message($"FCP: Mercenary faction {faction.Name} demanded {requestedTributeAmount}x {requestedTributeItem.label}. Deadline: {GenDate.ToStringTicksToDays(tributeDeadlineTick - Find.TickManager.TicksGame)} days.");
        }

        public void CheckTributeDeadline()
        {
            if (tributeRequestedTick == -1 || Find.TickManager.TicksGame < tributeDeadlineTick) return;

            Log.Message($"FCP: Tribute deadline passed for {faction.Name}.");
            Messages.Message("MessageTributeDemandFailed".Translate(faction.Name), MessageTypeDefOf.NegativeEvent);

            FactionRelation relation = faction.RelationWith(Faction.OfPlayer, true);
            if (relation != null)
            {
                relation.kind = FactionRelationKind.Hostile;
                failedTributeHostile = true;
                nextReinforcementTick = Find.TickManager.TicksGame + (extension.mercenaryArrivalFrequencyDays * GenDate.TicksPerDay);
            }
            tributeRequestedTick = -1;
            tributeDeadlineTick = -1;
            requestedTributeItem = null;
            requestedTributeAmount = 0;
        }

        public void TryPayTribute()
        {
            if (tributeRequestedTick == -1 || requestedTributeItem == null || requestedTributeAmount <= 0)
            {
                Log.Warning("FCP: TryPayTribute called when no tribute was active.");
                return;
            }

            if (MercenaryUtility.TryRemoveTributeItems(requestedTributeItem, requestedTributeAmount))
            {
                Messages.Message("MessageTributePaid".Translate(requestedTributeAmount, requestedTributeItem.label, faction.Name), MessageTypeDefOf.PositiveEvent);
                tributeRequestedTick = -1;
                tributeDeadlineTick = -1;
                requestedTributeItem = null;
                requestedTributeAmount = 0;
                failedTributeHostile = false;
                nextReinforcementTick = -1;
            }
            else
            {
                Messages.Message("MessageTributePaymentFailed".Translate(requestedTributeAmount, requestedTributeItem.label), MessageTypeDefOf.RejectInput);
            }
        }

        public override void LordJobTick()
        {
            base.LordJobTick();

            if (!failedTributeHostile || Find.TickManager.TicksGame < nextReinforcementTick)
            {
                return;
            }

            if (lord.ownedPawns.Any(p => !p.Downed && !p.Dead))
            {
                Log.Message($"FCP: Spawning tribute reinforcements for {faction.Name}.");
                SpawnReinforcements();
                nextReinforcementTick = Find.TickManager.TicksGame + (extension.mercenaryArrivalFrequencyDays * GenDate.TicksPerDay);
            }
            else
            {
                Log.Message($"FCP: Original hostile tribute group for {faction.Name} defeated. Stopping reinforcements.");
                failedTributeHostile = false;
                nextReinforcementTick = -1;
            }
        }

        private void SpawnReinforcements()
        {
            Map map = lord.Map;
            if (map == null) return;

            float points = 200f;

            PawnGroupMaker groupMaker = extension?.mercenaryGroupToArrive ?? faction.def.pawnGroupMakers.FirstOrDefault(pgm => pgm.kindDef == PawnGroupKindDefOf.Combat);
            if (groupMaker == null)
            {
                Log.Error($"FCP: Cannot spawn reinforcements for {faction.Name}: No suitable PawnGroupMaker found.");
                return;
            }

            PawnGroupMakerParms groupMakerParms = new PawnGroupMakerParms
            {
                groupKind = PawnGroupKindDefOf.Combat,
                tile = map.Tile,
                faction = faction,
                points = points,
                dontUseSingleUseRocketLaunchers = true
            };
            List<Pawn> reinforcements = groupMaker.GeneratePawns(groupMakerParms).ToList();

            if (!reinforcements.Any())
            {
                Log.Error($"FCP: Failed to generate any pawns for mercenary reinforcement group {faction.Name}.");
                return;
            }

            if (!RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 spawnSpot, map, CellFinder.EdgeRoadChance_Hostile))
            {
                Log.Error($"FCP: Could not find spawn spot for mercenary reinforcement group {faction.Name} on map {map.Index}.");
                return;
            }

            foreach (Pawn pawn in reinforcements)
            {
                GenSpawn.Spawn(pawn, spawnSpot, map, WipeMode.Vanish);
            }

            LordJob assaultJob = new LordJob_AssaultColony(faction, true, true, false, false, true);
            LordMaker.MakeNewLord(faction, assaultJob, map, reinforcements);

            Messages.Message("MessageMercenaryReinforcementsArrived".Translate(faction.Name), new LookTargets(spawnSpot, map), MessageTypeDefOf.ThreatBig);
        }
    }

    public class LordToil_SetupCamp : LordToil
    {
        private IntVec3 campCenter;
        private Faction faction;
        private MercenaryExtension extension;
        private List<ThingDef> buildables;
        private List<Thing> placedBlueprintsRef;

        public LordToil_SetupCamp(IntVec3 campCenter, Faction faction, MercenaryExtension extension, List<Thing> placedBlueprints)
        {
            this.campCenter = campCenter;
            this.faction = faction;
            this.extension = extension;
            this.buildables = extension?.campBuildableDefs ?? new List<ThingDef>();
            this.placedBlueprintsRef = placedBlueprints;
        }

        // Safe wrapper for ClosestThingReachable that won't throw exceptions
        private Thing SafeClosestThingReachable(IntVec3 center, Map map, ThingRequest thingReq,
            PathEndMode pathEndMode, TraverseParms traverseParams, float maxDistance,
            Predicate<Thing> validator = null)
        {
            if (map == null || !center.InBounds(map))
                return null;

            try
            {
                return GenClosest.ClosestThingReachable(center, map, thingReq, pathEndMode, traverseParams, maxDistance, validator);
            }
            catch (Exception ex)
            {
                Log.Warning($"FCP: Error in SafeClosestThingReachable: {ex.Message}");
                return null;
            }
        }

        public override void UpdateAllDuties()
        {
            // Validate map and camp center
            if (lord?.Map == null)
            {
                Log.Error("FCP: Lord or map is null in UpdateAllDuties");
                return;
            }

            if (!campCenter.InBounds(lord.Map))
            {
                Log.Error($"FCP: Camp center {campCenter} is out of bounds for map {lord.Map.Index}");
                // Find a valid position within the map
                campCenter = CellFinder.RandomCell(lord.Map);
                Log.Message($"FCP: Relocated camp center to {campCenter}");
            }

            if (Find.TickManager.TicksGame % 120 == 0)
            {
                TryPlaceMissingBlueprints();
            }

            foreach (Pawn pawn in lord.ownedPawns)
            {
                // Skip null pawns
                if (pawn == null) continue;

                if (!pawn.Downed)
                {
                    try
                    {
                        // Initialize work settings if needed
                        if (pawn.workSettings == null)
                        {
                            pawn.workSettings = new Pawn_WorkSettings(pawn);
                            pawn.workSettings.EnableAndInitialize();
                        }

                        bool canConstruct = pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) &&
                                          WorkTypeDefOf.Construction != null &&
                                          pawn.workSettings.WorkIsActive(WorkTypeDefOf.Construction);

                        float radius = extension?.mercenaryCampRadius ?? 20f;

                        if (canConstruct)
                        {
                            pawn.mindState.duty = new PawnDuty(DutyDefOf.Build, campCenter, radius);
                        }
                        else
                        {
                            pawn.mindState.duty = new PawnDuty(DutyDefOf.Defend, campCenter, radius);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"FCP: Error assigning duty to {pawn.LabelShort}: {ex.Message}");
                        // Fallback to a simple defend duty
                        pawn.mindState.duty = new PawnDuty(DutyDefOf.Defend, campCenter);
                    }
                }
                else
                {
                    pawn.mindState.duty = null;
                }
            }
        }

        private void TryPlaceMissingBlueprints()
        {
            Map map = this.lord.Map;
            if (map == null) return;

            float buildRadius = extension?.mercenaryCampRadius ?? 20f;

            placedBlueprintsRef.RemoveAll(bp => bp == null || bp.Destroyed);

            // Validate camp center is within map bounds
            if (!campCenter.InBounds(map))
            {
                Log.Error($"FCP: Camp center {campCenter} is out of bounds for map {map.Index}");
                return;
            }

            foreach (ThingDef buildableDef in buildables)
            {
                bool alreadyExists = placedBlueprintsRef.Any(bp => bp != null && (bp.def == buildableDef || (bp.def.entityDefToBuild == buildableDef)));
                if (!alreadyExists)
                {
                    try
                    {
                        // Use a safe version of ClosestThingReachable that won't throw exceptions
                        Thing existingThing = SafeClosestThingReachable(campCenter, map, ThingRequest.ForDef(buildableDef),
                            PathEndMode.InteractionCell, TraverseParms.For(TraverseMode.PassDoors), buildRadius);

                        if (existingThing != null)
                        {
                            alreadyExists = true;
                        }
                        else
                        {
                            ThingRequest blueprintReq = ThingRequest.ForGroup(ThingRequestGroup.Blueprint);
                            ThingRequest frameReq = ThingRequest.ForGroup(ThingRequestGroup.BuildingFrame);

                            Thing existingBlueprint = SafeClosestThingReachable(campCenter, map, blueprintReq,
                                PathEndMode.InteractionCell, TraverseParms.For(TraverseMode.PassDoors), buildRadius,
                                t => t is Blueprint b && b.def.entityDefToBuild == buildableDef);

                            if (existingBlueprint != null)
                            {
                                alreadyExists = true;
                            }
                            else
                            {
                                Thing existingFrame = SafeClosestThingReachable(campCenter, map, frameReq,
                                    PathEndMode.InteractionCell, TraverseParms.For(TraverseMode.PassDoors), buildRadius,
                                    t => t is Frame f && f.def.entityDefToBuild == buildableDef);

                                if (existingFrame != null)
                                {
                                    alreadyExists = true;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"FCP: Error checking for existing buildings: {ex.Message}");
                        continue;
                    }
                }

                if (!alreadyExists)
                {
                    Rot4 rotation = Rot4.Random;
                    IntVec3 placeCell = IntVec3.Invalid;
                    foreach (IntVec3 cell in GenRadial.RadialCellsAround(campCenter, buildRadius, true))
                    {
                        if (GenConstruct.CanPlaceBlueprintAt(buildableDef, cell, rotation, map, false, null).Accepted)
                        {
                            placeCell = cell;
                            break;
                        }
                    }

                    if (placeCell.IsValid)
                    {
                        Thing placedThing = GenConstruct.PlaceBlueprintForBuild(buildableDef, placeCell, map, rotation, faction, null);
                        if (placedThing != null)
                        {
                            placedBlueprintsRef.Add(placedThing);
                        }
                        else
                        {
                            Log.Warning($"FCP: Failed to place blueprint for {buildableDef.defName} near {campCenter}");
                        }
                    }
                    else
                    {
                        Log.Warning($"FCP: Could not find valid placement cell for {buildableDef.defName} near {campCenter}");
                    }
                }
            }
        }
    }

    public class LordToil_IdleInCamp : LordToil
    {
        private IntVec3 campCenter;
        private MercenaryExtension extension;
        private float campRadius;

        public LordToil_IdleInCamp(IntVec3 campCenter, MercenaryExtension extension)
        {
            this.campCenter = campCenter;
            this.extension = extension;
            this.campRadius = extension?.mercenaryCampRadius ?? 20f;
        }

        private static JobGiver_CookAtCampfire cookGiver = new JobGiver_CookAtCampfire();
        private static JobGiver_HuntInRadius hunterGiver = new JobGiver_HuntInRadius();
        private static JobGiver_ForageInRadius foragerGiver = new JobGiver_ForageInRadius();
        private const int MinSkillForTask = 5;

        public override void UpdateAllDuties()
        {
            List<Pawn> availablePawns = lord.ownedPawns.Where(p => !p.Downed).ToList();
            if (!availablePawns.Any()) return;

            HashSet<Pawn> assignedPawns = new HashSet<Pawn>();

            // Initialize work settings for all pawns if needed
            foreach (Pawn p in availablePawns)
            {
                if (p.workSettings == null)
                {
                    p.workSettings = new Pawn_WorkSettings(p);
                    p.workSettings.EnableAndInitialize();
                }
            }

            var potentialCooks = availablePawns
                .Where(p => (p.skills?.GetSkill(SkillDefOf.Cooking)?.Level ?? 0) >= MinSkillForTask &&
                       p.workSettings != null &&
                       p.workSettings.WorkIsActive(DefDatabase<WorkTypeDef>.GetNamed("Cooking")))
                .OrderByDescending(p => p.skills?.GetSkill(SkillDefOf.Cooking)?.Level ?? 0)
                .ToList();

            var potentialHunters = availablePawns
                .Where(p => (p.skills?.GetSkill(SkillDefOf.Animals)?.Level ?? 0) >= MinSkillForTask)
                .OrderByDescending(p => p.skills?.GetSkill(SkillDefOf.Animals)?.Level ?? 0)
                .ToList();

            var potentialForagers = availablePawns
                .Where(p => (p.skills?.GetSkill(SkillDefOf.Plants)?.Level ?? 0) >= MinSkillForTask)
                .OrderByDescending(p => p.skills?.GetSkill(SkillDefOf.Plants)?.Level ?? 0)
                .ToList();
            foreach (Pawn cook in potentialCooks)
            {
                if (assignedPawns.Contains(cook)) continue;
                ThinkResult cookResult = cookGiver.TryIssueJobPackage(cook, default(JobIssueParams));
                if (cookResult.IsValid)
                {
                    cook.mindState.duty = new PawnDuty(FCPDefOf.FCP_CookAtCampfire);
                    assignedPawns.Add(cook);
                    break;
                }
            }
            foreach (Pawn hunter in potentialHunters)
            {
                if (assignedPawns.Contains(hunter)) continue;
                ThinkResult huntResult = hunterGiver.TryIssueJobPackage(hunter, default(JobIssueParams));
                if (huntResult.IsValid)
                {
                    hunter.mindState.duty = new PawnDuty(FCPDefOf.FCP_HuntInRadius, campCenter, campRadius);
                    assignedPawns.Add(hunter);
                    break;
                }
            }
            foreach (Pawn forager in potentialForagers)
            {
                if (assignedPawns.Contains(forager)) continue;
                ThinkResult forageResult = foragerGiver.TryIssueJobPackage(forager, default(JobIssueParams));
                if (forageResult.IsValid)
                {
                    forager.mindState.duty = new PawnDuty(FCPDefOf.FCP_ForageInRadius, campCenter, campRadius);
                    assignedPawns.Add(forager);
                    break;
                }
            }
            var remainingPawns = availablePawns.Except(assignedPawns).ToList();
            int numRemaining = remainingPawns.Count;
            int desiredGuardCount = Mathf.CeilToInt(numRemaining / 3f);
            int currentGuardCount = 0;

            foreach (Pawn pawn in remainingPawns)
            {
                if (currentGuardCount < desiredGuardCount)
                {
                    pawn.mindState.duty = new PawnDuty(DutyDefOf.Defend, campCenter, campRadius);
                    currentGuardCount++;
                }
                else
                {
                    pawn.mindState.duty = new PawnDuty(DutyDefOf.Idle);
                }
                assignedPawns.Add(pawn);
            }
            foreach (Pawn pawn in lord.ownedPawns.Where(p => p.Downed))
            {
                pawn.mindState.duty = null;
            }
        }

        public override void LordToilTick()
        {
            base.LordToilTick();
            if (Find.TickManager.TicksGame % GenDate.TicksPerDay == 0)
            {
                var lordJob = this.lord.LordJob as LordJob_MercenaryCamp;
                if (lordJob != null)
                {
                    lordJob.TryRequestTribute();
                    lordJob.CheckTributeDeadline();
                }
            }
        }
    }
    public class Trigger_MercCampBlueprintsPlaced : Trigger
    {
        public Trigger_MercCampBlueprintsPlaced() { }

        public override bool ActivateOn(Lord lord, TriggerSignal signal)
        {
            if (signal.type == TriggerSignalType.Tick && Find.TickManager.TicksGame % 500 == 0)
            {
                var specificJob = lord.LordJob as LordJob_MercenaryCamp;
                if (specificJob == null || specificJob.extension == null) return false;

                var placedBlueprints = specificJob.GetPlacedBlueprints();
                int requiredCount = specificJob.extension.campBuildableDefs?.Count ?? 0;
                if (requiredCount == 0) return true;

                int currentCount = placedBlueprints?.Count(bp => bp != null && !bp.Destroyed) ?? 0;
                return currentCount >= requiredCount;
            }
            return false;
        }
    }
}
