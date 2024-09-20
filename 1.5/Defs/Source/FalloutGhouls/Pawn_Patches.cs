using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;


namespace FalloutCore
{

    [HarmonyPatch(typeof(PawnGenerator), "GenerateRandomAge")]
    public static class Patch_GenerateRandomAge
    {
        private static void Postfix(Pawn pawn, PawnGenerationRequest request)
        {
            if (pawn.IsGhoul())
            {
                pawn.ageTracker.AgeBiologicalTicks = (long)(Rand.RangeInclusive(250, 405) * 3600000f) + Rand.Range(0, 3600000);
                pawn.ageTracker.AgeChronologicalTicks = pawn.ageTracker.AgeBiologicalTicks;
            }
        }
    }
}