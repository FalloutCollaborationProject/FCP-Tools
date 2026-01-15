using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Tent
{
    class CompatibilityPatches
    {
        public static void ExecuteCompatibilityPatches(Harmony harmony)
        {
            //var showHairAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => assembly.FullName.ToLower().StartsWith("showhair"));
            //if (showHairAssembly != null)
            //{
            //    var org = AccessTools.Method(showHairAssembly.GetType("ShowHair.Patch_PawnRenderer_RenderPawnInternal"), "Postfix");
            //    var prefix = new HarmonyMethod(typeof(ShowHairPatches), nameof(ShowHairPatches.Patch_PawnRenderer_RenderPawnInternal_Postfix_Prefix));
            //    harmony.Patch(org, prefix, null);
            //}

            //var facialHairAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => assembly.FullName.ToLower().StartsWith("facialstuff"));
            //if (facialHairAssembly != null)
            //{
            //    var org = AccessTools.Method(facialHairAssembly.GetType("FacialStuff.Harmony.HarmonyPatch_PawnRenderer"), "Prefix");
            //    if (org == null) Log.Message($"org null");
            //    var prefix = new HarmonyMethod(typeof(FacialHairStuffPatches), nameof(FacialHairStuffPatches.HarmonyPatch_PawnRenderer_Prefix_Prefix));
            //    harmony.Patch(org, prefix, null);
            //}
        }
    }

    class ShowHairPatches
    {
        public static bool Patch_PawnRenderer_RenderPawnInternal_Postfix_Prefix(Pawn ___pawn)
        {
            return !(___pawn?.CurrentBed()?.def?.HasModExtension<TentModExtension>() == true);
        }
    }

    //class FacialHairStuffPatches
    //{
    //    public static bool HarmonyPatch_PawnRenderer_Prefix_Prefix(PawnRenderer __instance)
    //    {
    //        return !(__instance?.graphics?.pawn?.CurrentBed()?.def?.HasModExtension<TentModExtension>() == true);
    //    }
    //}
}
