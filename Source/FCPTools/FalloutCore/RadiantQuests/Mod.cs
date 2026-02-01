using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FCP.Core.RadiantQuests
{
    [StaticConstructorOnStartup]
    public static class HarmonyStarter
    {
        static HarmonyStarter()
        {
            Log.Message("Harmony starter started");
            Harmony harmony = new Harmony("FCP.RadiantQuests");
            harmony.PatchAll();
        }
    }
}

