using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse.AI;
using Verse.AI.Group;
using RimWorld.Planet;

namespace FCP.Core
{
    [HarmonyPatch(typeof(Faction), "FactionTick")]
    public static class Faction_FactionTick_MercenaryPayment
    {
        static void Postfix(Faction __instance)
        {
            if (__instance.def.HasModExtension<MercenaryExtension>())
            {
                var extension = __instance.def.GetModExtension<MercenaryExtension>();
                TryProcessMercenaryPayment(__instance, extension);
            }
        }

        static void TryProcessMercenaryPayment(Faction faction, MercenaryExtension extension)
        {
            var data = faction.GetMercenaryData();
            if (data == null) return;

            int currentTick = Find.TickManager.TicksGame;

            int ticksPerCycle = extension.paymentCycleDays * GenDate.TicksPerDay;
            int paymentDueTick = data.lastPaymentTicks + ticksPerCycle;

            if (currentTick >= paymentDueTick)
            {
                var hiredMercs = data.AllHiredPawns;
                hiredMercs.RemoveAll(p => p == null || p.Destroyed);

                if (!hiredMercs.Any())
                {
                    data.missedPaymentCount = 0;
                    data.firstMissedPaymentTick = -1;
                    data.lastPaymentTicks = currentTick;
                }
                else
                {
                    float totalUpkeepCost = hiredMercs.Count;

                    if (MercenaryUtility.TryPayCost(totalUpkeepCost))
                    {
                        data.lastPaymentTicks = currentTick;
                        data.missedPaymentCount = 0;
                        data.firstMissedPaymentTick = -1;
                        data.hiringBlockedUntilTick = -1;
                    }
                    else
                    {
                        data.missedPaymentCount++;
                        if (data.firstMissedPaymentTick == -1)
                        {
                            data.firstMissedPaymentTick = currentTick;
                        }
                        Messages.Message("MessageMercenaryPaymentFailed".Translate(faction.Name, totalUpkeepCost.ToString("F0")), MessageTypeDefOf.NegativeEvent);

                        int consequenceTriggerTick = data.firstMissedPaymentTick + (extension.hostilityDelayDaysAfterMissedPayment * GenDate.TicksPerDay);
                        if (currentTick >= consequenceTriggerTick)
                        {
                            ApplyMercenaryConsequences(faction, extension, data);
                            data.missedPaymentCount = 0;
                            data.firstMissedPaymentTick = -1;
                        }
                        data.lastPaymentTicks = currentTick;
                    }
                }
            }

            CheckReservedArrivals(faction, data, currentTick);
            if (currentTick % GenDate.TicksPerHour == 0)
            {
                ProcessCaravanReturns(faction, data, currentTick);
            }
        }

        static void CheckReservedArrivals(Faction faction, FactionMercenaryData data, int currentTick)
        {
            List<QueuedMercenaryArrival> arrivalsToRemove = SimplePool<List<QueuedMercenaryArrival>>.Get();
            arrivalsToRemove.Clear();

            foreach (var arrival in data.reservedArrivals)
            {
                if (currentTick >= arrival.arrivalTick)
                {
                    Map targetMap = arrival.destinationMap;
                    if (targetMap != null && targetMap.IsPlayerHome && !targetMap.Disposed)
                    {
                        PawnGenerationRequest request = new PawnGenerationRequest(arrival.pawnKind, arrival.faction, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, 1f, false, false, true, true, true);
                        Pawn pawn = PawnGenerator.GeneratePawn(request);

                        if (pawn != null)
                        {
                            if (RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 spawnSpot, targetMap, CellFinder.EdgeRoadChance_Neutral))
                            {
                                GenSpawn.Spawn(pawn, spawnSpot, targetMap, WipeMode.Vanish);
                                MercenaryUtility.HireMercenary(pawn);

                                Find.LetterStack.ReceiveLetter(
                                    "LetterLabelMercenaryReservedArrived".Translate(),
                                    "LetterMercenaryReservedArrived".Translate(pawn.LabelShortCap, faction.Name, targetMap.Parent.Label),
                                    LetterDefOf.PositiveEvent,
                                    pawn
                                );
                            }
                            else
                            {
                                Log.Error($"FCP: Could not find spawn spot for reserved mercenary {arrival.pawnKind.defName} on map {targetMap.Index}.");
                                pawn.Discard();
                            }
                        }
                        else
                        {
                            Log.Error($"FCP: Failed to generate reserved mercenary pawn {arrival.pawnKind.defName} for faction {faction.Name}.");
                        }
                    }
                    else
                    {
                        Log.Warning($"FCP: Target map for reserved mercenary {arrival.pawnKind.defName} no longer exists, is disposed, or is not player home. Discarding arrival.");
                    }
                    arrivalsToRemove.Add(arrival);
                }
            }

