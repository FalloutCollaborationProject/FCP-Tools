using HarmonyLib;
using RimWorld;
using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace BM_HeavyWeapon
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute
    {
    }

    [HotSwappable]
    [HarmonyPatch(typeof(Command), "GizmoOnGUI")]
    public static class Command_GizmoOnGUI_Patch
    {
        public static void Prefix(Command __instance, out Verb_Shoot_ApparelAmmo __state)
    	{
            __state = null;
            if (__instance is Command_VerbTarget verbTarget && verbTarget.verb is Verb_Shoot_ApparelAmmo verb_Shoot_Apparel
                 && verbTarget.verb.caster is Pawn pawn)
            {
                __state = verb_Shoot_Apparel;
                var comp = GetAvailableApparelAmmo(pawn, verb_Shoot_Apparel, out bool hasAny, out string failReason);
                if (comp is null)
                {
                    if (hasAny)
                    {
                        __instance.Disable(failReason);
                    }
                    else
                    {
                        __instance.Disable(verb_Shoot_Apparel.Props.missingReason);
                    }
                }
            }
    	}

        public static CompApparelReloadable GetAvailableApparelAmmo(this Pawn pawn, Verb_Shoot_ApparelAmmo verb, out bool hasAny, out string failReason)
        {
            hasAny = false;
            failReason = "";
            foreach (var apparel in pawn.apparel.WornApparel)
            {
                if (verb.Props.apparelList.Contains(apparel.def))
                {
                    var comp = apparel.GetComp<CompApparelReloadable>();
                    if (comp != null)
                    {
                        hasAny = true;
                        if (comp.CanBeUsed(out failReason))
                        {
                            return comp;
                        }
                    }
                }
            }
            return null;
        }

        public static void Postfix(Command __instance, Verb_Shoot_ApparelAmmo __state, Vector2 topLeft, float maxWidth)
        {
            if (__state != null)
            {
                Text.Font = GameFont.Tiny; 
                Text.Anchor = TextAnchor.UpperRight;

                var butRect = new Rect(topLeft.x, topLeft.y, __instance.GetWidth(maxWidth), 75f);
                foreach (var apparel in __state.CasterPawn.apparel.WornApparel)
                {
                    if (__state.Props.apparelList.Contains(apparel.def))
                    {
                        var comp = apparel.GetComp<CompApparelReloadable>();
                        if (comp != null)
                        {
                            var topRightLabel = comp.LabelRemaining;
                            Vector2 vector2 = Text.CalcSize(topRightLabel);
                            Rect position;
                            Rect rect2 = (position = new Rect(butRect.xMax - vector2.x - 2f, butRect.y + 3f, vector2.x, vector2.y));
                            position.x -= 2f;
                            position.width += 3f;
                            GUI.DrawTexture(position, TexUI.GrayTextBG);
                            Widgets.Label(rect2, topRightLabel);
                            butRect.y += rect2.height;
                        }
                    }
                }
                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Small;
            }
        }
    }
}
