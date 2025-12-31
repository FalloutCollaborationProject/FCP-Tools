using System.Reflection.Emit;
using HarmonyLib;

namespace FCP.Core
{
    [HarmonyPatch(typeof(ITab_Pawn_Gear), "TryDrawOverallArmor")]
    public class ITab_Gear_TryDrawArmor_Patch
    {

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = instructions.ToList();
            for (int i = 1; i < code.Count - 1; i++)
            {
                if (code[i].opcode == OpCodes.Ldc_R4 && code[i].operand is 2f)
                {
                    code[i].operand = FCP_Utility.MaxArmorValue;
                }
            }
            return code;
        }
    }
}