            foreach (var arrivalToRemove in arrivalsToRemove)
            {
                data.reservedArrivals.Remove(arrivalToRemove);
            }
            arrivalsToRemove.Clear();
            SimplePool<List<QueuedMercenaryArrival>>.Return(arrivalsToRemove);
        }

        static void ProcessCaravanReturns(Faction faction, FactionMercenaryData data, int currentTick)
        {
            if (data?.activeCaravans == null) return;

            List<MercenaryCaravanData> completedCaravans = SimplePool<List<MercenaryCaravanData>>.Get();
            completedCaravans.Clear();
            for (int i = data.activeCaravans.Count - 1; i >= 0; i--)
            {
                var caravan = data.activeCaravans[i];
                if (currentTick >= caravan.returnTick)
                {
                    ResolveCaravan(caravan);
                    completedCaravans.Add(caravan);
                }
            }
            if (completedCaravans.Any())
            {
                data.activeCaravans.RemoveAll(c => completedCaravans.Contains(c));
            }

            completedCaravans.Clear();
            SimplePool<List<MercenaryCaravanData>>.Return(completedCaravans);
        }


        static void ResolveCaravan(MercenaryCaravanData caravan)
        {
            Map targetMap = Find.Maps.Where(m => m.IsPlayerHome).RandomElementWithFallback();
            if (targetMap == null)
            {
                Log.Warning($"FCP: Mercenary caravan returning for {caravan.faction?.Name ?? "Unknown Faction"} but no player home map found. Discarding results.");
                return;
            }

            bool success = Rand.Chance(caravan.successChance);
            string letterLabel;
            string letterText;
            LetterDef letterDef;
            List<Thing> rewards = new List<Thing>();

            if (success)
            {
                letterLabel = "LetterLabelMercenaryCaravanSuccess".Translate();
                letterText = "LetterMercenaryCaravanSuccess".Translate(caravan.faction?.Name ?? "Mercenaries");
                letterDef = LetterDefOf.PositiveEvent;
                GenerateRewards(caravan, rewards);
                if (rewards.Any())
                {
                    letterText += "\n\n" + "MercenaryCaravanRewards".Translate() + "\n" + GenLabel.ThingsLabel(rewards);
                }
            }
            else
            {
                letterLabel = "LetterLabelMercenaryCaravanFailure".Translate();
                letterText = "LetterMercenaryCaravanFailure".Translate(caravan.faction?.Name ?? "Mercenaries");
                letterDef = LetterDefOf.NegativeEvent;
            }
            if (RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 spawnSpot, targetMap, CellFinder.EdgeRoadChance_Neutral))
            {
                foreach (var reward in rewards)
                {
                    GenPlace.TryPlaceThing(reward, spawnSpot, targetMap, ThingPlaceMode.Near);
                }
                if (success && caravan.pawns != null)
                {
                    foreach (var pawn in caravan.pawns)
                    {
                        if (pawn != null && !pawn.Destroyed && !pawn.Dead)
                        {
                            GenSpawn.Spawn(pawn, spawnSpot, targetMap, WipeMode.Vanish);
                        }
                    }
                }

                Find.LetterStack.ReceiveLetter(letterLabel, letterText, letterDef, new LookTargets(spawnSpot, targetMap));
            }
            else
            {
                Log.Error($"FCP: Could not find spawn spot for returning mercenary caravan {caravan.faction?.Name ?? "Unknown Faction"} on map {targetMap.Index}. Rewards and pawns lost.");
                rewards.ForEach(r => r.Destroy());
            }
        }

        static void GenerateRewards(MercenaryCaravanData caravan, List<Thing> rewards)
        {
            if (caravan.objective == null || caravan.pawns == null) return;
            float totalPoints = caravan.pawns.Sum(p => p?.kindDef?.combatPower ?? 50f);
            float baseRewardValue = totalPoints * 2f;
            if (!caravan.objective.tradeTags.NullOrEmpty())
            {
                ThingSetMaker_MarketValue thingSetMaker = new ThingSetMaker_MarketValue();
                ThingSetMakerParams parms = default(ThingSetMakerParams);
                parms.totalMarketValueRange = new FloatRange(baseRewardValue * 0.8f, baseRewardValue * 1.2f);

                var generatedItems = thingSetMaker.Generate(parms);
                var filteredItems = generatedItems.Where(item =>
                    item.def.tradeTags != null &&
                    caravan.objective.tradeTags.Any(tagDef => tagDef != null && item.def.tradeTags.Contains(tagDef.defName))
                ).ToList();

                if (filteredItems.Any())
                {
                    rewards.AddRange(filteredItems);
                }
                else if (generatedItems.Any())
                {
                    rewards.AddRange(generatedItems);
                    Log.Warning($"FCP: Could not generate rewards matching tradeTags {string.Join(", ", caravan.objective.tradeTags.Select(t => t?.defName ?? "null"))} for caravan. Providing generic rewards.");
                }
            }
            if (!caravan.objective.prisonerPawnKinds.NullOrEmpty())
            {
                int numPrisoners = Rand.RangeInclusive(1, Mathf.Max(1, caravan.pawns.Count / 3));
                for (int i = 0; i < numPrisoners; i++)
                {
                    PawnKindDef kindToGenerate = caravan.objective.prisonerPawnKinds.RandomElement();
                    if (kindToGenerate == null) continue;
                    PawnGenerationRequest request = new PawnGenerationRequest(
                        kindToGenerate,
                        Faction.OfAncientsHostile,
                        PawnGenerationContext.NonPlayer,
                        tile: -1,
                        forceGenerateNewPawn: true,
                        allowDead: false,
                        allowDowned: true,
                        canGeneratePawnRelations: true,
                        mustBeCapableOfViolence: false,
                        colonistRelationChanceFactor: 0f
                    );


                    Pawn prisoner = PawnGenerator.GeneratePawn(request);
                    if (prisoner != null)
                    {
                        HealthUtility.DamageUntilDowned(prisoner, false);
                        prisoner.guest.SetGuestStatus(Faction.OfPlayer, GuestStatus.Prisoner);
                        rewards.Add(prisoner);
                    }
                }
            }
        }


        static void ApplyMercenaryConsequences(Faction faction, MercenaryExtension extension, FactionMercenaryData data)
        {
            int currentTick = Find.TickManager.TicksGame;
            data.hiringBlockedUntilTick = currentTick + (extension.hiringCooldownDaysAfterHostility * GenDate.TicksPerDay);

            var consequence = extension.missedPaymentConsequences.RandomElementWithFallback(MissedPaymentConsequence.Hostility);

            Messages.Message("MessageMercenaryConsequences".Translate(faction.Name, consequence.ToString()), MessageTypeDefOf.ThreatBig);

            if (consequence == MissedPaymentConsequence.Hostility)
            {
                FactionRelation relation = faction.RelationWith(Faction.OfPlayer, true);
                if (relation != null)
                {
                    relation.kind = FactionRelationKind.Hostile;
                }

                foreach (Pawn merc in data.AllHiredPawns)
                {
                    if (merc != null && merc.Map != null && !merc.Dead && !merc.Downed)
                    {
                    }
                }
            }
            else if (consequence == MissedPaymentConsequence.Theft)
            {
                Map targetMap = Find.Maps.Where(m => m.IsPlayerHome).RandomElementWithFallback();
                if (targetMap == null)
                {
                    Log.Warning($"FCP: Mercenary theft consequence for {faction.Name} failed: No player home map found.");
                    faction.TryAffectGoodwillWith(Faction.OfPlayer, -75, true, true, null, null);
                    return;
                }

                float points = 150f;

                PawnGroupMaker groupMaker = faction.def.GetModExtension<MercenaryExtension>()?.mercenaryGroupToArrive ?? faction.def.pawnGroupMakers.FirstOrDefault(pgm => pgm.kindDef == PawnGroupKindDefOf.Combat);
                if (groupMaker == null)
                {
                    Log.Error($"FCP: Cannot trigger theft for {faction.Name}: No suitable PawnGroupMaker found.");
                    faction.TryAffectGoodwillWith(Faction.OfPlayer, -75, true, true, null, null);
                    return;
                }

                PawnGroupMakerParms groupMakerParms = new PawnGroupMakerParms
                {
                    groupKind = PawnGroupKindDefOf.Combat,
                    tile = targetMap.Tile,
                    faction = faction,
                    points = points,
                    dontUseSingleUseRocketLaunchers = true
                };
                List<Pawn> thieves = groupMaker.GeneratePawns(groupMakerParms).ToList();

                if (!thieves.Any())
                {
                    Log.Error($"FCP: Failed to generate any pawns for mercenary theft group {faction.Name}.");
                    faction.TryAffectGoodwillWith(Faction.OfPlayer, -75, true, true, null, null);
                    return;
                }
                if (!RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 spawnSpot, targetMap, CellFinder.EdgeRoadChance_Hostile))
                {
                    Log.Error($"FCP: Could not find spawn spot for mercenary theft group {faction.Name} on map {targetMap.Index}.");
                    faction.TryAffectGoodwillWith(Faction.OfPlayer, -75, true, true, null, null);
                    return;
                }
                foreach (Pawn thief in thieves)
                {
                    GenSpawn.Spawn(thief, spawnSpot, targetMap, WipeMode.Vanish);
                }
                LordJob lordJob = new LordJob_StealAndLeave(faction, spawnSpot);
                LordMaker.MakeNewLord(faction, lordJob, targetMap, thieves);
                Find.LetterStack.ReceiveLetter(
                    "LetterLabelMercenaryTheft".Translate(),
                    "LetterMercenaryTheft".Translate(faction.Named("FACTION")),
                    LetterDefOf.ThreatSmall,
                    new LookTargets(spawnSpot, targetMap)
                );
            }
        }
    }

    [HarmonyPatch(typeof(Faction), "ExposeData")]
    public static class Faction_ExposeData_Patch
    {
        public static SaveDataHandler<Faction, FactionMercenaryData> MercenaryDataHandler = new SaveDataHandler<Faction, FactionMercenaryData>("mercData", LookMode.Deep);

        public static void Postfix(Faction __instance)
        {
            if (__instance.def.HasModExtension<MercenaryExtension>())
            {
                GetMercenaryData(__instance);
                MercenaryDataHandler.ExposeData(__instance);
            }
        }

        public static FactionMercenaryData GetMercenaryData(this Faction faction)
        {
            if (faction == null || !faction.def.HasModExtension<MercenaryExtension>())
            {
                return null;
            }

            if (!MercenaryDataHandler.TryGet(faction, out FactionMercenaryData data))
            {
                data = new FactionMercenaryData();
                MercenaryDataHandler.Set(faction, data);
            }
            data.groups ??= new List<MercenaryGroup>();
            data.reservedArrivals ??= new List<QueuedMercenaryArrival>();

            return data;
        }
    }

    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
    public static class FloatMenuMakerMap_AddHumanlikeOrders_Patch
    {
        static void Postfix(List<FloatMenuOption> opts, Pawn pawn, UnityEngine.Vector3 clickPos)
        {
            if (pawn == null || pawn.Faction == null || pawn.Faction.IsPlayer || !pawn.RaceProps.Humanlike)
            {
                return;
            }

            var extension = pawn.Faction.def.GetModExtension<MercenaryExtension>();
            if (extension == null)
            {
                return;
            }

            var lordJob = pawn.GetLord()?.LordJob as LordJob_MercenaryCamp;
            if (lordJob != null && lordJob.tributeRequestedTick != -1 && lordJob.requestedTributeItem != null && lordJob.requestedTributeAmount > 0)
            {
                bool canPay = MercenaryUtility.PlayerHasEnoughTribute(lordJob.requestedTributeItem, lordJob.requestedTributeAmount);
                string label = "PayTributeOption".Translate(lordJob.requestedTributeAmount, lordJob.requestedTributeItem.label);
                FloatMenuOption tributeOption;

                if (canPay)
                {
                    tributeOption = new FloatMenuOption(label, () =>
                    {
                        lordJob.TryPayTribute();
                    }, MenuOptionPriority.Low);
                }
                else
                {
                    label += " (" + "CommandPayTributeFailInsufficient".Translate(lordJob.requestedTributeAmount, lordJob.requestedTributeItem.label) + ")";
                    tributeOption = new FloatMenuOption(label, null, MenuOptionPriority.Low);
                }
                opts.Add(tributeOption);
            }

            var data = pawn.Faction.GetMercenaryData();
            IntVec3 clickCell = IntVec3.FromVector3(clickPos);

            if (MercenaryUtility.IsMercenaryHired(pawn))
            {
                FloatMenuOption releaseOption = new FloatMenuOption("ReleaseMercenary".Translate(pawn.LabelShortCap), () =>
                {
                    MercenaryUtility.ReleaseMercenary(pawn);
                    Messages.Message("MessageMercenaryReleased".Translate(pawn.LabelShortCap, pawn.Faction.Name), pawn, MessageTypeDefOf.PositiveEvent);
                }, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);

                opts.Add(releaseOption);
                if (pawn.Map != null && pawn.Map.IsPlayerHome)
                {
                    Command_Action sendCaravanAction = new Command_Action
                    {
                        defaultLabel = "SendMercenaryCaravanGizmoLabel".Translate(),
                        defaultDesc = "SendMercenaryCaravanGizmoDesc".Translate(),
                        icon = ContentFinder<UnityEngine.Texture2D>.Get("UI/Commands/LaunchShip", true),
                        action = () =>
                        {
                            Messages.Message($"DEBUG: Open Send Caravan Dialog for {pawn.LabelShortCap} (Not Implemented)", MessageTypeDefOf.NeutralEvent);
                        }
                    };
                    if (pawn.Downed)
                    {
                        sendCaravanAction.Disable("CannotSendMercenaryCaravanIncapacitated".Translate(pawn.LabelShortCap));
                    }
                    FloatMenuOption sendCaravanOption = new FloatMenuOption(
                        sendCaravanAction.defaultLabel,
                        sendCaravanAction.Disabled ? null : sendCaravanAction.action,
                        MenuOptionPriority.Default,
                        null, null, 0f, null, null, true, 0
                    );
                    if (sendCaravanAction.Disabled)
                    {
                        sendCaravanOption.Label += " (" + sendCaravanAction.disabledReason + ")";
                    }

                    opts.Add(sendCaravanOption);
                }
            }
            else
            {
                bool canHire = true;
                string disabledReason = null;
                float cost = extension.initialHiringCost;
                if (pawn.Faction.GoodwillWith(Faction.OfPlayer) < 0)
                {
                    canHire = false;
                    disabledReason = "CannotHireMercenaryReasonBadGoodwill".Translate(pawn.Faction.Name);
                }
                else if (data != null && data.hiringBlockedUntilTick > Find.TickManager.TicksGame)
                {
                    canHire = false;
                    int daysRemaining = (int)GenDate.TicksToDays(data.hiringBlockedUntilTick - Find.TickManager.TicksGame);
                    disabledReason = "CannotHireMercenaryReasonBlocked".Translate(pawn.Faction.Name, daysRemaining);
                }
                else if (!MercenaryUtility.PlayerHasEnoughSilver(cost))
                {
                    canHire = false;
                    disabledReason = "CannotHireMercenaryReasonInsufficientFunds".Translate(cost.ToString("F0"));
                }

                FloatMenuOption hireOption;
                if (canHire)
                {
                    hireOption = new FloatMenuOption("HireMercenary".Translate(pawn.LabelShortCap, cost.ToString("F0")), () =>
                    {
                        TryHireMercenary_FloatMenu(pawn, extension, data, cost);
                    }, MenuOptionPriority.High, null, null, 0f, null, null, true, 0);
                }
                else
                {
                    hireOption = new FloatMenuOption("HireMercenary".Translate(pawn.LabelShortCap, cost.ToString("F0")) + " (" + disabledReason + ")", null, MenuOptionPriority.High, null, null, 0f, null, null, true, 0);
                }
                opts.Add(hireOption);
            }
        }

        private static void TryHireMercenary_FloatMenu(Pawn pawn, MercenaryExtension extension, FactionMercenaryData data, float cost)
        {
            if (MercenaryUtility.IsMercenaryHired(pawn)) return;
            if (pawn.Faction.GoodwillWith(Faction.OfPlayer) < 0) return;
            if (data != null && data.hiringBlockedUntilTick > Find.TickManager.TicksGame) return;

            if (MercenaryUtility.TryPayCost(cost))
            {
                MercenaryUtility.HireMercenary(pawn);
                if (data != null)
                {
                    data.lastPaymentTicks = Find.TickManager.TicksGame;
                    data.missedPaymentCount = 0;
                    data.firstMissedPaymentTick = -1;
                }
                Messages.Message("MessageMercenaryHired".Translate(pawn.LabelShortCap, pawn.Faction.Name), pawn, MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                Messages.Message("MessageMercenaryHireFailedFunds".Translate(cost.ToString("F0")), pawn, MessageTypeDefOf.RejectInput);
            }
        }
    }
}
