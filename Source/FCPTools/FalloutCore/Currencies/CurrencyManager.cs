using HarmonyLib;

namespace FCP.Currencies;

[StaticConstructorOnStartup]
public static class CurrencyManager
{
    public static readonly ThingDef defaultCurrencyDef;

    private static readonly AccessTools.FieldRef<StockGenerator_SingleDef, ThingDef> ThingDefRef =
        AccessTools.FieldRefAccess<StockGenerator_SingleDef, ThingDef>("thingDef");

    static CurrencyManager()
    {
        defaultCurrencyDef = DefDatabase<ThingDef>.GetNamedSilentFail("FCP_Currency_Caps") ?? ThingDefOf.Silver;
        if (defaultCurrencyDef == ThingDefOf.Silver) return;

        foreach (TraderKindDef traderKind in DefDatabase<TraderKindDef>.AllDefs)
        {
            List<StockGenerator> toAdd = null;
            for (int i = 0; i < traderKind.stockGenerators.Count; i++)
            {
                if (traderKind.stockGenerators[i] is not StockGenerator_SingleDef sg) continue;
                if (ThingDefRef(sg) != ThingDefOf.Silver) continue;
                var caps = new StockGenerator_SingleDef { countRange = sg.countRange };
                ThingDefRef(caps) = defaultCurrencyDef;
                (toAdd ??= new List<StockGenerator>()).Add(caps);
            }
            if (toAdd != null)
                traderKind.stockGenerators.AddRange(toAdd);
        }
    }

    public static bool TryGetCurrency(this ITrader trader, out ThingDef currency)
    {
        if (trader.TraderKind.TryGetCurrency(out currency)) return true;
        if (trader.Faction.TryGetCurrency(out currency)) return true;
        currency = null;
        return false;
    }

    public static bool TryGetCurrency(this Faction faction, out ThingDef currency)
    {
        CurrencyReplacement ext = faction?.def.GetModExtension<CurrencyReplacement>();
        currency = ext?.currency;
        return ext != null;
    }

    public static bool TryGetCurrency(this TraderKindDef traderKind, out ThingDef currency)
    {
        CurrencyReplacement ext = traderKind?.GetModExtension<CurrencyReplacement>();
        currency = ext?.currency;
        return ext != null;
    }

    public static void SwapCurrency(ThingDef newDef)
    {
        ThingDefOf.Silver = newDef;
    }
}