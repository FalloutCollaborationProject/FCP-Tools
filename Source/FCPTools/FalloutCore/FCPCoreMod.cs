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
    public const string TentsPatchesCategory = "FCP.Core.Tents";
    
    private SettingsTab currentTab = null;

    public FCPCoreMod(ModContentPack content) : base(content)
    {
        Instance = this;
        Settings = GetSettings<FCPSettings>();
        
        // PatchesUwU ~ Steve
        Harmony.PatchAllUncategorized();
        if (ModsConfig.IsActive("Rick.FCP.Tents"))
            Harmony.PatchCategory(TentsPatchesCategory);
        LongEventHandler.ExecuteWhenFinished(() =>
        {
            Harmony.PatchCategory(LatePatchesCategory);
        });
        FCPLog.Warning("Beta version: bugs likely, if not guaranteed! " +
                       "Report bugs on steam workshop page or on discord: 3HEXN3Qbn4");
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
        foreach (var tab in Settings.Tabs)
            tab.OnWriteSettings();
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
        if (currentTab is { Enabled: true })
            currentTab.DoTabWindowContents(mainRect.ContractedBy(15f));
        else if (currentTab != null)
        {
            Rect labelRect = mainRect.ContractedBy(15f);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(labelRect, $"Requires the corresponding FCP module to be installed and active.");
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
        }
    }
}
