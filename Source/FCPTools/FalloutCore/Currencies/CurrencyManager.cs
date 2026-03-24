using HarmonyLib;

namespace FCP.Currencies;

[StaticConstructorOnStartup]
public static class CurrencyManager
{
    public static readonly ThingDef defaultCurrencyDef;
    public static readonly HashSet<StockGenerator_SingleDef> silverStockGenerators = [];

    private static readonly AccessTools.FieldRef<StockGenerator_SingleDef, ThingDef> ThingDefRef =
        AccessTools.FieldRefAccess<StockGenerator_SingleDef, ThingDef>("thingDef");

    static CurrencyManager()
    {
        defaultCurrencyDef = ThingDefOf.Silver;
        foreach (TraderKindDef traderKind in DefDatabase<TraderKindDef>.AllDefs)
        {
            StockGenerator goldStock = traderKind.stockGenerators.FirstOrDefault(x => x is StockGenerator_SingleDef singleDef
                                                                       && ThingDefRef(singleDef) == ThingDefOf.Gold);
            if (goldStock != null)
            {
                var silverStock = new StockGenerator_SingleDef
                {
                    countRange = new IntRange(goldStock.countRange.min * 2, goldStock.countRange.max * 2)
                };
                ThingDefRef(silverStock) = defaultCurrencyDef;
                silverStockGenerators.Add(silverStock);
                traderKind.stockGenerators.Add(silverStock);
            }
        }
    }

    [Pure]
    public static bool TryGetCurrency(this ITrader trader, out ThingDef currency)
    {
        if (trader.TraderKind.TryGetCurrency(out currency))
        {
            return true;
        }
        if (trader.Faction.TryGetCurrency(out currency))
        {
            return true;
        }
        currency = null;
        return false;
    }

    [Pure]
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

    [Pure]
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