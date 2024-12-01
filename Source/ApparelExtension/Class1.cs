using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
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
        public BodyTypeDef displayBodyType;
    }

    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        static HarmonyInit()
        {
            new Harmony("FalloutCore.ApparelExtension").PatchAll();
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

    [HarmonyPatch(typeof(PawnRenderNodeWorker), "AppendDrawRequests")]
    public static class PawnRenderNodeWorker_AppendDrawRequests_Patch
    {
        public static bool Prefix(PawnRenderNode node, PawnDrawParms parms, List<PawnGraphicDrawRequest> requests)
        {
            if ((node is PawnRenderNode_Head || node.parent is PawnRenderNode_Head) && parms.pawn.apparel.AnyApparel)
            {
                foreach (var apparel in parms.pawn.apparel.WornApparel)
                {
                    if (apparel.def.ShouldHideHead())
                    {
                        requests.Add(new PawnGraphicDrawRequest(node)); // adds an empty draw request to not draw head
                        return false;
                    }
                }
            }
            if ((node is PawnRenderNode_Body || node.parent is PawnRenderNode_Body) && parms.pawn.apparel.AnyApparel)
            {
                foreach (var apparel in parms.pawn.apparel.WornApparel)
                {
                    if (apparel.def.ShouldHideBody())
                    {
                        requests.Add(new PawnGraphicDrawRequest(node)); // adds an empty draw request to not draw body
                        return false;
                    }
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PawnRenderNodeWorker_Apparel_Head), "CanDrawNow")]
    public static class PawnRenderNodeWorker_Apparel_Head_CanDrawNow_Patch
    {
        public static void Prefix(PawnDrawParms parms, out bool __state)
        {
            __state = Prefs.HatsOnlyOnMap;
            if (parms.pawn.apparel.AnyApparel)
            {
                var headgear = parms.pawn.apparel.WornApparel
                    .FirstOrDefault(x => x.def.ShouldHideHead());
                if (headgear != null)
                {
                    Prefs.HatsOnlyOnMap = false;
                }
            }
        }

        public static void Postfix(bool __state)
        {
            Prefs.HatsOnlyOnMap = __state;
        }
    }

    [HarmonyPatch(typeof(PawnRenderNodeWorker_Apparel_Head), "HeadgearVisible")]
    public static class PawnRenderNodeWorker_Apparel_Head_HeadgearVisible_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var get_HatsOnlyOnMap = AccessTools.PropertyGetter(typeof(Prefs), nameof(Prefs.HatsOnlyOnMap));
            foreach (var codeInstruction in codeInstructions)
            {
                yield return codeInstruction;
                if (codeInstruction.Calls(get_HatsOnlyOnMap))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(PawnRenderNodeWorker_Apparel_Head_HeadgearVisible_Patch),
                        "TryOverrideHatsOnlyOnMap"));
                }
            }
        }

        public static bool TryOverrideHatsOnlyOnMap(bool result, PawnDrawParms parms)
        {
            if (result is true && parms.pawn.apparel.AnyApparel)
            {
                var headgear = parms.pawn.apparel.WornApparel
                    .FirstOrDefault(x => x.def.ShouldHideHead());
                if (headgear != null)
                {
                    return false;
                }
            }
            return result;
        }
    }

    [HarmonyPatch(typeof(ApparelGraphicRecordGetter), "TryGetGraphicApparel")]
    public static class ApparelGraphicRecordGetter_TryGetGraphicApparel_Patch
    {
        public static void Prefix(Apparel apparel, ref BodyTypeDef bodyType)
        {
            var pawn = apparel.Wearer;
            if (pawn != null)
            {
                var extension = apparel.def.GetModExtension<ApparelExtension>();
                if (extension != null && extension.displayBodyType != null)
                {
                    bodyType = extension.displayBodyType;
                }
            }
        }
    }
}
