using HarmonyLib;

namespace FCP.Core;

[UsedImplicitly]
[HarmonyPatch(typeof(ApparelGraphicRecordGetter), nameof(ApparelGraphicRecordGetter.TryGetGraphicApparel))]
public static class ApparelGraphicRecordGetter_TryGetGraphicApparel_Patch
{
    public static void Prefix(Apparel apparel, ref BodyTypeDef bodyType)
    {
        BodyTypeDef overrideType = apparel.def.GetDisplayBodyType();
        bodyType = overrideType ?? bodyType;
    }
}

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
                    requests.Add(new PawnGraphicDrawRequest(node));
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
                    requests.Add(new PawnGraphicDrawRequest(node));
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
            Apparel headgear = parms.pawn.apparel.WornApparel
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
    // Head-hiding apparel must show in portraits even when "hats only on map" is on.
    [HarmonyInfix(typeof(Prefs), nameof(Prefs.HatsOnlyOnMap), InnerTargetKind.Getter)]
    [HarmonyPostfix]
    public static void HatsOnlyOnMap_Postfix(ref bool __result, [HarmonyOuter] PawnDrawParms parms)
    {
        if (__result && parms.pawn.apparel.AnyApparel
            && parms.pawn.apparel.WornApparel.Any(x => x.def.ShouldHideHead()))
        {
            __result = false;
        }
    }
}
