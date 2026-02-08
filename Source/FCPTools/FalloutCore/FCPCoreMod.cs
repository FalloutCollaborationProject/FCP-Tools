using FCP.Core;
using HarmonyLib;
using UnityEngine;

namespace FCP.Core;

[UsedImplicitly]
public class FCPCoreMod : Mod
{
    public static FCPCoreMod Instance { get; private set; }
    public static Harmony Harmony { get; } = new Harmony("FCP.Core.Patches");
    public static FCPSettings Settings { get; private set; }

    // Settings Helper
    public static T SettingsTab<T>() where T : SettingsTab => Settings.GetTab<T>();

    // Patch Categories
    public const string LatePatchesCategory = "FCP.Core.LatePatches";
    
    private SettingsTab currentTab = null;

    public FCPCoreMod(ModContentPack content) : base(content)
    {
        Instance = this;
        Settings = GetSettings<FCPSettings>();
        
        // PatchesUwU ~ Steve
        Harmony.PatchAllUncategorized();
        LongEventHandler.ExecuteWhenFinished(() =>
        {
            Harmony.PatchCategory(LatePatchesCategory);
        });
        FCPLog.Warning("Beta version: bugs likely, if not guaranteed! " +
                       "Report bugs on steam workshop page or on discord: 3HEXN3Qbn4");
    }

    public override string SettingsCategory() => "FCP_Settings_Category".Translate();
    
    public override void DoSettingsWindowContents(Rect inRect)
    {
        var tabRect = new Rect(inRect)
        {
            y = inRect.y + 40f
        };
        var mainRect = new Rect(inRect)
        {
            height = inRect.height - 40f,
            y = inRect.y + 40f
        };

        Widgets.DrawMenuSection(mainRect);

        // Draw Tabs
        currentTab ??= SettingsTab<GeneralSettings>();

        var tabs = Settings.Tabs
            .Select(tab => new TabRecord(tab.TabName, () =>
            {
                currentTab = tab;
                WriteSettings();
            }, currentTab == tab))
            .ToList();

        TabDrawer.DrawTabs(tabRect, tabs);
        currentTab?.DoTabWindowContents(mainRect.ContractedBy(15f));
    }
}
