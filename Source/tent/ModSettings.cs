using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Tent
{
    class SleepingEffects : IExposable
    {
        public string tentDefName;
        public string hediffDefName;
        public string label;
        public bool negateWater = false;
        public bool negateSleptOutside = false;
        public bool negateSleptInCold = false;
        public bool negateSleptInHeat = false;
        public bool negateSleptInBarracks = false;
        public bool ideologyTentAssignmentAllowed = false;
        public float fuelCapacity = 0;
        public float fuelConsumptionRate = 0;
        public bool fuelEnabled;

        public void ExposeData()
        {
            Scribe_Values.Look(ref tentDefName, "tentDefName");
            Scribe_Values.Look(ref hediffDefName, "hediffDefName");
            Scribe_Values.Look(ref negateWater, "negateWater");
            Scribe_Values.Look(ref negateSleptOutside, "negateSleptOutside", false);
            Scribe_Values.Look(ref negateSleptInCold, "negateSleptInCold", false);
            Scribe_Values.Look(ref negateSleptInHeat, "negateSleptInHeat", false);
            Scribe_Values.Look(ref negateSleptInBarracks, "negateSleptInBarracks", false);
            Scribe_Values.Look(ref ideologyTentAssignmentAllowed, "ideologyTentAssignmentAllowed", false);
            Scribe_Values.Look(ref fuelCapacity, "fuelCapacity", 0);
            Scribe_Values.Look(ref fuelConsumptionRate, "fuelConsumptionRate", 0);
            Scribe_Values.Look(ref fuelEnabled, "fuelEnabled", true);
        }
    }

    class HediffSet : IExposable
    {
        public string hediffDefName;
        public string label;
        public float comfyTemperatureMin = 0;
        public float comfyTemperatureMax = 0;

        public void ExposeData()
        {
            Scribe_Values.Look(ref hediffDefName, "hediffDefName");
            Scribe_Values.Look(ref comfyTemperatureMin, "comfyTemperatureMin", 0);
            Scribe_Values.Look(ref comfyTemperatureMax, "comfyTemperatureMax", 0);
        }
    }

    class ModSettings : Verse.ModSettings
    {
        public static List<SleepingEffects> effects;
        public static List<HediffSet> hediffSets;
        public static bool xmlOverride;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref effects, "effects", LookMode.Deep);
            Scribe_Collections.Look(ref hediffSets, "hediffSets", LookMode.Deep);
            Scribe_Values.Look(ref xmlOverride, "xmlOverride", false);
        }

        private Vector2 scrollPos;

        public void DoWindowContents(Rect mainRect)
        {
            var options = new Listing_Standard();
            var viewRect = new Rect(0f, 0f, mainRect.width - 60, effects != null ? (effects.Count * 304f + hediffSets.Count * 42) : 2000f);
            Widgets.BeginScrollView(mainRect, ref this.scrollPos, viewRect);
            options.Begin(viewRect);

            Text.Font = GameFont.Medium;
            options.Label("All changes require a game restart to take effect");
            Text.Font = GameFont.Small;
            
            options.CheckboxLabeled($"Override xml values (Save your changes)", ref xmlOverride);
            options.GapLine();
            options.Gap();

            foreach (var effect in effects)
            {
                Text.Font = GameFont.Medium; 
                options.Label($"Tent: {effect.tentDefName} (Hediff: {effect.label})");
                Text.Font = GameFont.Small;
                options.CheckboxLabeled($"Negate getting wet", ref effect.negateWater);
                options.CheckboxLabeled($"Negate slept outside", ref effect.negateSleptOutside);
                options.CheckboxLabeled($"Negate slept in cold", ref effect.negateSleptInCold);
                options.CheckboxLabeled($"Negate slept in heat", ref effect.negateSleptInHeat);
                options.CheckboxLabeled($"Negate slept in barracks", ref effect.negateSleptInBarracks);
                if (ModsConfig.IdeologyActive) options.CheckboxLabeled($"Disable ideology tent assignment limitation", ref effect.ideologyTentAssignmentAllowed);
                options.CheckboxLabeled($"Fuel requirement enabled", ref effect.fuelEnabled);
                if (effect.fuelEnabled)
                {
                    var rect = options.GetRect(Text.LineHeight);
                    rect.width = options.ColumnWidth / 2;
                    Widgets.Label(rect, "Fuel capacity: ");
                    var input = Widgets.TextField(ModSettings.BRect(options.ColumnWidth - 40, rect.y, 40, Text.LineHeight), effect.fuelCapacity.ToString());
                    if (double.TryParse(input, out var inputDouble)) effect.fuelCapacity = Convert.ToInt32(inputDouble);

                    rect = options.GetRect(Text.LineHeight);
                    rect.width = options.ColumnWidth / 2;
                    Widgets.Label(rect, "Fuel consumption rate: ");
                    input = Widgets.TextField(ModSettings.BRect(options.ColumnWidth - 40, rect.y, 40, Text.LineHeight), effect.fuelConsumptionRate.ToString());
                    if (double.TryParse(input, out inputDouble)) effect.fuelConsumptionRate = Convert.ToInt32(inputDouble);
                }
                options.Gap();
            }

            options.GapLine();
            options.Gap();

            foreach (var hediff in hediffSets)
            {
                Text.Font = GameFont.Medium;
                options.Label($"Hediff: {hediff.label}");
                Text.Font = GameFont.Small;
                var rect = options.GetRect(Text.LineHeight);
                rect.width = options.ColumnWidth / 2;
                Widgets.Label(rect, "Min Compfy Temperature bonus: ");
                var input = Widgets.TextField(ModSettings.BRect(options.ColumnWidth - 40, rect.y, 40, Text.LineHeight), hediff.comfyTemperatureMin.ToString());
                if (double.TryParse(input, out var inputDouble)) hediff.comfyTemperatureMin = Convert.ToInt32(inputDouble);

                rect = options.GetRect(Text.LineHeight);
                rect.width = options.ColumnWidth / 2;
                Widgets.Label(rect, "Max Compfy Temperature bonus: ");
                input = Widgets.TextField(ModSettings.BRect(options.ColumnWidth - 40, rect.y, 40, Text.LineHeight), hediff.comfyTemperatureMax.ToString());
                if (double.TryParse(input, out inputDouble)) hediff.comfyTemperatureMax = Convert.ToInt32(inputDouble);
                options.Gap();
            }
            options.End();
            Widgets.EndScrollView();

        }

        public static Rect lastRect = default;
        public static Rect BRect(float x, float y, float width, float height)
        {
            lastRect = new Rect(x, y, width, height);
            return lastRect;
        }

        public static void InitTents()
        {
            var tents = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.HasModExtension<TentModExtension>());
            if (effects != null) effects.RemoveAll(x => string.IsNullOrEmpty(x.tentDefName));
            if (effects != null && effects?.Count == tents.Count() && ModSettings.xmlOverride) ApplyEffects(effects);
            else ReadEffects(tents);

            var hediff = DefDatabase<HediffDef>.AllDefsListForReading.Where(x => x.defName.StartsWith("TENT_Comfy"));
            if (hediffSets != null) hediffSets.RemoveAll(x => string.IsNullOrEmpty(x.hediffDefName));
            if (hediffSets != null && hediffSets?.Count == hediff.Count() && ModSettings.xmlOverride) ApplyHediffs(hediffSets);
            else ReadHediffs(hediff);
        }

        private static void ReadHediffs(IEnumerable<HediffDef> hediffs)
        {
            hediffSets = new List<HediffSet>();
            foreach (var hediff in hediffs)
            {
                if (hediff?.defName == null) continue;

                hediffSets.Add(new HediffSet()
                {
                    hediffDefName = hediff.defName,
                    label = hediff.label,
                    comfyTemperatureMin = hediff?.stages?.FirstOrDefault()?.statOffsets?.FirstOrDefault(x => x?.stat == StatDefOf.ComfyTemperatureMin)?.value ?? 0,
                    comfyTemperatureMax = hediff?.stages?.FirstOrDefault()?.statOffsets?.FirstOrDefault(x => x?.stat == StatDefOf.ComfyTemperatureMax)?.value ?? 0,
                });
            }
        }

        private static void ApplyHediffs(List<HediffSet> hediffSets)
        {
            foreach (var hediffSet in hediffSets)
            {
                var hediff = DefDatabase<HediffDef>.GetNamed(hediffSet.hediffDefName, false);

                if (hediff != null)
                {
                    hediffSet.label = hediff.label;
                    var statTempMin = hediff?.stages?.FirstOrDefault()?.statOffsets?.FirstOrDefault(x => x?.stat == StatDefOf.ComfyTemperatureMin);
                    if (statTempMin != null) statTempMin.value = hediffSet.comfyTemperatureMin;
                    var statTempMax = hediff?.stages?.FirstOrDefault()?.statOffsets?.FirstOrDefault(x => x?.stat == StatDefOf.ComfyTemperatureMax);
                    if (statTempMax != null) statTempMax.value = hediffSet.comfyTemperatureMax;
                }
            }
        }

        private static void ReadEffects(IEnumerable<ThingDef> tents)
        {
            effects = new List<SleepingEffects>();
            foreach (var tent in tents)
            {
                if (tent?.defName == null) continue;
                var modExt = tent.GetModExtension<TentModExtension>();
                var refuelComp = tent.GetCompProperties<CompProperties_Refuelable>();
                var hediff = DefDatabase<HediffDef>.GetNamed(modExt?.customHediff?.defName ?? "", false);
                effects.Add(new SleepingEffects()
                {
                    tentDefName = tent.defName,
                    label = hediff?.label ?? "",
                    hediffDefName = modExt?.customHediff?.defName ?? "",
                    negateWater = modExt.negateWater,
                    negateSleptInBarracks = modExt.negateSleptInBarracks,
                    negateSleptInCold = modExt.negateSleptInCold,
                    negateSleptInHeat = modExt.negateSleptInHeat,
                    negateSleptOutside = modExt.negateSleptOutside,
                    ideologyTentAssignmentAllowed = modExt.ideologyTentAssignmentAllowed,
                    fuelEnabled = false,
                    fuelCapacity = refuelComp?.fuelCapacity ?? 0,
                    fuelConsumptionRate = refuelComp?.fuelConsumptionRate ?? 0
                });
            }
        }

        private static void ApplyEffects(List<SleepingEffects> effects)
        {
            foreach (var effect in effects)
            {
                var tent = DefDatabase<ThingDef>.GetNamed(effect.tentDefName, false);
                if (tent == null)
                {
                    Log.Warning($"Tent {effect.tentDefName} was null");
                    continue;
                }

                var modExt = tent.GetModExtension<TentModExtension>();
                if (modExt != null)
                {
                    modExt.negateWater = effect.negateWater;
                    modExt.negateSleptInBarracks = effect.negateSleptInBarracks;
                    modExt.negateSleptInCold = effect.negateSleptInCold;
                    modExt.negateSleptInHeat = effect.negateSleptInHeat;
                    modExt.negateSleptOutside = effect.negateSleptOutside;
                    modExt.ideologyTentAssignmentAllowed = effect.ideologyTentAssignmentAllowed;
                }
                
                var refuelCompProps = tent.GetCompProperties<CompProperties_Refuelable>();
                if (!effect.fuelEnabled && refuelCompProps != null) tent.comps.Remove(refuelCompProps);
                else if (refuelCompProps != null)
                {
                    refuelCompProps.fuelCapacity = effect.fuelCapacity;
                    refuelCompProps.fuelConsumptionRate = effect.fuelConsumptionRate;
                }

                var hediff = DefDatabase<HediffDef>.GetNamed(modExt.customHediff.defName, false);
                if (hediff != null)
                {
                    effect.label = hediff.label;
                }
            }
        }
    }

    [StaticConstructorOnStartup]
    class InitTentStartup { static InitTentStartup() => ModSettings.InitTents(); }

}
