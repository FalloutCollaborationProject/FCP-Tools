using UnityEngine;
using Verse;

namespace ArmorRacks
{
    public class ArmorRacksModSettings : ModSettings
    {
        public bool EquipSetForced = false;
        public bool TransferSetForced = false;
        public int RareTicksPerMend = 40;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref EquipSetForced, "EquipSetForced");
            Scribe_Values.Look(ref TransferSetForced, "TransferSetForced");
            Scribe_Values.Look(ref RareTicksPerMend, "RareTicksPerMend");
        }
    }

    public class ArmorRacksMod : Verse.Mod
    {
        public ArmorRacksModSettings Settings;
        
        public ArmorRacksMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<ArmorRacksModSettings>();
        }
        
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("ArmorRacks_EquipSetForcedModSetting_Label".Translate(), ref Settings.EquipSetForced);
            listingStandard.CheckboxLabeled("ArmorRacks_TransferSetForcedModSetting_Label".Translate(), ref Settings.TransferSetForced);
            listingStandard.GapLine();
            listingStandard.Label(
                "ArmorRacks_RareTicksPerMendModSetting_Label".Translate(), 
                -1f, 
                "ArmorRacks_RareTicksPerMendModSetting_Tooltip".Translate());
            var buffer = Settings.RareTicksPerMend.ToString();
            listingStandard.IntEntry(ref Settings.RareTicksPerMend, ref buffer, 1);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }
        
        public override string SettingsCategory()
        {
            return "ArmorRacks_ModName".Translate();
        }
    }
    
}