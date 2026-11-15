using System.Text;
using HarmonyLib;

namespace FCP.Core.PowerArmor;

[HarmonyPatch(typeof(MassUtility), nameof(MassUtility.Capacity))]
public static class MassUtility_Capacity_Patch
{
    public static void Postfix(Pawn p, StringBuilder explanation, ref float __result)
    {
        if (__result <= 0f || p.apparel == null)
            return;

        List<Apparel> worn = p.apparel.WornApparel;
        for (int i = 0; i < worn.Count; i++)
        {
            float offset = worn[i].def.equippedStatOffsets.GetStatOffsetFromList(StatDefOf.CarryingCapacity);
            if (offset == 0f)
                continue;

            __result += offset;
            explanation?.AppendLine().Append("  - " + worn[i].LabelShortCap + ": " + offset.ToStringMassOffset());
        }
    }
}
