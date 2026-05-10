using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace FCP.Core;

[StaticConstructorOnStartup]
public static class IgnoreConfigErrors_HarmonyInit
{
    static IgnoreConfigErrors_HarmonyInit()
    {
        Log.Message("[FCP.Core.IgnoreConfigErrors] Initializing...");
        
        var harmony = new HarmonyLib.Harmony("FCP.Core.IgnoreConfigErrors");
        
        Type iteratorType = typeof(CompProperties_AbilityStopMentalState)
            .GetNestedTypes(AccessTools.all)
            .FirstOrDefault(t => t.Name.Contains("<ConfigErrors>"));
        harmony.Patch(
            AccessTools.Method(iteratorType, "MoveNext"),
            transpiler: new HarmonyMethod(typeof(IgnoreConfigErrors_HarmonyInit), nameof(Patch_AbilityStopMentalState))
        );
        
        Type verbIteratorType = typeof(VerbProperties)
            .GetNestedTypes(AccessTools.all)
            .FirstOrDefault(t => t.Name.Contains("<ConfigErrors>"));
        harmony.Patch(
            AccessTools.Method(verbIteratorType, "MoveNext"),
            transpiler: new HarmonyMethod(typeof(IgnoreConfigErrors_HarmonyInit), nameof(Patch_VerbProperties))
        );
        
        Log.Message("[FCP.Core.IgnoreConfigErrors] Harmony patches complete!");
    }

    private static IEnumerable<CodeInstruction> Patch_AbilityStopMentalState(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = instructions.ToList();
        int patchCount = 0;
        
        FieldInfo minorField = AccessTools.Field(typeof(CompProperties_AbilityStopMentalState), "psyfocusCostForMinor");
        FieldInfo majorField = AccessTools.Field(typeof(CompProperties_AbilityStopMentalState), "psyfocusCostForMajor");
        FieldInfo extremeField = AccessTools.Field(typeof(CompProperties_AbilityStopMentalState), "psyfocusCostForExtreme");
        
        Type iteratorType = typeof(CompProperties_AbilityStopMentalState)
            .GetNestedTypes(AccessTools.all)
            .First(t => t.Name.Contains("<ConfigErrors>"));
        FieldInfo parentDefField = iteratorType
            .GetFields(AccessTools.all)
            .First(t => t.Name.Contains("parentDef"));
        
        for (int i = 0; i < codes.Count; i++)
        {
            if ((codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == minorField) ||
                (codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == majorField) ||
                (codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == extremeField))
            {
                codes.InsertRange(i + 3, new[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, parentDefField),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(IgnoreConfigErrors_HarmonyInit), nameof(HasModExt_AbilityStopMentalState))),
                    new CodeInstruction(OpCodes.Brtrue_S, codes[i + 2].operand)
                });
                patchCount++;
            }
        }
        
        if (patchCount < 3)
        {
            Log.Error($"[FCP.Core.IgnoreConfigErrors] Patch_AbilityStopMentalState failed! (patchCount: {patchCount})");
        }
        
        return codes;
    }

    private static bool HasModExt_AbilityStopMentalState(AbilityDef parentDef)
    {
        return parentDef.HasModExtension<Ignore_AbilityStopMentalState>();
    }

    private static IEnumerable<CodeInstruction> Patch_VerbProperties(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = instructions.ToList();
        int patchCount = 0;
        
        MethodInfo launchesProjectileGetter = AccessTools.PropertyGetter(typeof(VerbProperties), "LaunchesProjectile");
        
        Type iteratorType = typeof(VerbProperties)
            .GetNestedTypes(AccessTools.all)
            .FirstOrDefault(t => t.Name.Contains("<ConfigErrors>"));
        FieldInfo parentField = iteratorType
            .GetFields(AccessTools.all)
            .First(t => t.Name.Contains("parent"));
        
        for (int i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == launchesProjectileGetter)
            {
                codes.InsertRange(i + 2, new[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, parentField),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(IgnoreConfigErrors_HarmonyInit), nameof(HasModExt_ForcedMissRadius))),
                    new CodeInstruction(OpCodes.Brtrue_S, codes[i + 1].operand)
                });
                patchCount++;
                break;
            }
        }
        
        if (patchCount < 1)
        {
            Log.Error("[FCP.Core.IgnoreConfigErrors] Patch_VerbProperties failed!");
        }
        
        return codes;
    }

    private static bool HasModExt_ForcedMissRadius(ThingDef parentDef)
    {
        return parentDef.HasModExtension<Ignore_ForcedMissRadius>();
    }
}
