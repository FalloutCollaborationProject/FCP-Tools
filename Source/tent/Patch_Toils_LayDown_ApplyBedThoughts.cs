using System;
using System.Linq;
using HarmonyLib;
using Verse;
using RimWorld;

namespace Tent
{
    [HarmonyPatch(typeof(Toils_LayDown), "ApplyBedThoughts", new Type[] { typeof(Pawn), typeof(Building_Bed) })]
    public class Patch_Toils_LayDown_ApplyBedThoughts
    {
        public static void Postfix(Pawn actor)
        {
            Building_Bed building_Bed = actor.CurrentBed();
            if (building_Bed == null) return;
            var modExt = building_Bed.def.GetModExtension<TentModExtension>();
            if (modExt == null) return;

            var effect = ModSettings.effects.FirstOrDefault(x => x?.tentDefName == building_Bed.def.defName);
            if (effect == null) return;
            if (effect.negateSleptOutside) actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptOutside);
            if (effect.negateSleptInCold) actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInCold);
            if (effect.negateSleptInHeat) actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInHeat);
            if (effect.negateSleptInBarracks) actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBarracks);
        }
    }
}   
