using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FalloutCore
{
    public class ApparelExtension : DefModExtension
    {
        public bool shouldHideBody;
        public bool shouldHideHead;
    }

    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        static HarmonyInit()
        {
            new Harmony("FalloutCore.Mod").PatchAll();
        }

        private static Dictionary<ThingDef, ApparelExtension> cachedExtensions = new Dictionary<ThingDef, ApparelExtension>();
        public static bool ShouldHideBody(this ThingDef def)
        {
            if (!cachedExtensions.TryGetValue(def, out var extension))
            {
                cachedExtensions[def] = extension = def.GetModExtension<ApparelExtension>();
            }
            if (extension != null && extension.shouldHideBody)
            {
                return true;
            }
            return false;
        }

        public static bool ShouldHideHead(this ThingDef def)
        {
            if (!cachedExtensions.TryGetValue(def, out var extension))
            {
                cachedExtensions[def] = extension = def.GetModExtension<ApparelExtension>();
            }
            if (extension != null && extension.shouldHideHead)
            {
                return true;
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(PawnGraphicSet), "MatsBodyBaseAt")]
    public static class PawnGraphicSet_MatsBodyBaseAt_Test_Patch
    {
        [HarmonyPostfix, HarmonyPriority(Priority.Last)]
        public static void Postfix(PawnGraphicSet __instance, ref List<Material> __result)
        {
            Pawn pawn = __instance.pawn;
            if (!pawn.RaceProps.Humanlike)
            {
                return;
            }
            if (pawn.apparel.AnyApparel)
            {
                if (pawn.apparel.WornApparel.Any(x => x.def.ShouldHideBody()))
                {
                    for (int i = 0; i < __result.Count; i++)
                    {
                        __result[i] = BaseContent.ClearMat;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), "DrawHeadHair")]
    public static class DrawHeadHair_Patch
    {
        public static bool Prefix(Pawn ___pawn, Vector3 rootLoc, Vector3 headOffset, float angle, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags)
        {
            Pawn pawn = ___pawn;
            if (pawn.apparel.AnyApparel)
            {
                if (pawn.apparel.WornApparel.Any(x => x.def.ShouldHideHead()))
                {
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PawnGraphicSet), "HairMatAt")]
    public static class PawnGraphicSet_HairMatAt_Patch
    {
        public static void Postfix(PawnGraphicSet __instance, ref Material __result, Rot4 facing, bool portrait = false, bool cached = false)
        {
            Pawn pawn = __instance.pawn;
            if (pawn.apparel.AnyApparel && !portrait)
            {
                if (pawn.apparel.WornApparel.Any(x => x.def.ShouldHideHead()))
                {
                    __result = BaseContent.ClearMat;
                }
            }
        }
    }

    [HarmonyPatch(typeof(PawnGraphicSet), "HeadMatAt")]
    public static class PawnGraphicSet_HeadMatAt_Patch
    {
        public static void Postfix(PawnGraphicSet __instance, ref Material __result, Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh, bool stump = false, bool portrait = false, bool allowOverride = true)
        {
            Pawn pawn = __instance.pawn;
            if (pawn.apparel.AnyApparel && !portrait)
            {
                if (pawn.apparel.WornApparel.Any(x => x.def.ShouldHideHead()))
                {
                    __result = BaseContent.ClearMat;
                }
            }
        }
    }
}
