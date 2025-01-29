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
        public static bool enableShuttleCrash = true;
        public static bool enableActiveSkirmish = true;
        public static bool enableShadyTraders = true;

        public static int shuttleCrashWeightA = 1;
        private string _shuttleCrashWeightA;
        public static int shuttleCrashWeightB = 500;
        private string _shuttleCrashWeightB;
        public static int shuttleCrashWeightC = 1;
        private string _shuttleCrashWeightC;

        public static float shadyTradersAmbushChance = 0.5f;

        public static int activeSkirmishWeightA = 1;
        private string _activeSkirmishWeightA;
        public static int activeSkirmishWeightB = 500;
        private string _activeSkirmishWeightB;
        public static int activeSkirmishWeightC = 1;
        private string _activeSkirmishWeightC;

        public static int shuttleWeightsTotal => shuttleCrashWeightA + shuttleCrashWeightB + shuttleCrashWeightC;
        public static int[] cumulativeWeightsShuttleCrash => new int[] { shuttleCrashWeightA, shuttleCrashWeightA + shuttleCrashWeightB, shuttleCrashWeightA + shuttleCrashWeightB + shuttleCrashWeightC};

        public static int activeSkirmishWeightsTotal => activeSkirmishWeightA + activeSkirmishWeightB + activeSkirmishWeightC;
        public static int[] cumulativeWeightsActiveSkirmish => new int[] { activeSkirmishWeightA, activeSkirmishWeightA + activeSkirmishWeightB, activeSkirmishWeightA + activeSkirmishWeightB + activeSkirmishWeightC };

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

            listing_Standard.CheckboxLabeled("FCP_CaravanIncident_Settings_ShuttleCrash_Enable".Translate(), ref enableShuttleCrash);

            listing_Standard.Label("FCP_CaravanIncident_Settings_ShuttleCrash_VariantA".Translate());
            listing_Standard.TextFieldNumeric(ref shuttleCrashWeightA, ref _shuttleCrashWeightA, 0, 250);

            listing_Standard.Label("FCP_CaravanIncident_Settings_ShuttleCrash_VariantB".Translate());
            listing_Standard.TextFieldNumeric(ref shuttleCrashWeightB, ref _shuttleCrashWeightB, 0, 250);

            listing_Standard.Label("FCP_CaravanIncident_Settings_ShuttleCrash_VariantC".Translate());
            listing_Standard.TextFieldNumeric(ref shuttleCrashWeightC, ref _shuttleCrashWeightC, 0, 100);


            listing_Standard.Gap();
            using (new TextBlock(GameFont.Medium))
            {
                listing_Standard.Label("FCP_CaravanIncident_Settings_ShadyTraders_Label".Translate());

            }
            listing_Standard.CheckboxLabeled("FCP_CaravanIncident_Settings_ShadyTraders_Enable".Translate(), ref enableShadyTraders);
            listing_Standard.Label("FCP_CaravanIncident_Settings_ShadyTraders_AmbushChance".Translate() + ": " + (shadyTradersAmbushChance * 100f).ToString() + "%");
            shadyTradersAmbushChance = (float)Math.Round((double)listing_Standard.Slider(shadyTradersAmbushChance, 0f, 1f), 2);

            listing_Standard.Gap();
            using (new TextBlock(GameFont.Medium))
            {
                listing_Standard.Label("FCP_CaravanIncident_Settings_ActiveSkirmish_Label".Translate());

            }

            listing_Standard.CheckboxLabeled("FCP_CaravanIncident_Settings_ActiveSkirmish_Enable".Translate(), ref enableActiveSkirmish);

            listing_Standard.Label("FCP_CaravanIncident_Settings_ActiveSkirmish_VariantA".Translate());
            listing_Standard.TextFieldNumeric(ref activeSkirmishWeightA, ref _activeSkirmishWeightA, 0, 250);

            listing_Standard.Label("FCP_CaravanIncident_Settings_ActiveSkirmish_VariantB".Translate());
            listing_Standard.TextFieldNumeric(ref activeSkirmishWeightB, ref _activeSkirmishWeightB, 0, 250);

            listing_Standard.Label("FCP_CaravanIncident_Settings_ActiveSkirmish_VariantC".Translate());
            listing_Standard.TextFieldNumeric(ref activeSkirmishWeightC, ref _activeSkirmishWeightC, 0, 100);

            listing_Standard.End();
            //Widgets.EndScrollView();
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref enableShuttleCrash, "enableShuttleCrash", true);
            Scribe_Values.Look(ref shuttleCrashWeightA, "shuttleCrashWeightA", 1);
            Scribe_Values.Look(ref shuttleCrashWeightB, "shuttleCrashWeightB", 1);
            Scribe_Values.Look(ref shuttleCrashWeightC, "shuttleCrashWeightC", 1);
            Scribe_Values.Look(ref enableActiveSkirmish, "enableActiveSkirmish", true);
            Scribe_Values.Look(ref activeSkirmishWeightA, "activeSkirmishWeightA", 1);
            Scribe_Values.Look(ref activeSkirmishWeightB, "activeSkirmishWeightB", 1);
            Scribe_Values.Look(ref activeSkirmishWeightC, "activeSkirmishWeightC", 1);
            Scribe_Values.Look(ref enableShadyTraders, "enableShadyTraders", true);
            Scribe_Values.Look(ref shadyTradersAmbushChance, "shadyTradersAmbushChance", 0.5f);


        }
    }
}
