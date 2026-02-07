using UnityEngine;

namespace FCP.Core;

public class GeneralSettings : SettingsTab
{
    public override string TabName => "FCP_Settings.General".Translate();
    public override string TabToolTip => "FCP_Settings.General.tt".Translate();

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