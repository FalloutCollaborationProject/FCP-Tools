using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace FCP.ApparelExtensions;

[StaticConstructorOnStartup]
public static class HarmonyInit
{
    static HarmonyInit()
    {
        new Harmony("FCP.ApparelExtension").PatchAll();
    }
}