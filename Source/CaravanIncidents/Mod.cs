using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FCP_CaravanIncidents
{
    public class IncidentsMod : Mod
    {
        CaravanIncidents_Settings settings;
        public IncidentsMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<CaravanIncidents_Settings>();
        }


        public override string SettingsCategory()
        {
            return "FCP - Caravan Incidents";
        }


        public override void DoSettingsWindowContents(Rect inRect)
        {
            settings.DoWindowContents(inRect);
        }
    }
    public class CaravanIncidents_Settings : ModSettings
    {
        public static int shuttleCrashWeightA = 1;
        private string _shuttleCrashWeightA;
        public static int shuttleCrashWeightB = 500;
        private string _shuttleCrashWeightB;
        public static int shuttleCrashWeightC = 1;
        private string _shuttleCrashWeightC;

        public static int shuttleWeightsTotal => shuttleCrashWeightA + shuttleCrashWeightB + shuttleCrashWeightC;
        public static int[] cumulativeWeightsShuttleCrash => new int[] { shuttleCrashWeightA, shuttleCrashWeightA + shuttleCrashWeightB, shuttleCrashWeightA + shuttleCrashWeightB + shuttleCrashWeightC};

        public void DoWindowContents(Rect inRect)
        {
/*            Rect rect2 = new Rect(inRect);
            rect2.height = 750f;
            Rect rect3 = rect2;
            Widgets.AdjustRectsForScrollView(inRect, ref rect2, ref rect3);
            Widgets.BeginScrollView(inRect, ref _scrollPosition, rect3, showScrollbars: false);
*/
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);

            listing_Standard.Gap();
            using (new TextBlock(GameFont.Medium))
            {
                listing_Standard.Label("FCP_CaravanIncident_Settings_ShuttleCrash_Label".Translate());

            }

            listing_Standard.Label("FCP_CaravanIncident_Settings_ShuttleCrash_VariantA".Translate());
            listing_Standard.TextFieldNumeric(ref shuttleCrashWeightA, ref _shuttleCrashWeightA, 0, 250);

            listing_Standard.Label("FCP_CaravanIncident_Settings_ShuttleCrash_VariantB".Translate());
            listing_Standard.TextFieldNumeric(ref shuttleCrashWeightB, ref _shuttleCrashWeightB, 0, 250);

            listing_Standard.Label("FCP_CaravanIncident_Settings_ShuttleCrash_VariantC".Translate());
            listing_Standard.TextFieldNumeric(ref shuttleCrashWeightC, ref _shuttleCrashWeightC, 0, 100);

            listing_Standard.End();
            //Widgets.EndScrollView();
        }
        public override void ExposeData()
        {
            base.ExposeData();
        }
    }
}
