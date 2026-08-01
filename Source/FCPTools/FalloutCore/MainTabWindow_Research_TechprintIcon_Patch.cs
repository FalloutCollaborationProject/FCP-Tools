using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FCP.Core;

[HarmonyPatch(typeof(MainTabWindow_Research), "DrawBottomRow")]
public static class MainTabWindow_Research_TechprintIcon_Patch
{
    static readonly FieldInfo TechprintRequirementTexField = AccessTools.Field(typeof(MainTabWindow_Research), "TechprintRequirementTex");
    static readonly MethodInfo TextureGetter = AccessTools.PropertyGetter(typeof(CachedTexture), nameof(CachedTexture.Texture));
    static readonly MethodInfo IconGetter = AccessTools.Method(typeof(MainTabWindow_Research_TechprintIcon_Patch), nameof(GetTechprintIcon));

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new(instructions);
        for (int i = 0; i < codes.Count - 1; i++)
        {
            if (codes[i].opcode == OpCodes.Ldsfld && TechprintRequirementTexField.Equals(codes[i].operand)
                && (codes[i + 1].opcode == OpCodes.Callvirt || codes[i + 1].opcode == OpCodes.Call)
                && TextureGetter.Equals(codes[i + 1].operand))
            {
                codes[i] = new CodeInstruction(OpCodes.Ldarg_2);
                codes[i + 1] = new CodeInstruction(OpCodes.Call, IconGetter);
                break;
            }
        }
        return codes;
    }

    static Texture2D GetTechprintIcon(ResearchProjectDef project)
    {
        TechprintExtension extension = project.GetModExtension<TechprintExtension>();
        if (extension != null && !extension.texPath.NullOrEmpty())
            return CachedTexture.Get(extension.texPath);
        return ((CachedTexture)TechprintRequirementTexField.GetValue(null)).Texture;
    }
}
