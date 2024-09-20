using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FalloutCurrencies
{
    public class FactionCurrency : DefModExtension
    {
        public ThingDef currency;
    }

    [HarmonyPatch(typeof(TradeSession), "SetupWith")]
    public static class SetupWith_Patch
    {
        public static void Postfix(ITrader newTrader, Pawn newPlayerNegotiator, bool giftMode)
        {
            var faction = newTrader.Faction;
            if (faction.TryGetCurrency(out var currency))
            {
                CurrencyManager.SwapCurrency(currency);
            }
            else if (ThingDefOf.Silver != CurrencyManager.defaultCurrencyDef)
            {
                CurrencyManager.SwapCurrency(CurrencyManager.defaultCurrencyDef);
            }
        }
    }

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

    [HarmonyPatch(typeof(StockGenerator_SingleDef), "HandlesThingDef")]
    public static class HandlesThingDef_Patch
    {
        public static void Postfix(StockGenerator_SingleDef __instance, ref bool __result)
        {
            if (__instance.thingDef == CurrencyManager.defaultCurrencyDef && __instance.thingDef != ThingDefOf.Silver)
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(StockGenerator_SingleDef), "GenerateThings")]
    public static class GenerateThings_Patch
    {
        public static void Prefix(StockGenerator_SingleDef __instance, int forTile, Faction faction = null)
        {
            if (__instance.thingDef == CurrencyManager.defaultCurrencyDef && faction.TryGetCurrency(out var currency))
            {
                __instance.thingDef = currency;
            }
        }
        public static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, StockGenerator_SingleDef __instance, int forTile, Faction faction = null)
        {
            foreach (var thing in __result)
            {
                if (faction.TryGetCurrency(out var currency) && __instance.thingDef == currency)
                {
                    __instance.thingDef = CurrencyManager.defaultCurrencyDef;
                }
                yield return thing;
            }
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
            ReplaceSilverWithBottleCaps();
        }

        public static bool TryGetCurrency(this Faction faction, out ThingDef currency)
        {
            var extension = faction?.def.GetModExtension<FactionCurrency>();
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
            ThingDefOf.Silver = newDef;
        }
        private static void ReplaceSilverWithBottleCaps()
        {
            var replacedSilver = DefDatabase<ThingDef>.GetNamedSilentFail("ReplacedSilver");
            if (replacedSilver != null)
            {
                replacedSilver.label = ThingDefOf.Silver.label;
                replacedSilver.description = ThingDefOf.Silver.description;

                var dummyBottleCaps = ThingDef.Named("DummyBottleCaps");
                ThingDefOf.Silver.label = dummyBottleCaps.label;
                ThingDefOf.Silver.description = dummyBottleCaps.description;

                List<ThingDef> things = DefDatabase<ThingDef>.AllDefsListForReading;
                foreach (ThingDef thing in things)
                {
                    var silverValue = thing?.costList?.Where(x => x.thingDef == ThingDefOf.Silver)?.FirstOrDefault();
                    if (silverValue != null)
                    {
                        ThingDefCountClass newValue = new ThingDefCountClass();
                        newValue.thingDef = replacedSilver;
                        newValue.count = silverValue.count;
                        thing.costList.Add(newValue);
                        thing.costList.Remove(silverValue);
                    }
                }

                List<TerrainDef> terrains = DefDatabase<TerrainDef>.AllDefsListForReading;
                foreach (TerrainDef terrain in terrains)
                {
                    var silverValue = terrain?.costList?.Where(x => x.thingDef == ThingDefOf.Silver)?.FirstOrDefault();
                    if (silverValue != null)
                    {
                        ThingDefCountClass newValue = new ThingDefCountClass();
                        newValue.thingDef = ThingDef.Named("ReplacedSilver");
                        newValue.count = silverValue.count;
                        terrain.costList.Add(newValue);
                        terrain.costList.Remove(silverValue);
                    }
                }

                List<RecipeDef> recipes = DefDatabase<RecipeDef>.AllDefsListForReading;
                foreach (RecipeDef recipe in recipes)
                {
                    foreach (var ingredient in recipe?.ingredients)
                    {
                        var silverValue = ingredient.filter.AllowedThingDefs?.Where(x => x == ThingDefOf.Silver)?.FirstOrDefault();
                        if (silverValue != null)
                        {
                            ingredient.filter.AllowedThingDefs.ToList().Add(ThingDef.Named("ReplacedSilver"));
                            ingredient.filter.AllowedThingDefs.ToList().Remove(ThingDefOf.Silver);
                        }
                    }
                }

                ThingDefOf.Silver.stuffProps.categories.Remove(StuffCategoryDefOf.Metallic);
                var dummy = DefDatabase<StuffCategoryDef>.GetNamed("DummyMetallic", true);
                ThingDefOf.Silver.stuffProps.categories.Add(dummy);
            }
        }
    }
}

