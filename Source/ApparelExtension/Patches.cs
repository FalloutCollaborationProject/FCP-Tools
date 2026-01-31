using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using JetBrains.Annotations;
using Verse;

namespace FCP.ApparelExtensions;

[UsedImplicitly]
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

[UsedImplicitly]
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

[UsedImplicitly]
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

[UsedImplicitly]
[HarmonyPatch(typeof(ApparelGraphicRecordGetter), "TryGetGraphicApparel")]
public static class ApparelGraphicRecordGetter_TryGetGraphicApparel_Patch
{
    public static void Prefix(Apparel apparel, ref BodyTypeDef bodyType)
    {
        var pawn = apparel.Wearer;
        if (pawn != null)
        {
            foreach (var apparel2 in pawn.apparel.WornApparel)
            {
                var extension = apparel2.def.GetModExtension<ApparelExtension>();
                if (extension != null && extension.displayBodyType != null)
                {
                    bodyType = extension.displayBodyType;
                    break;
                }
            }
        }
    }
}