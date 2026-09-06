using HarmonyLib;

namespace FCP.Core.Hediffs;

[HarmonyPatch(typeof(Pawn), "ButcherProducts")]
public static class Patch_HiddenButcherYield
{
    public static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, Pawn __instance)
    {
        List<HediffComp_HiddenButcherYield> comps = __instance?.health?.hediffSet?.hediffs
            .OfType<HediffWithComps>()
            .Select(h => h.TryGetComp<HediffComp_HiddenButcherYield>())
            .Where(c => c != null)
            .ToList();

        if (comps.NullOrEmpty())
            return __result;

        IEnumerable<Thing> result = __result;
        foreach (HediffComp_HiddenButcherYield comp in comps)
        {
            if (comp.Props.replacesNormalYield)
                result = Enumerable.Empty<Thing>();

            result = result.Concat(comp.MakeYieldThings());
        }

        return result;
    }
}
