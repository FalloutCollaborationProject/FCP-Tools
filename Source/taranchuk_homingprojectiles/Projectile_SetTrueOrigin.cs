using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace taranchuk_homingprojectiles
{
    [HarmonyPatch]
    public static class Projectile_SetTrueOrigin
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(Projectile), "CheckForFreeInterceptBetween");
            yield return AccessTools.Method(typeof(Projectile), "CheckForFreeIntercept");
            yield return AccessTools.Method(typeof(Projectile), "ImpactSomething");
        }

        public static MethodInfo InterceptChanceFactorFromDistanceInfo
            = AccessTools.Method(typeof(Verse.VerbUtility), "InterceptChanceFactorFromDistance");

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            var field = AccessTools.Field(typeof(Projectile), nameof(Projectile.origin));
            for (var i = 0; i < codes.Count; i++)
            {
                var codeInstruction = codes[i];
                if (codes.Count - 3 > i && codeInstruction.LoadsField(field) && codes[i + 2].Calls(InterceptChanceFactorFromDistanceInfo))
                {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Projectile_SetTrueOrigin), nameof(GetTrueOrigin)));
                }
                else
                {
                    yield return codeInstruction;
                }
            }
        }

        public static Vector3 GetTrueOrigin(Projectile projectile)
        {
            if (projectile.IsHomingProjectile(out var comp))
            {
                return comp.originLaunchCell;
            }
            return projectile.origin;
        }
    }
}
