using HarmonyLib;
using Verse.AI;

namespace FCP.Core.Birds;

[HarmonyPatchCategory("Birds")]
[HarmonyPatch(typeof(Pawn_JobTracker), "CleanupCurrentJob")]
public class Pawn_JobTracker_CleanupCurrentJob
{
    public static void Prefix(Pawn ___pawn)
    {
        if (!___pawn.IsFlyingPawn(out var comp))
        {
            return;
        }

        if (!comp.isFlyingCurrently)
        {
            return;
        }

        comp.isFlyingCurrently = false;
        comp.ChangeGraphic();
    }
}