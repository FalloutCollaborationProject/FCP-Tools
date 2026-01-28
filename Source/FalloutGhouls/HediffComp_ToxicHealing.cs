using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace FCP_Ghoul
{
    public class HediffComp_ToxicHealing : HediffComp
    {
        private const int CheckInterval = 60; // Check every 1 second

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (!Pawn.IsHashIntervalTick(CheckInterval))
                return;

            var toxicBuildup = Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ToxicBuildup);
            
            if (toxicBuildup == null || toxicBuildup.Severity <= 0.001f)
                return;
            
            // First, immediately cap toxic at 80% to prevent lethal levels
            if (toxicBuildup.Severity > 0.8f)
            {
                toxicBuildup.Severity = 0.8f;
            }
            
            // Fixed consumption rate - consume 0.05 (5%) per check, heal 0.20 per check
            // This means consistent healing speed regardless of toxic level
            float toxicToConsume = 0.05f;
            float healPower = 0.20f;
            
            // Don't consume more toxic than exists
            if (toxicToConsume > toxicBuildup.Severity)
            {
                toxicToConsume = toxicBuildup.Severity;
                healPower = toxicToConsume * 4f; // Scale healing proportionally
            }
            
            // Get injuries list and apply healing if any exist
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
            
            // ALWAYS consume toxic regardless of whether injuries were healed
            toxicBuildup.Severity -= toxicToConsume;
            
            // Keep it under 80% maximum
            if (toxicBuildup.Severity > 0.8f)
            {
                toxicBuildup.Severity = 0.8f;
            }
        }
    }
}
