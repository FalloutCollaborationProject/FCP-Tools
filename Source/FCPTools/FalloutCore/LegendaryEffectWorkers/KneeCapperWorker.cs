namespace FCP.Core.LegendaryEffectWorkers;

public class KneeCapperWorker : LegendaryEffectWorker
{
    public override void ApplyEffect(ref DamageInfo damageInfo, Pawn pawn)
    {
        // 20% of the time
        if (pawn == null || Rand.Range(0, 4) != 0)
        {
            return;
        }

        IEnumerable<BodyPartRecord> legs = pawn.def.race.body.AllParts.Where(part => part.Label.ToLower().Contains("leg"));

        foreach (BodyPartRecord leg in legs)
        {
            pawn.health.AddHediff(effect.hediffToApply, leg, damageInfo);
        }
    }
}
