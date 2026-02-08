using System.Collections.Generic;
using Verse;

namespace FCP.Core
{
    public class CompProperties_TemperatureApparelPreference : CompProperties
    {
        public bool enabled = true;

        // Any combination is valid. Defaults mean "not configured".
        // - forceBelowTempC: force include when tempC < value
        // - avoidAboveTempC: avoid when tempC > value
        // - forceAboveTempC: force include when tempC > value
        // - avoidBelowTempC: avoid when tempC < value
        public float forceBelowTempC = float.NegativeInfinity;
        public float avoidAboveTempC = float.PositiveInfinity;

        public float forceAboveTempC = float.PositiveInfinity;
        public float avoidBelowTempC = float.NegativeInfinity;

        public CompProperties_TemperatureApparelPreference()
        {
            compClass = typeof(CompTemperatureApparelPreference);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var e in base.ConfigErrors(parentDef)) yield return e;

            if (parentDef == null || !parentDef.IsApparel)
            {
                yield return (parentDef != null ? parentDef.defName : "(null)") +
                             " has CompProperties_TemperatureApparelPreference but is not apparel.";
                yield break;
            }

            bool anyConfigured =
                !float.IsNegativeInfinity(forceBelowTempC) ||
                !float.IsPositiveInfinity(avoidAboveTempC) ||
                !float.IsPositiveInfinity(forceAboveTempC) ||
                !float.IsNegativeInfinity(avoidBelowTempC);

            if (!anyConfigured)
            {
                yield return parentDef.defName +
                             " TemperatureApparelPreference has no thresholds configured (set at least one of forceBelowTempC, avoidAboveTempC, forceAboveTempC, avoidBelowTempC).";
            }
        }
    }
}
