using RimWorld;
using Verse;
using System.Collections.Generic;
using RimWorld;

namespace FCP.Core
{
    public class MercenaryExtension : DefModExtension
    {
        public float initialHiringCost = 0f;
        public float upkeepCostPerMonth = 0f;
        public int paymentCycleDays = 30;
        public List<MissedPaymentConsequence> missedPaymentConsequences = new List<MissedPaymentConsequence>();
        public int hostilityDelayDaysAfterMissedPayment = 7;
        public int hiringCooldownDaysAfterHostility = 30;
        public float tributeRequestChance = 0f;
        public int tributeDeadlineDays = 15;
        public float caravanSuccessRateBase = 0.5f;
        public float mercenaryCampRadius = 20f;
        public int mercenaryArrivalFrequencyDays = 30;
        public PawnGroupMaker mercenaryGroupToArrive;
        public List<CaravanObjectiveDef> caravanObjectives;
        public List<ThingDef> campBuildableDefs;
        public List<ThingDef> paymentMethods;
        public List<ThingDef> tributeRequestItems;
        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var error in base.ConfigErrors())
            {
                yield return error;
            }

            if (upkeepCostPerMonth < 0)
            {
                yield return "upkeepCostPerMonth cannot be negative.";
            }
            if (initialHiringCost < 0)
            {
                yield return "initialHiringCost cannot be negative.";
            }
            if (paymentCycleDays <= 0)
            {
                yield return "paymentCycleDays must be positive.";
            }
            if (hostilityDelayDaysAfterMissedPayment < 0)
            {
                yield return "hostilityDelayDaysAfterMissedPayment cannot be negative.";
            }
            if (hiringCooldownDaysAfterHostility < 0)
            {
                yield return "hiringCooldownDaysAfterHostility cannot be negative.";
            }
            if (tributeRequestChance < 0 || tributeRequestChance > 1)
            {
                yield return "tributeRequestChance must be between 0 and 1.";
            }
            if (caravanSuccessRateBase < 0 || caravanSuccessRateBase > 1)
            {
                yield return "caravanSuccessRateBase must be between 0 and 1.";
            }
            if (mercenaryCampRadius < 0)
            {
                yield return "mercenaryCampRadius cannot be negative.";
            }
            if (mercenaryArrivalFrequencyDays <= 0)
            {
                yield return "mercenaryArrivalFrequencyDays must be positive.";
            }
            if (tributeDeadlineDays <= 0)
            {
                yield return "tributeDeadlineDays must be positive.";
            }
        }
    }


    public enum MissedPaymentConsequence
    {
        Hostility,
        Theft
    }
}
