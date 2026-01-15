using System.Linq;
using HarmonyLib;
using Verse;
using RimWorld;

namespace Tent
{
    [HarmonyPatch(typeof(CompAssignableToPawn_Bed), "IdeoligionForbids")]
    public class Patch_CompAssignableToPawn_Bed_IdeoligionForbids
    {
        public static void Postfix(CompAssignableToPawn_Bed __instance, ref bool __result, Pawn pawn)
        {
            if (__instance?.parent == null) return;
            var modExt = __instance.parent.def.GetModExtension<TentModExtension>();
            if (modExt == null) return;

            var effect = ModSettings.effects.FirstOrDefault(x => x?.tentDefName == __instance.parent.def.defName);
            if (effect == null) return;
            if (effect.ideologyTentAssignmentAllowed) __result = false;
        }
    }
}   
