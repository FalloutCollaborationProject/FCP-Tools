using HarmonyLib;
using UnityEngine;

namespace FCP.Core.Tents;

[HarmonyPatchCategory(FCPCoreMod.TentsPatchesCategory)]
[HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.RenderPawnAt), typeof(Vector3), typeof(Rot4?), typeof(bool))]
public class Patch_PawnRenderer_RenderPawnAt
{
    public static bool Prefix(Pawn ___pawn)
    {
        if (___pawn?.Map == null || ___pawn?.RaceProps?.Humanlike != true) return true;
        return !(___pawn?.CurrentBed()?.def?.HasModExtension<TentModExtension>() == true);
    }
}
