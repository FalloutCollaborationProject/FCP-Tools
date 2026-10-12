using HarmonyLib;

namespace FCP.Mutations;

[HarmonyPatch(typeof(Thing), nameof(Thing.PostApplyDamage))]
public static class Thing_PostApplyDamage_Retaliation_Patch
{
    private const float MeleeRange = 1.9f;
    private const float ShockDamageAmount = 8f;
    private const float IsotopeChance = 0.10f;
    private const float IsotopeSeverity = 0.08f;

    private static bool retaliating;

    public static void Postfix(Thing __instance, DamageInfo dinfo, float totalDamageDealt)
    {
        if (retaliating || dinfo.Def.isRanged)
            return;
        if (totalDamageDealt <= 0f || __instance is not Pawn victim || victim.Dead || victim.genes == null)
            return;
        if (dinfo.Instigator is not Pawn attacker || attacker == victim || attacker.Dead)
            return;
        if (victim.Map == null || attacker.Position.DistanceTo(victim.Position) > MeleeRange)
            return;

        if (victim.genes.HasActiveGene(MutationsDefOf.FCP_Gene_ElectricallyCharged))
        {
            DamageInfo shock = new DamageInfo(DamageDefOf.Burn, ShockDamageAmount, 0f, -1f, victim);
            retaliating = true;
            try
            {
                attacker.TakeDamage(shock);
            }
            finally
            {
                retaliating = false;
            }
        }

        if (!attacker.Dead && victim.genes.HasActiveGene(MutationsDefOf.FCP_Gene_UnstableIsotope) && Rand.Chance(IsotopeChance))
            HealthUtility.AdjustSeverity(attacker, HediffDefOf.ToxicBuildup, IsotopeSeverity);
    }
}
