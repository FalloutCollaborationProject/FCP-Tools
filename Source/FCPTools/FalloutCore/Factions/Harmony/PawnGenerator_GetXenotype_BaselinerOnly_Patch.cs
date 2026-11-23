using HarmonyLib;
using Verse;

namespace FCP.Factions.Harmony;

[HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GetXenotypeForGeneratedPawn))]
public static class PawnGenerator_GetXenotype_BaselinerOnly_Patch
{
    public static bool Prefix(PawnGenerationRequest request, ref XenotypeDef __result)
    {
        if (request.Faction?.def.HasModExtension<FactionExtension_BaselinerOnly>() != true)
            return true;

        __result = XenotypeDefOf.Baseliner;
        return false;
    }
}
