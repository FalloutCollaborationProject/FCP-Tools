using UnityEngine;
using Verse;

namespace FCP.Core.TemperatureApparelPreference
{
    //You might wanna remove this once you're done testing :)
    public class Settings : ModSettings
    {
        public bool verboseLogging;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref verboseLogging, "verboseLogging", false);
        }

        public void DoWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);

            listing.CheckboxLabeled(
                "Verbose logging",
                ref verboseLogging,
                "If enabled, logs detailed decisions made by Temperature Apparel Preference. Disable unless diagnosing."
            );

            listing.End();
        }
    }
}
