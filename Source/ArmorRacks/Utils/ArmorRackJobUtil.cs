using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using ArmorRacks.Things;
using RimWorld;
using Verse;

namespace ArmorRacks.Utils
{
    [StaticConstructorOnStartup]
    public static class ArmorRackJobUtil
    {
        public static bool AlienRacesLoaded;
        private static MethodInfo CanWearMethodInfo;
        private static MethodInfo CanEquipMethodInfo;
        private static Type ThingDef_AlienRaceType;
        private static Type AlienSettingsType;
        private static Type GeneralSettingsType;
        private static Type AlienPartGeneratorType;
        static ArmorRackJobUtil()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            AlienRacesLoaded = false;
            var runningMods = LoadedModManager.RunningModsListForReading;
            foreach (var runningMod in runningMods)
            {
                foreach (Assembly asm in runningMod.assemblies.loadedAssemblies)
                {
                    Type cls = asm.GetType("AlienRace.RaceRestrictionSettings");
                    if (cls != null)
                    {
                        AlienRacesLoaded = true;
                        CanWearMethodInfo = cls.GetMethod("CanWear");
                        CanEquipMethodInfo = cls.GetMethod("CanEquip");
                        ThingDef_AlienRaceType = asm.GetType("AlienRace.ThingDef_AlienRace");
                        AlienSettingsType = asm.GetType("AlienRace.ThingDef_AlienRace+AlienSettings");
                        GeneralSettingsType = asm.GetType("AlienRace.GeneralSettings");
                        AlienPartGeneratorType = asm.GetType("AlienRace.AlienPartGenerator");
                    }
                }
            }
            sw.Stop();
        }

        public static IEnumerable<BodyTypeDef> GetRaceBodyTypes(ThingDef race)
        {
            if (AlienRacesLoaded == false) 
            {
                return DefDatabase<BodyTypeDef>.AllDefs;
            }
            
            // There has to be a better way of doing this...
            object alienRace;
            try
            {
                alienRace = Convert.ChangeType(race, ThingDef_AlienRaceType);
            } catch (InvalidCastException)
            {
                return DefDatabase<BodyTypeDef>.AllDefs;
            }
            
            var alienRaceFieldInfo = ThingDef_AlienRaceType.GetField("alienRace");
            var alienRaceValue = Convert.ChangeType(alienRaceFieldInfo.GetValue(alienRace), AlienSettingsType);

            var generalSettingsFieldInfo = AlienSettingsType.GetField("generalSettings");
            var generalSettingsValue = Convert.ChangeType(generalSettingsFieldInfo.GetValue(alienRaceValue), GeneralSettingsType);

            var partGeneratorFieldInfo = GeneralSettingsType.GetField("alienPartGenerator");
            var partGeneratorValue = Convert.ChangeType(partGeneratorFieldInfo.GetValue(generalSettingsValue), AlienPartGeneratorType);

            var bodytypesInfo = AlienPartGeneratorType.GetField("bodyTypes");
            var bodytypesValue = bodytypesInfo.GetValue(partGeneratorValue);
            var bodytypes = (List<BodyTypeDef>) bodytypesValue;

            if (bodytypes.Count == 0)
            {
                return DefDatabase<BodyTypeDef>.AllDefs;
            }

            return bodytypes;
        }

        public static bool RackHasItems(ArmorRack rack)
        {
            return rack.InnerContainer.Count != 0;
        }

        public static bool PawnCanEquipWeaponSet(ArmorRack rack, Pawn pawn)
        {
            return !(pawn.WorkTagIsDisabled(WorkTags.Violent) && rack.GetStoredWeapon() != null);
        }

        public static bool RaceCanWear(ThingDef apparel, ThingDef race)
        {
            if (CanWearMethodInfo != null)
            {
                var result = CanWearMethodInfo.Invoke(null, new [] {apparel, race});
                return (bool) result;
            }
            return true;
        }
        
        public static bool RaceCanEquip(ThingDef weapon, ThingDef race)
        {
            if (CanEquipMethodInfo != null)
            {
                var result = CanEquipMethodInfo.Invoke(null, new [] {weapon, race});
                return (bool) result;
            }
            return true;
        }
        
    }
}