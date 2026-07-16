using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FCP.Enlist;

class EnlistSettings : ModSettings
{
    public Dictionary<string, bool> enlistStates = new Dictionary<string, bool>();
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref enlistStates, "enlistStates", LookMode.Value, LookMode.Value, ref enlistKeys, ref boolValues);
    }

    private List<string> enlistKeys;
    private List<bool> boolValues;
    public void DoSettingsWindowContents(Rect inRect)
    {
        var keys = enlistStates.Keys.ToList().OrderByDescending(x => x).ToList();
        Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
        Rect rect2 = new Rect(0f, 0f, inRect.width - 30f, 30 + (keys.Count * 24));
        Widgets.BeginScrollView(rect, ref scrollPosition, rect2, true);
        Listing_Standard listingStandard = new Listing_Standard();
        listingStandard.Begin(rect2);
        listingStandard.Label("RH.ActiveEnlistOptions".Translate());
        for (int num = keys.Count - 1; num >= 0; num--)
        {
            var test = enlistStates[keys[num]];
            listingStandard.CheckboxLabeled(keys[num], ref test);
            enlistStates[keys[num]] = test;
        }
        listingStandard.End();
        Widgets.EndScrollView();
        base.Write();
    }

    private static Vector2 scrollPosition = Vector2.zero;

}