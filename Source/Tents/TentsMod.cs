using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace FCP.Tents;

public class TentsMod : Mod
{
    public TentsMod(ModContentPack content) : base(content)
    {
        GetSettings<ModSettings>();
        var harmony = new Harmony("FCP.Tents");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        base.DoSettingsWindowContents(inRect);
        GetSettings<ModSettings>().DoWindowContents(inRect);
    }

    public override string SettingsCategory()
    {
        return "Camping Tent";
    }
}