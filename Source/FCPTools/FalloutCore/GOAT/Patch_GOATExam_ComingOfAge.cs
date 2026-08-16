using HarmonyLib;
using RimWorld;
using Verse;

namespace FCP.Core.GOAT;

[HarmonyPatch(typeof(Pawn_AgeTracker), "BirthdayBiological")]
public static class Patch_GOATExam_ComingOfAge
{
    public static void Postfix(Pawn_AgeTracker __instance, int birthdayAge, Pawn ___pawn)
    {
        if ((float)birthdayAge != __instance.AdultMinAge)
            return;

        if (___pawn == null || !___pawn.Spawned || !___pawn.IsColonist || ___pawn.Faction != Faction.OfPlayer)
            return;

        if (!___pawn.RaceProps.Humanlike || ___pawn.skills == null)
            return;

        Find.WindowStack.Add(new Dialog_GOATExam(___pawn, new Queue<Pawn>()));
    }
}
