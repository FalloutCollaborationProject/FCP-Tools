using System.Text;
using FCP.Core.Stats;
using HarmonyLib;

namespace FCP.Core.Vats;

[HarmonyPatch(typeof(Thing))]
public static class Thing_Patch
{
    [HarmonyPatch(nameof(Thing.PostQualitySet))]
    [HarmonyPostfix]
    public static void PostQualitySet_Patch(Thing __instance)
    {
        QualityCategory cat;

        if (!__instance.TryGetQuality(out cat) || cat != QualityCategory.Legendary)
        {
            return;
        }

        if (LegendaryEffectGameTracker.HasEffect(__instance))
            return;

        LegendaryEffectGameTracker.AddNewLegendaryEffectFor(__instance);
    }

    [HarmonyPatch("Label", MethodType.Getter)]
    [HarmonyPostfix]
    public static void Label_Patch(Thing __instance, ref string __result)
    {
        if (!LegendaryEffectGameTracker.HasEffect(__instance))
        {
            return;
        }

        StringBuilder sb = new StringBuilder();

        foreach (LegendaryEffectDef legendaryEffectDef in LegendaryEffectGameTracker.EffectsDict[__instance])
        {
            sb.Append($"{legendaryEffectDef.LabelCap} ");
        }

        sb.Append(__result);

        __result = sb.ToString();
    }

    [HarmonyPatch(nameof(Thing.GetInspectString))]
    [HarmonyPostfix]
    public static void GetInspectString_Patch(Thing __instance, ref string __result)
    {
        string effectDesc = LegendaryEffectGameTracker.GetEffectDescription(__instance);

        if (effectDesc == null)
        {
            return;
        }

        StringBuilder sb = new StringBuilder(__result);

        sb.AppendLine("FCP_VATS_LegendaryStats".Translate());
        sb.AppendLine(effectDesc);

        __result = LegendaryEffectGameTracker.RemoveEmptyLines(sb.ToString());
    }

    [HarmonyPatch(nameof(Thing.SpecialDisplayStats))]
    [HarmonyPostfix]
    public static void SpecialDisplayStats_Postfix(Thing __instance, ref IEnumerable<StatDrawEntry> __result)
    {
        if (!LegendaryEffectGameTracker.HasEffect(__instance))
        {
            return;
        }

        List<LegendaryEffectDef> effects = LegendaryEffectGameTracker.EffectsDict[__instance].Where(eff => !eff.statOffsets.NullOrEmpty()).ToList();

        List<StatDrawEntry> output = __result.ToList();

        foreach (LegendaryEffectDef effectDef in effects)
        {
            foreach (StatModifier statMod in effectDef.statOffsets)
            {
                StatDef stat = statMod.stat;
                StringBuilder stringBuilder = new StringBuilder();
                float val = statMod.value;

                if (stat.Worker != null)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine("StatsReport_BaseValue".Translate() + ": " + stat.ValueToString(val, ToStringNumberSense.Offset, stat.finalizeEquippedStatOffset));
                    val = StatWorker.StatOffsetFromGear(__instance, stat);
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine("StatsReport_FinalValue".Translate() + ": " + stat.ValueToString(val, ToStringNumberSense.Offset, !stat.formatString.NullOrEmpty()));
                }

                IEnumerable<Dialog_InfoCard.Hyperlink> hl = Gen.YieldSingle(new Dialog_InfoCard.Hyperlink(effectDef));

                output.Add(
                    new LegendaryStatDrawEntry(
                        effectDef,
                        FCPDefOf.FCP_LegendaryEffectStats,
                        stat,
                        statMod.value,
                        StatRequest.ForEmpty(),
                        ToStringNumberSense.Offset,
                        forceUnfinalizedMode: true,
                        hyperlinks: hl
                    ).SetReportText(stringBuilder.ToString())
                );
            }

            foreach (StatModifier statMod in effectDef.statFactors)
            {
                StatDef stat = statMod.stat;
                StringBuilder stringBuilder = new StringBuilder();
                float val = statMod.value;

                if (stat.Worker != null)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine("StatsReport_BaseValue".Translate() + ": " + stat.ValueToString(val, ToStringNumberSense.Factor, stat.finalizeEquippedStatOffset));
                    val = StatWorker.StatOffsetFromGear(__instance, stat);
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine("StatsReport_FinalValue".Translate() + ": " + stat.ValueToString(val, ToStringNumberSense.Factor, !stat.formatString.NullOrEmpty()));
                }

                IEnumerable<Dialog_InfoCard.Hyperlink> hl = Gen.YieldSingle(new Dialog_InfoCard.Hyperlink(effectDef));

                output.Add(
                    new LegendaryStatDrawEntry(
                        effectDef,
                        FCPDefOf.FCP_LegendaryEffectStats,
                        stat,
                        statMod.value,
                        StatRequest.ForEmpty(),
                        ToStringNumberSense.Factor,
                        forceUnfinalizedMode: true,
                        hyperlinks: hl
                    ).SetReportText(stringBuilder.ToString())
                );
            }
        }

        __result = output.AsEnumerable();
    }

    [HarmonyPatch(nameof(Thing.GetGizmos))]
    [HarmonyPostfix]
    public static void GetGizmos(Thing __instance, ref IEnumerable<Gizmo> __result)
    {
        List<Gizmo> gizmos = __result.ToList();

        if (LegendaryEffectGameTracker.HasEffect(__instance) && DebugSettings.godMode)
        {
            Command_Action rerollEffect = new Command_Action();
            rerollEffect.defaultLabel = "DEV: Reroll Legendary Effect";

            rerollEffect.action = delegate
            {
                LegendaryEffectGameTracker.Reroll(__instance);
            };

            gizmos.Add(rerollEffect);

            Command_Action changeEffect = new Command_Action();
            changeEffect.defaultLabel = "DEV: Change Legendary Effect";

            changeEffect.action = delegate
            {
                LegendaryEffectGameTracker.MakeChangeEffectFloatMenu(__instance);
            };

            gizmos.Add(changeEffect);
        }

        __result = gizmos.AsEnumerable();
    }
}
