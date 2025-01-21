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
        public static int shuttleCrashWeightA = 10000;
        public static int shuttleCrashWeightB = 2;
        public static int shuttleCrashWeightC = 1;

        public static int shuttleWeightsTotal => shuttleCrashWeightA + shuttleCrashWeightB + shuttleCrashWeightC;
        public static int[] cumulativeWeightsShuttleCrash => new int[] { shuttleCrashWeightA, shuttleCrashWeightA + shuttleCrashWeightB, shuttleCrashWeightA + shuttleCrashWeightB + shuttleCrashWeightC};

        public void DoWindowContents(Rect inRect)
        {

        }
        public override void ExposeData()
        {
            base.ExposeData();
        }
    }
}
