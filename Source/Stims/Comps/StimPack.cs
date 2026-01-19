using System;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace StimPacks.Comps
{
    public class StimPack : HediffComp
    {
        private float ToHeal = 0;

        public StimPackProp Props => (StimPackProp)props;

        public override string CompDescriptionExtra => $"\nHealing left : {ToHeal} HP";

        public int NextTickHeal = 0;

        public override void CompPostMerged(Hediff other)
        {
            var stim = other.TryGetComp<StimPack>();
            if (stim != null)
            {
                var toAdd = stim.GetHealingAmount();
                ToHeal += toAdd;
            }
            
            base.CompPostMerged(other);
        }

        private float GetHealingAmount()
        {
            var toHeal = 0f;
            if (Props.ScaleHealedAmountWithTotalHitPoints)
            {
                toHeal = StimPacks.TotalHpForRace[Pawn.RaceProps.body] / StimPacks.AverageHitPoint;
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
            ToHeal = GetHealingAmount();
            RefreshTickWait();
        }

        private void RefreshTickWait()
        {
            NextTickHeal = Current.Game.tickManager.TicksGame + Rand.Range(Props.TickMinimumBetweenHealing, Props.TickMaximumBetweenHealing);
        }
        
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (ToHeal <= 0)
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
            if (NextTickHeal <= Current.Game.tickManager.TicksGame)
            {
                RefreshTickWait();
                Hediff injury = null;
                foreach (var hediff in Pawn.health.hediffSet.hediffs)
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
                    if (ToHeal < 1)
                    {
                        healed = ToHeal;
                        injury.Heal(ToHeal);
                    }
                    else
                    {
                        healed = Math.Min(1, 3);
                        injury.Heal(healed);
                    }

                    if (injury.IsPermanent())
                        healed *= 1.5f;
                    ToHeal -= healed;
                }
            }
        }
    }
    

    
    public class StimPackProp : HediffCompProperties
    {
        public bool HealPermanentInjuries = false;
        public bool HealMissingBodyParts = false;
        public bool RemoveHediffWhenDone = true;
        public bool ScaleHealedAmountWithTotalHitPoints = true;
        public float HealedAmount = 30f;
        public int TickMinimumBetweenHealing = 50;
        public int TickMaximumBetweenHealing = 100;
        public StimPackProp()
        {
            compClass = typeof(StimPack);
        }
    }
}