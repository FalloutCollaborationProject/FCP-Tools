using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld.Planet;

namespace FCP.Factions;

[HarmonyPatch]
public static class WorldFactionsUIUtility_CanAddFaction_Patch
{
    private static MethodBase FindTarget()
    {
        Type root = typeof(WorldFactionsUIUtility);
        foreach (Type type in root.GetNestedTypes(AccessTools.all).Prepend(root))
        {
            foreach (MethodInfo method in AccessTools.GetDeclaredMethods(type))
            {
                if (method.Name.Contains("CanAddFaction"))
                    return method;
            }
        }
        return null;
    }

    public static bool Prepare() => FindTarget() != null;

    public static MethodBase TargetMethod() => FindTarget();

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (CodeInstruction instruction in instructions)
        {
            if (instruction.LoadsConstant(12))
            {
                instruction.opcode = OpCodes.Ldc_I4;
                instruction.operand = int.MaxValue;
            }
            yield return instruction;
        }
    }
}
