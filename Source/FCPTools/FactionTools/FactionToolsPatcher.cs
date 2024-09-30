global using System;
global using System.Collections.Generic;
global using System.Linq;
global using RimWorld;
global using Verse;
global using UnityEngine;
global using JetBrains.Annotations;
using HarmonyLib;

namespace FCP;

[StaticConstructorOnStartup]
public class FactionToolsPatcher
{
    static FactionToolsPatcher()
    {
        var harmony = new Harmony("FCP.FactionTools");
        harmony.PatchAll();
    }
}