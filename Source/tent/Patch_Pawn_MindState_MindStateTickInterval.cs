using HarmonyLib;
using Verse;
using RimWorld;
using Verse.AI;

namespace Tent
{
    [HarmonyPatch(typeof(Pawn_MindState), nameof(Pawn_MindState.MindStateTickInterval))]
    public class Patch_Pawn_MindState_MindStateTickInterval
    {
        public static void Postfix(Pawn_MindState __instance, int delta)
        {
            if (__instance.pawn.Spawned && __instance.pawn.RaceProps.IsFlesh && __instance.pawn.needs.mood != null)
            {
                var currBed = __instance.pawn.CurrentBed();
                if (currBed == null) return;
                var modExt = currBed.def.GetModExtension<TentModExtension>();
                if (modExt == null || !modExt.negateWater) return;

                WeatherDef curWeatherLerped = __instance.pawn.Map.weatherManager.CurWeatherLerped;
                if (curWeatherLerped.weatherThought != null && curWeatherLerped.weatherThought == ThoughtDef.Named("SoakingWet") && !__instance.pawn.Position.Roofed(__instance.pawn.Map))
                {
                    __instance.pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(curWeatherLerped.weatherThought);
                }
            }
        }
    }
}   
