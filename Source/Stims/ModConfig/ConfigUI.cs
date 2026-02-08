using UnityEngine;
using Verse;

namespace StimPacks.ModConfig
{
    public class ConfigUI : Mod
    {
            public static Configs Config;
            public ConfigUI(ModContentPack content) : base(content)
            {
                Config = GetSettings<Configs>();
            }

            public override string SettingsCategory()
            {
                return "Stims";
            }
            
            public override void DoSettingsWindowContents(Rect inRect)
            {
                Listing_Standard listing = new Listing_Standard();
                listing.Begin(inRect);
                
                listing.CheckboxLabeled("AutoStim".Translate(), ref Config.AutoStim, "AutoStimDesc".Translate());
                
                listing.CheckboxLabeled("TeetotalerAutoStim".Translate(), ref Config.TeetotalerAutoStim, "TeetotalerAutoStimDesc".Translate());
                
                listing.End();
                
                base.DoSettingsWindowContents(inRect);
            }
    }
}