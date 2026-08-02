using HarmonyLib;
using UnityEngine;

namespace FCP.Core.WeaponCondition;

[HarmonyPatch(typeof(ITab_Pawn_Gear), "DrawThingRow")]
public static class ITabPawnGear_DrawThingRow_Patch
{
    private const float BarHeight = 3f;
    private const float BarGap = 1f;

    private static readonly Texture2D HitPointsTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.4f, 0.7f, 0.3f));
    private static readonly Texture2D ConditionTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.8f, 0.7f, 0.2f));

    public static void Postfix(ref float y, float width, Thing thing)
    {
        if (!FCPCoreMod.Settings.General.weaponConditionEnabled) return;
        if (thing is not ThingWithComps twc) return;
        CompWeaponCondition cond = twc.GetComp<CompWeaponCondition>();
        if (cond == null) return;

        float hpPct = thing.def.useHitPoints ? thing.HitPoints / (float)thing.MaxHitPoints : 1f;

        Rect hpRect = new Rect(4f, y, width - 4f, BarHeight);
        Widgets.FillableBar(hpRect, hpPct, HitPointsTex, BaseContent.BlackTex, doBorder: false);
        y += BarHeight + BarGap;

        Rect condRect = new Rect(4f, y, width - 4f, BarHeight);
        Widgets.FillableBar(condRect, cond.Condition / 100f, ConditionTex, BaseContent.BlackTex, doBorder: false);
        if (Mouse.IsOver(condRect))
            TooltipHandler.TipRegion(condRect, "FCP_WeaponCondition_Condition".Translate(cond.Condition.ToString("F0")));
        y += BarHeight + BarGap;
    }
}
