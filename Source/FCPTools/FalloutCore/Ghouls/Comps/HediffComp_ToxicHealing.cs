using UnityEngine;

namespace FCP.Core.Ghouls;

public class HediffComp_ToxicHealing : HediffComp
{
    private const int CheckInterval = 60;

    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);

        if (!Pawn.IsHashIntervalTick(CheckInterval))
            return;

        var toxicBuildup = Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ToxicBuildup);

        if (toxicBuildup == null || toxicBuildup.Severity <= 0.001f)
            return;

        if (toxicBuildup.Severity > 0.8f)
        {
            toxicBuildup.Severity = 0.8f;
        }

        float toxicToConsume = 0.05f;
        float healPower = 0.20f;

        if (toxicToConsume > toxicBuildup.Severity)
        {
            toxicToConsume = toxicBuildup.Severity;
            healPower = toxicToConsume * 4f;
        }

        List<Hediff_Injury> injuries = new List<Hediff_Injury>();
        Pawn.health.hediffSet.GetHediffs(ref injuries, (Hediff_Injury h) => h.TendableNow());

        if (injuries.Any() && healPower > 0)
        {
            foreach (var injury in injuries.InRandomOrder())
            {
                if (healPower <= 0) break;

                float healAmount = Mathf.Min(healPower, injury.Severity);
                injury.Heal(healAmount);
                healPower -= healAmount;
            }
        }

        toxicBuildup.Severity -= toxicToConsume;

        if (toxicBuildup.Severity > 0.8f)
        {
            toxicBuildup.Severity = 0.8f;
        }
    }
}