using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace FCP.Currencies;

[StaticConstructorOnStartup]
public static class CurrencyManager
{
    public static ThingDef defaultCurrencyDef;
    public static HashSet<StockGenerator_SingleDef> silverStockGenerators = new HashSet<StockGenerator_SingleDef>();
    static CurrencyManager()
    {
        defaultCurrencyDef = ThingDefOf.Silver;
        foreach (var traderKind in DefDatabase<TraderKindDef>.AllDefs)
        {
            var stock = traderKind.stockGenerators.FirstOrDefault(x => x is StockGenerator_SingleDef singleDef
                                                                       && singleDef.thingDef == ThingDefOf.Gold);
            if (stock != null)
            {
                var silverStock = new StockGenerator_SingleDef
                {
                    thingDef = defaultCurrencyDef,
                    countRange = new IntRange(stock.countRange.min * 2, stock.countRange.max * 2)
                };
                silverStockGenerators.Add(silverStock);
                traderKind.stockGenerators.Add(silverStock);
            }
        }
        new Harmony("FCPCurrencies").PatchAll();
    }

    public static bool TryGetCurrency(this ITrader trader, out ThingDef currency)
    {
        if (TryGetCurrency(trader.TraderKind, out currency))
        {
            return true;
        }
        if (TryGetCurrency(trader.Faction, out currency))
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
        ThingDefOf.Silver = newDef;
    }
}