using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FCP.Core;

public class InfoSettings : SettingsTab
{
    public override string TabName => "FCP_Settings.Info".Translate();
    public override string TabToolTip => "FCP_Settings.Info.tt".Translate();

    private Vector2 scrollPosition;
    private float viewHeight = 2000f;

    public override void DoTabWindowContents(Rect tabRect)
    {
        Rect outRect = tabRect;
        Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, viewHeight);

        Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
        Listing_Standard list = new Listing_Standard { maxOneColumn = true };
        list.Begin(viewRect);

        Text.Font = GameFont.Small;
        list.Label("FCP_Settings_Info_Intro".Translate());
        list.Gap(12f);

        DrawGroup(list, "FCP_Settings_Info_Group_Combat");
        DrawGroup(list, "FCP_Settings_Info_Group_PowerArmor");
        DrawGroup(list, "FCP_Settings_Info_Group_Mutation");
        DrawGroup(list, "FCP_Settings_Info_Group_Survival");
        DrawGroup(list, "FCP_Settings_Info_Group_Settlements");
        DrawGroup(list, "FCP_Settings_Info_Group_Story");
        DrawGroup(list, "FCP_Settings_Info_Group_World");
        DrawGroup(list, "FCP_Settings_Info_Group_Misc");

        list.End();
        if (Event.current.type == EventType.Layout)
            viewHeight = list.CurHeight;
        Widgets.EndScrollView();
    }

    private static void DrawGroup(Listing_Standard list, string groupKey)
    {
        list.Gap(6f);
        Text.Font = GameFont.Medium;
        list.Label((groupKey + ".Title").Translate());
        Text.Font = GameFont.Small;
        list.GapLine();

        foreach (string entryKey in EntryKeysFor(groupKey))
        {
            list.Label("<b>" + (entryKey + ".Title").Translate() + "</b>");
            list.Label((entryKey + ".Desc").Translate());
            list.Gap(10f);
        }
    }

    private static IEnumerable<string> EntryKeysFor(string groupKey) => groupKey switch
    {
        "FCP_Settings_Info_Group_Combat" =>
        [
            "FCP_Settings_Info_VATS",
            "FCP_Settings_Info_WeaponCondition",
            "FCP_Settings_Info_WeaponRepair",
            "FCP_Settings_Info_ItemRarity",
            "FCP_Settings_Info_Legendary",
            "FCP_Settings_Info_HeavyWeapons",
            "FCP_Settings_Info_Homing",
            "FCP_Settings_Info_LaserWeapons",
            "FCP_Settings_Info_Grenades",
        ],
        "FCP_Settings_Info_Group_PowerArmor" =>
        [
            "FCP_Settings_Info_PowerArmor",
            "FCP_Settings_Info_Robotics",
            "FCP_Settings_Info_Cyberdogs",
        ],
        "FCP_Settings_Info_Group_Mutation" =>
        [
            "FCP_Settings_Info_Ghoulification",
            "FCP_Settings_Info_Gooification",
            "FCP_Settings_Info_SuperMutants",
            "FCP_Settings_Info_Mutations",
            "FCP_Settings_Info_Xenotypes",
        ],
        "FCP_Settings_Info_Group_Survival" =>
        [
            "FCP_Settings_Info_AutoStim",
            "FCP_Settings_Info_Radiation",
        ],
        "FCP_Settings_Info_Group_Settlements" =>
        [
            "FCP_Settings_Info_NamedSettlements",
            "FCP_Settings_Info_FactionCurrencies",
            "FCP_Settings_Info_FactionBehavior",
            "FCP_Settings_Info_HiddenFactions",
            "FCP_Settings_Info_Vertibird",
            "FCP_Settings_Info_VertibirdConstruction",
            "FCP_Settings_Info_VertibirdCrash",
            "FCP_Settings_Info_FactionOutposts",
            "FCP_Settings_Info_Radio",
        ],
        "FCP_Settings_Info_Group_Story" =>
        [
            "FCP_Settings_Info_Enlistment",
            "FCP_Settings_Info_UniqueCharacters",
            "FCP_Settings_Info_UniqueItems",
            "FCP_Settings_Info_CustomTraits",
            "FCP_Settings_Info_FactionIdeology",
            "FCP_Settings_Info_CustomQuests",
            "FCP_Settings_Info_Gauntlet",
        ],
        "FCP_Settings_Info_Group_World" =>
        [
            "FCP_Settings_Info_Holotapes",
            "FCP_Settings_Info_Terminal_Hacking",
            "FCP_Settings_Info_ResearchTrees",
            "FCP_Settings_Info_Blueprints",
            "FCP_Settings_Info_ProductionBenches",
            "FCP_Settings_Info_Tents",
            "FCP_Settings_Info_PocketMaps",
        ],
        "FCP_Settings_Info_Group_Misc" =>
        [
            "FCP_Settings_Info_GearRestrictions",
            "FCP_Settings_Info_LootableFurniture",
            "FCP_Settings_Info_FeralWildlife",
            "FCP_Settings_Info_AnimalHerds",
        ],
        _ => [],
    };
}
