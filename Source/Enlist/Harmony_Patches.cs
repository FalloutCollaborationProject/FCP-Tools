using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace  FCP.Enlist
{
    [HarmonyPatch(typeof(QuestGen))]
    [HarmonyPatch("AddSlateQuestTags")]
    public static class Patch_AddSlateQuestTags
    {
        public static Slate slate;

        public static void Postfix()
        {
            slate = QuestGen.slate.DeepCopy();
        }
    }


    [HarmonyPatch(typeof(QuestNode_GetSiteTile))]
    [HarmonyPatch("TryFindTile")]
    public static class Patch_TryFindTile
    {
        public static WorldObject worldObject;
        public static bool Prefix(ref bool __result, QuestNode_GetSiteTile __instance, Slate slate, ref PlanetTile tile)
        {
            if (worldObject != null && TryFindTile(worldObject, slate, __instance.preferCloserTiles, __instance.allowCaravans, __instance.clampRangeBySiteParts, __instance.sitePartDefs, out tile))
            {
                __result = true;
                return false;
            }
            return true;
        }

        private static bool TryFindTile(WorldObject worldObject, Slate slate, SlateRef<bool> preferCloserTiles, SlateRef<bool> allowCaravans, SlateRef<bool?> clampRangeBySiteParts, SlateRef<IEnumerable<SitePartDef>> sitePartDefs, out PlanetTile tile)
        {
            var nearThisTile = worldObject.Tile;
            int num = int.MaxValue;
            bool? value = clampRangeBySiteParts.GetValue(slate);
            if (value.HasValue && value.Value)
            {
                foreach (SitePartDef item in sitePartDefs.GetValue(slate))
                {
                    if (item.conditionCauserDef != null)
                    {
                        num = Mathf.Min(num, item.conditionCauserDef.GetCompProperties<CompProperties_CausesGameCondition>().worldRange);
                    }
                }
            }
            if (!slate.TryGet("siteDistRange", out IntRange var))
            {
                var = new IntRange(7, Mathf.Min(27, num));
            }
            else if (num != int.MaxValue)
            {
                var = new IntRange(Mathf.Min(var.min, num), Mathf.Min(var.max, num));
            }
            var tileMode = preferCloserTiles.GetValue(slate) ? TileFinderMode.Near : TileFinderMode.Random;
            return TileFinder.TryFindNewSiteTile(out tile, nearThisTile, var.min, var.max, allowCaravans.GetValue(slate), tileFinderMode: tileMode);
        }
    }

    [HarmonyPatch(typeof(Faction))]
    [HarmonyPatch("TryAffectGoodwillWith")]
    public static class Patch_TryAffectGoodwillWith
    {
        public static void Postfix(Faction __instance, Faction other, int goodwillChange, bool canSendMessage = true, bool canSendHostilityLetter = true, string reason = null, GlobalTargetInfo? lookTarget = null)
        {
            if (__instance != null && other != null)
            {

            }
            var worldTracker = WorldEnlistTracker.Instance;
            if (__instance != Faction.OfPlayer && other == Faction.OfPlayer && other.RelationKindWith(__instance) == FactionRelationKind.Hostile)
            {
                foreach (var def in __instance.GetEnlistOptions())
                {
                    if (worldTracker.EnlistedTo(__instance, def))
                    {
                        worldTracker.KickOut(__instance, def);
                    }
                }
            }
            else if (other != Faction.OfPlayer && __instance == Faction.OfPlayer && other.RelationKindWith(__instance) == FactionRelationKind.Hostile)
            {
                foreach (var def in other.GetEnlistOptions())
                {
                    if (worldTracker.EnlistedTo(other, def))
                    {
                        worldTracker.KickOut(other, def);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_DraftController), "GetGizmos")]
    public class Pawn_DraftController_GetGizmos_Patch
    {
        public static TargetingParameters ForLoc()
        {
            TargetingParameters targetingParameters = new TargetingParameters();
            targetingParameters.canTargetLocations = true;
            return targetingParameters;
        }
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn_DraftController __instance)
        {
            foreach (var gizmo in __result)
            {
                yield return gizmo;
            }
            Pawn pawn = __instance.pawn;
            if (pawn.IsColonistPlayerControlled && __instance.Drafted)
            {
                var tracker = WorldEnlistTracker.Instance;
                foreach (var enlistedFaction in tracker.EnlistedFactions())
                {
                    foreach (var optionsDef in enlistedFaction.GetEnlistOptions())
                    {
                        if (tracker.EnlistedTo(enlistedFaction, optionsDef))
                        {
                            if (optionsDef.reinforcementsAreEnabled)
                            {
                                Command_Action command = new Command_Action()
                                {
                                    defaultLabel = optionsDef.reinforcementsButtonLabelKey.Translate(enlistedFaction),
                                    defaultDesc = optionsDef.reinforcementsButtonDescKey.Translate(enlistedFaction),
                                    icon = ContentFinder<Texture2D>.Get(optionsDef.reinforcementsButtonIconTexPath),
                                    disabled = !tracker.CanCallReinforcementFrom(enlistedFaction, optionsDef),
                                    action = delegate
                                    {
                                        Find.Targeter.BeginTargeting(ForLoc(), delegate (LocalTargetInfo x)
                                        {
                                            tracker.CallReinforcement(enlistedFaction, optionsDef, pawn, x.Cell);
                                        }, null, null);
                                    }
                                };
                                yield return command;
                            }
                        }
                    }
                }
            }
        }
    }

    /*[HarmonyPatch(typeof(StatExtension), nameof(StatExtension.GetStatValue))]
    public static class GetStatValue_Patch
    {
        private static void Postfix(Thing thing, StatDef stat, bool applyPostProcess, ref float __result)
        {
            if (stat == StatDefOf.ImmunityGainSpeed && thing is Pawn pawn)
            {
                var caravan = pawn.GetCaravan();
                if (caravan != null && !caravan.pather.moving && !caravan.NightResting)
                {
                    var settlement = Find.WorldObjects.SettlementAt(caravan.Tile);
                    if (settlement != null)
                    {
                        foreach (var def in settlement.Faction.GetEnlistOptions())
                        {
                            if (WorldEnlistTracker.Instance.EnlistedTo(settlement.Faction, def))
                            {
                                var comp = settlement.GetComponent<WorldObjectCompEnlist>();
                                var caravanOptions = comp.GetCaravanOptions(caravan);
                                if (caravanOptions?.curWorkOption != null && caravanOptions.curWorkOption.immunityGainSpeedMultiplier.HasValue)
                                {
                                    __result *= caravanOptions.curWorkOption.immunityGainSpeedMultiplier.Value;
                                }
                            }
                        }
                    }
                }
            }
        }
    } Removed due to replacing with Statpart */

    [HarmonyPatch(typeof(TransferableUIUtility), nameof(TransferableUIUtility.DrawCaptiveTradeInfo))]
    public static class TransferableUIUtility_Patch
    {
        private static bool Prefix(Transferable trad, ITrader trader, Rect rect, ref float curX)
        {
            if (trader is PawnTrader)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(CaravanVisitUtility), nameof(CaravanVisitUtility.TradeCommand))]
    public static class CaravanVisitUtility_TradeCommand_Patch
    {
        public static void Postfix(ref Command __result, Caravan caravan, Faction faction = null, TraderKindDef trader = null)
        {
            if (faction != null)
            {
                foreach (var option in faction.GetEnlistOptions())
                {
                    if (WorldEnlistTracker.Instance.EnlistedTo(faction, option) is false)
                    {
                        if (option.settlementTradingLockedBehindEnlist)
                        {
                            if (option.settlementTradingLockedBehindEnlistReasonKey.NullOrEmpty())
                            {
                                __result.Disable();
                            }
                            else
                            {
                                __result.Disable(option.settlementTradingLockedBehindEnlistReasonKey.Translate());
                            }
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(FactionGiftUtility), nameof(FactionGiftUtility.OfferGiftsCommand))]
    public static class FactionGiftUtility_OfferGiftsCommand_Patch
    {
        public static void Postfix(ref Command __result, Caravan caravan, Settlement settlement)
        {
            if (settlement.Faction != null)
            {
                foreach (var option in settlement.Faction.GetEnlistOptions())
                {
                    if (WorldEnlistTracker.Instance.EnlistedTo(settlement.Faction, option) is false)
                    {
                        if (option.settlementGiftingLockedBehindEnlist)
                        {
                            if (option.settlementGiftingLockedBehindEnlistReasonKey.NullOrEmpty())
                            {
                                __result.Disable();
                            }
                            else
                            {
                                __result.Disable(option.settlementGiftingLockedBehindEnlistReasonKey.Translate());
                            }
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(GuestUtility), "IsSellingToSlavery")]
    public static class GuestUtility_IsSellingToSlavery_Patch
    {
        public static bool Prefix()
        {
            if (TradeSession.trader is PawnTrader pawnTrader 
                && pawnTrader.factionOptionDef?.turnInTraderKind == pawnTrader.TraderKind)
            {
                return false;
            }
            return true;
        }
    }
}
