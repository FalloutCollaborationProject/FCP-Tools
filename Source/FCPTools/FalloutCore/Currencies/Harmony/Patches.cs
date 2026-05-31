using FCP.Core;
using FCP.Currencies;
using HarmonyLib;

namespace FCP.Currency;

[HarmonyPatch(typeof(TradeSession), "SetupWith")]
[HarmonyPatchCategory(FCPCoreMod.CurrencyPatchesCategory)]
public static class SetupWith_Patch
{
    public static void Postfix(ITrader newTrader)
    {
        if (newTrader.TryGetCurrency(out ThingDef currency))
            CurrencyManager.SwapCurrency(currency);
        else if (ThingDefOf.Silver != CurrencyManager.defaultCurrencyDef)
            CurrencyManager.SwapCurrency(CurrencyManager.defaultCurrencyDef);
    }
}

[HarmonyPatch(typeof(TradeSession), "Close")]
[HarmonyPatchCategory(FCPCoreMod.CurrencyPatchesCategory)]
public static class Close_Patch
{
    public static void Prefix()
    {
        if (ThingDefOf.Silver != CurrencyManager.defaultCurrencyDef)
            CurrencyManager.SwapCurrency(CurrencyManager.defaultCurrencyDef);
    }
}

[HarmonyPatch(typeof(Tradeable), "TraderWillTrade", MethodType.Getter)]
[HarmonyPatchCategory(FCPCoreMod.CurrencyPatchesCategory)]
public static class Tradeable_TraderWillTrade_Patch
{
    public static void Postfix(Tradeable __instance, ref bool __result)
    {
        if (TradeSession.trader.TryGetCurrency(out ThingDef currency) && __instance.ThingDef == currency)
            __result = true;
    }
}

[HarmonyPatch(typeof(RewardsGenerator), "DoGenerate")]
[HarmonyPatchCategory(FCPCoreMod.CurrencyPatchesCategory)]
public static class RewardsGenerator_DoGenerate_Patch
{
    private static readonly List<ThingDef> MarketValueFillers =
        AccessTools.StaticFieldRefAccess<List<ThingDef>>(typeof(RewardsGenerator), "MarketValueFillers");

    public static void Prefix(RewardsGeneratorParams parms, out ThingDef __state)
    {
        __state = null;
        if (parms.giverFaction != null && parms.giverFaction.TryGetCurrency(out ThingDef currency)
            && !MarketValueFillers.Contains(currency))
        {
            __state = currency;
            MarketValueFillers.Add(currency);
        }
    }

    public static void Postfix(ThingDef __state)
    {
        if (__state != null)
            MarketValueFillers.Remove(__state);
    }
}

[HarmonyPatch(typeof(Tradeable), "IsCurrency", MethodType.Getter)]
[HarmonyPatchCategory(FCPCoreMod.CurrencyPatchesCategory)]
public static class Tradeable_IsCurrency_Patch
{
    public static void Postfix(Tradeable __instance, ref bool __result)
    {
        if (!__result && TradeSession.trader.TryGetCurrency(out ThingDef currency) && __instance.ThingDef == currency)
            __result = true;
    }
}

[HarmonyPatch(typeof(StockGenerator_SingleDef), "GenerateThings")]
[HarmonyPatchCategory(FCPCoreMod.CurrencyPatchesCategory)]
public static class GenerateThings_Patch
{
    private static readonly AccessTools.FieldRef<StockGenerator_SingleDef, ThingDef> ThingDefRef =
        AccessTools.FieldRefAccess<StockGenerator_SingleDef, ThingDef>("thingDef");

    public static IEnumerable<Thing> Postfix(IEnumerable<Thing> result, StockGenerator_SingleDef __instance, Faction faction = null)
    {
        ThingDef current = ThingDefRef(__instance);
        if (current == CurrencyManager.defaultCurrencyDef
            && (faction.TryGetCurrency(out ThingDef currency) || __instance.trader.TryGetCurrency(out currency)))
        {
            ThingDefRef(__instance) = currency;
        }
        foreach (Thing thing in result)
            yield return thing;
        ThingDefRef(__instance) = current;
    }
}