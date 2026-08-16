using HarmonyLib;

namespace FCP.Mutations;

[HarmonyPatch(typeof(Thing), nameof(Thing.PostApplyDamage))]
public static class Thing_PostApplyDamage_Retaliation_Patch
{
    private const float MeleeRange = 1.9f;
    private const float ShockDamageAmount = 8f;
    private const float IsotopeChance = 0.10f;
    private const float IsotopeSeverity = 0.08f;

    public static void Postfix(Thing __instance, DamageInfo dinfo, float totalDamageDealt)
    {
        if (totalDamageDealt <= 0f || __instance is not Pawn victim || victim.Dead || victim.genes == null)
            return;
        if (dinfo.Instigator is not Pawn attacker || attacker == victim || attacker.Dead)
            return;
        if (victim.Map == null || attacker.Position.DistanceTo(victim.Position) > MeleeRange)
            return;

        if (victim.genes.HasActiveGene(MutationsDefOf.FCP_Gene_ElectricallyCharged))
        {
            DamageInfo shock = new DamageInfo(DamageDefOf.Burn, ShockDamageAmount, 0f, -1f, victim);
            attacker.TakeDamage(shock);
        }

        if (victim.genes.HasActiveGene(MutationsDefOf.FCP_Gene_UnstableIsotope) && Rand.Chance(IsotopeChance))
        {
            Hediff hediff = attacker.health.AddHediff(HediffDefOf.ToxicBuildup);
            hediff.Severity += IsotopeSeverity;
        }
    }
}
