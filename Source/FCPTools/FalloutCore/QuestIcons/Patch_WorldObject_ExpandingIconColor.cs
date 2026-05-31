using HarmonyLib;
using RimWorld.Planet;
using UnityEngine;

namespace FCP.Core.QuestIcons;

[HarmonyPatch(typeof(WorldObject), nameof(WorldObject.ExpandingIconColor), MethodType.Getter)]
static class Patch_WorldObject_ExpandingIconColor
{
    static void Postfix(WorldObject __instance, ref Color __result)
    {
        if (__instance is Site site && site.MainSitePartDef != null && 
            site.MainSitePartDef.defName.StartsWith("FCP_"))
        {
            __result = QuestIconColorManager.GetColor();
        }
    }
}
