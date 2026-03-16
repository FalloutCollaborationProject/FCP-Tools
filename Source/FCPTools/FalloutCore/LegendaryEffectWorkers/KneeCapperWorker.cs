namespace FCP.Core.LegendaryEffectWorkers;

public class KneeCapperWorker : LegendaryEffectWorker
{
    public override void Notify_ApplyToPawn(ref DamageInfo damageInfo, Pawn pawn)
    {
        // Only do it 20% of the time
        bool randomChance = Rand.Range(0, 5) == 0;
        if (pawn == null || !randomChance)
            return;

        IEnumerable<BodyPartRecord> legs = pawn.def.race.body.AllParts.Where(part => part.Label.ToLower().Contains("leg"));

        foreach (BodyPartRecord leg in legs)
        {
            pawn.health.AddHediff(FCPDefOf.FCP_VATSCrippledHediff, leg, damageInfo);
        }
    }
}
