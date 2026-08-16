using FCP.Core;
using HarmonyLib;
using UnityEngine;

namespace FCP.Core;

[UsedImplicitly]
public class FCPCoreMod : Mod
{
    public static FCPCoreMod Instance { get; private set; }
    public static HarmonyLib.Harmony Harmony { get; } = new HarmonyLib.Harmony("FCP.Core.Patches");
    public static FCPSettings Settings { get; private set; }

    public static T SettingsTab<T>() where T : SettingsTab => Settings.GetTab<T>();

    public const string LatePatchesCategory = "FCP.Core.LatePatches";
    public const string CurrencyPatchesCategory = "FCP.Core.Currency";
    public const string TentsPatchesCategory = "FCP.Core.Tents";
    
    private SettingsTab currentTab = null;

    public FCPCoreMod(ModContentPack content) : base(content)
    {
        Instance = this;
        Settings = GetSettings<FCPSettings>();
        
        Harmony.PatchAllUncategorized();
        Harmony.PatchCategory(CurrencyPatchesCategory);
        if (ModsConfig.IsActive("Rick.FCP.Tents"))
            Harmony.PatchCategory(TentsPatchesCategory);
        
        LongEventHandler.ExecuteWhenFinished(() =>
        {
            Harmony.PatchCategory(LatePatchesCategory);
        });
        FCPLog.Message("Beta version: bugs likely, if not guaranteed! " +
                   "Report bugs on steam workshop page or on discord: 3HEXN3Qbn4");
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
        foreach (var tab in Settings.Tabs)
            tab.OnWriteSettings();
    }

    public override string SettingsCategory() => "FCP_Settings_Category".Translate();
    
    private const float TabBarHeight = 32f;

    private static Color DimPrimary => new Color(TerminalColors.PrimaryColor.r, TerminalColors.PrimaryColor.g, TerminalColors.PrimaryColor.b, 0.5f);

    public override void DoSettingsWindowContents(Rect inRect)
    {
        GUI.color = TerminalColors.PrimaryColor;

        currentTab ??= SettingsTab<InfoSettings>();

        var tabBarRect = new Rect(inRect.x, inRect.y, inRect.width, TabBarHeight);
        DrawTabBar(tabBarRect);

        var mainRect = new Rect(inRect.x, tabBarRect.yMax + 8f, inRect.width, inRect.height - tabBarRect.height - 8f);

        if (currentTab is { Enabled: true })
        {
            currentTab.DoTabWindowContents(mainRect.ContractedBy(4f, 0f));
        }
        else if (currentTab != null)
        {
            GUI.color = DimPrimary;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(mainRect, "Requires the corresponding FCP module to be installed and active.");
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
        }

        GUI.color = Color.white;
    }

    private void DrawTabBar(Rect rect)
    {
        var tabs = Settings.Tabs.ToList();
        float tabWidth = rect.width / tabs.Count;

        for (int i = 0; i < tabs.Count; i++)
        {
            DrawTabButton(new Rect(rect.x + tabWidth * i, rect.y, tabWidth, rect.height), tabs[i]);
        }

        GUI.color = TerminalColors.PrimaryColor;
        Widgets.DrawLineHorizontal(rect.x, rect.yMax, rect.width);
    }

    private void DrawTabButton(Rect rect, SettingsTab tab)
    {
        bool selected = currentTab == tab;

        GUI.color = selected ? TerminalColors.HighlightColor : DimPrimary;
        Widgets.DrawLineHorizontal(rect.x, rect.y, rect.width);
        if (selected)
            Widgets.DrawLineHorizontal(rect.x, rect.yMax - 2f, rect.width);
        if (!selected && Mouse.IsOver(rect))
            Widgets.DrawHighlight(rect);

        Text.Anchor = TextAnchor.MiddleCenter;
        GUI.color = selected ? TerminalColors.HighlightColor : TerminalColors.PrimaryColor;
        Widgets.Label(rect, tab.TabName);
        Text.Anchor = TextAnchor.UpperLeft;
        GUI.color = TerminalColors.PrimaryColor;

        if (!tab.TabToolTip.NullOrEmpty())
            TooltipHandler.TipRegion(rect, tab.TabToolTip);

        if (Widgets.ButtonInvisible(rect))
        {
            currentTab = tab;
            WriteSettings();
        }
    }
}
