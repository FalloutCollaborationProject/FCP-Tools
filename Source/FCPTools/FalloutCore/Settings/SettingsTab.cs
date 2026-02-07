using UnityEngine;

namespace FCP.Core.Settings;

public abstract class SettingsTab : IExposable
{
    public abstract string TabName { get;  }

    public abstract void DoTabWindowContents(Rect tabRect);
    
    public abstract void ExposeData();
}

public class DebugSettings : SettingsTab
{
    public override string TabName => "FCP_Settings.Debug".Translate();

    public bool verboseLogging;

    public override void DoTabWindowContents(Rect tabRect)
    {
        var list = new Listing_Standard();
        list.Begin(tabRect);
        
        list.Label("Logging");
        list.CheckboxLabeled("Verbose Logging", ref verboseLogging);
        
        list.End();
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref verboseLogging, nameof(verboseLogging), defaultValue: false);
    }
}

public class GeneralSettings : SettingsTab
{
    public override string TabName => "FCP_Settings.General".Translate();
    
    public override void DoTabWindowContents(Rect tabRect)
    {
        var list = new Listing_Standard();
        list.Begin(tabRect);
        
        list.Label("General");
        
        list.End();
    }

    public override void ExposeData()
    {
    }
}