using System.Reflection;
using FCP.Core.Unity;
using HarmonyLib;
using UnityEngine;

namespace FCP.Core.Vats;

[HarmonyPatch(typeof(ShaderTypeDef))]
public static class ShaderTypeDef_Patch
{
    public static Lazy<FieldInfo> ShaderIntFI = new Lazy<FieldInfo>(() => AccessTools.Field(typeof(ShaderTypeDef), "shaderInt"));

    [HarmonyPatch(nameof(ShaderTypeDef.Shader), MethodType.Getter)]
    [HarmonyPrefix]
    public static bool Shader(ShaderTypeDef __instance, ref Shader __result)
    {
        if (!__instance.HasModExtension<ModExtension_Shader>()) return true;

        if (ShaderIntFI.Value.GetValue(__instance) is null)
        {
            ShaderIntFI.Value.SetValue(__instance, Shaders.LoadShader(__instance.shaderPath));
        }

        return true;
    }
}
