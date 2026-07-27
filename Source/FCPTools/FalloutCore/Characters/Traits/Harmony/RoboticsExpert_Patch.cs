using HarmonyLib;
using Verse;

namespace FCP.Core.Traits;

[HarmonyPatch(typeof(Thing), nameof(Thing.TakeDamage))]
public static class RoboticsExpert_TakeDamage_Patch
{
    private const float MechanoidDamageFactor = 1.25f;

    public static void Prefix(Thing __instance, ref DamageInfo dinfo)
    {
        if (__instance is not Pawn target || !target.RaceProps.IsMechanoid)
            return;
        if (dinfo.Instigator is not Pawn attacker || attacker.story?.traits == null)
            return;
        if (!attacker.story.traits.HasTrait(TraitsDefOf.FCP_Trait_Robotics_Expert))
            return;

        dinfo.SetAmount(dinfo.Amount * MechanoidDamageFactor);
    }
}
