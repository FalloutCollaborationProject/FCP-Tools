using System.Globalization;
using UnityEngine;

namespace FCP.Core.Stims;

public class HediffComp_Stimpack : HediffComp
{
    private float toHeal = 0;

    private HediffCompProperties_Stimpack Props => (HediffCompProperties_Stimpack)props;

    public override string CompDescriptionExtra => $"\nHealing left : {toHeal} HP";

    private int nextTickHeal = 0;

    public override void CompPostMerged(Hediff other)
    {
        var stim = other.TryGetComp<HediffComp_Stimpack>();
        if (stim != null)
        {
            var toAdd = stim.GetHealingAmount();
            toHeal += toAdd;
        }

        base.CompPostMerged(other);
    }

    private float GetHealingAmount()
    {
        var toHeal = 0f;
        if (Props.ScaleHealedAmountWithTotalHitPoints)
        {
            toHeal = StimData.TotalHpForRace[Pawn.RaceProps.body] / StimData.AverageHitPoint;
        }
        else
        {
            toHeal = Props.HealedAmount;
        }
        toHeal *= Props.HealedAmount;
        return toHeal;
    }

    public override void CompPostPostAdd(DamageInfo? dinfo)
    {
        toHeal = GetHealingAmount();
        RefreshTickWait();
    }

    private void RefreshTickWait()
    {
        nextTickHeal = Current.Game.tickManager.TicksGame + Rand.Range(Props.TickMinimumBetweenHealing, Props.TickMaximumBetweenHealing);
    }

    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);
        if (toHeal <= 0)
        {
            if(Props.RemoveHediffWhenDone)
            {
                Pawn.health.RemoveHediff(parent);
            }
            else
            {
                return;
            }
        }

        if (nextTickHeal > Current.Game.tickManager.TicksGame) 
            return;
        
        RefreshTickWait();
        Hediff injury = null;
        foreach (Hediff hediff in Pawn.health.hediffSet.hediffs)
        {
            if (hediff.IsPermanent())
            {
                if (!Props.HealPermanentInjuries)
                {
                    continue;
                }

                if (hediff is Hediff_Injury)
                {
                    injury = hediff;
                }
            }

            if (hediff is Hediff_MissingPart)
            {
                continue;
            }

            if (hediff.SummaryHealthPercentImpact > 0)
            {
                injury = hediff;
                break;
            }
        }

        if (injury != null)
        {
            float healed = 0;
            if (toHeal < 1)
            {
                healed = toHeal;
                injury.Heal(toHeal);
            }
            else
            {
                healed = Math.Min(1, 3);
                injury.Heal(healed);
            }

            if (injury.IsPermanent())
                healed *= 1.5f;
            toHeal -= healed;
        }
        else if(Props.HealMissingBodyParts)
        {
            var missingBodyParts = Pawn.health.hediffSet.GetMissingPartsCommonAncestors();
            if (missingBodyParts.Count <= 0) 
                return;
            
            Hediff_MissingPart missing = missingBodyParts.First();
            Pawn.health.RemoveHediff(missing);
            Hediff toAdd = HediffMaker.MakeHediff(HediffDefOf.Cut, Pawn, missing.Part);
            float partHealth = toAdd.Part.def.GetMaxHealth(Pawn) - 1f;
            FCPLog.Warning(partHealth.ToString(CultureInfo.InvariantCulture));
            toAdd.Severity = partHealth;
            Pawn.health.AddHediff(toAdd);
            toHeal -= 3;
        }
    }
}