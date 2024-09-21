using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace FalloutCurrencies_NonReplacement
{
    public class CurrencyReplacement : DefModExtension
    {
        public ThingDef currency;

        public static CurrencyReplacement Get(Def def)
        {
            return def.GetModExtension<CurrencyReplacement>();
        }
    }
    /// <summary>
    /// Swaps the currency the trade uses
    /// Without this the trade caravan will still generate with their custom currency,
    /// however the actual trade will still use silver
    /// </summary>
    [HarmonyPatch(typeof(TradeSession), "SetupWith")]
    public static class SetupWith_Patch
    {
        public static void Postfix(ITrader newTrader, Pawn newPlayerNegotiator, bool giftMode)
        {
            if (newTrader.TryGetCurrency(out var currency))
            {
                CurrencyManager.SwapCurrency(currency);
            }
            else if (ThingDefOf.Silver != CurrencyManager.defaultCurrencyDef)
            {
                CurrencyManager.SwapCurrency(CurrencyManager.defaultCurrencyDef);
            }
        }
    }
    /// <summary>
    /// Doesn't seem to actually ever be run
    /// </summary>
    [HarmonyPatch(typeof(TradeSession), "Close")]
    public static class Close_Patch
    {
        public static void Prefix()
        {
            if (ThingDefOf.Silver != CurrencyManager.defaultCurrencyDef)
            {
                CurrencyManager.SwapCurrency(CurrencyManager.defaultCurrencyDef);
            }
        }
    }

    [HarmonyPatch(typeof(Tradeable), "TraderWillTrade", MethodType.Getter)]
    public static class Tradeable_TraderWillTrade_Patch
    {
        public static void Postfix(Tradeable __instance, ref bool __result)
        {
            if (TradeSession.trader.TryGetCurrency(out var currency) && __instance.ThingDef == currency)
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(RewardsGenerator), nameof(RewardsGenerator.DoGenerate))]
    public static class RewardsGenerator_DoGenerate_Patch
    {
        public static void Prefix(RewardsGeneratorParams parms, out ThingDef __state)
        {
            __state = null;
            if (parms.giverFaction != null && parms.giverFaction.TryGetCurrency(out var currency) 
                && RewardsGenerator.MarketValueFillers.Contains(currency) is false)
            {
                __state = currency;
                RewardsGenerator.MarketValueFillers.Add(currency);
            }
        }

        public static void Postfix(ThingDef __state)
        {
            if (__state != null)
            {
                RewardsGenerator.MarketValueFillers.Remove(__state);
            }
        }
    }

    /// <summary>
    /// Replaces all instances of Silver with the factions specified currency
    /// Obviously raises the issue that traders can never actually sell silver, 
    /// as it will always be converted into their own currency
    /// </summary>
    [HarmonyPatch(typeof(StockGenerator_SingleDef), "GenerateThings")]
    public static class GenerateThings_Patch
    {
        public static void Prefix(StockGenerator_SingleDef __instance, ref ThingDef ___thingDef, out ThingDef __state, int forTile, Faction faction = null)
        {
            __state = ___thingDef;
            if (___thingDef == CurrencyManager.defaultCurrencyDef 
                && (faction.TryGetCurrency(out var currency) || __instance.trader.TryGetCurrency(out currency)))
            {
                ___thingDef = currency;
            }
        }
        public static void Postfix(ref ThingDef ___thingDef, ThingDef __state)
        {
            ___thingDef = __state;
        }
    }

    [StaticConstructorOnStartup]
    public static class CurrencyManager
    {
        public static ThingDef defaultCurrencyDef;
        static CurrencyManager()
        {
            defaultCurrencyDef = ThingDefOf.Silver;
            new Harmony("FalloutCurrencies.Mod").PatchAll();
            foreach (var traderKind in DefDatabase<TraderKindDef>.AllDefs)
            {
                var stock = traderKind.stockGenerators.FirstOrDefault(x => x is StockGenerator_SingleDef singleDef
                && singleDef.thingDef == ThingDefOf.Gold);
                if (stock != null)
                {
                    traderKind.stockGenerators.Add(new StockGenerator_SingleDef
                    {
                        thingDef = defaultCurrencyDef,
                        countRange = new IntRange(stock.countRange.min * 2, stock.countRange.max * 2)
                    });
                }
            }
        }

        public static bool TryGetCurrency(this ITrader trader, out ThingDef currency)
        {
            if (TryGetCurrency(trader.Faction, out currency))
            {
                return true;
            }
            if (TryGetCurrency(trader.TraderKind, out currency))
            {
                return true;
            }
            currency = null;
            return false;
        }

        public static bool TryGetCurrency(this Faction faction, out ThingDef currency)
        {
            var extension = faction?.def.GetModExtension<CurrencyReplacement>();
            if (extension != null)
            {
                currency = extension.currency;
                return true;
            }
            currency = null;
            return false;
        }

        public static bool TryGetCurrency(this TraderKindDef traderKind, out ThingDef currency)
        {
            var extension = traderKind?.GetModExtension<CurrencyReplacement>();
            if (extension != null)
            {
                currency = extension.currency;
                return true;
            }
            currency = null;
            return false;
        }

        public static void SwapCurrency(ThingDef newDef)
        {
            Log.Message("Swapping silver to: " + newDef.label);
            ThingDefOf.Silver = newDef;
            Log.Message("Silver is now: " + ThingDefOf.Silver.label);
        }
    }
}

