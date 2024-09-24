using UnityEngine;
using Verse;
using System.Text;
using Verse.AI;
using HarmonyLib;

namespace FalloutCore
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            new Harmony("FalloutCoreGooification").PatchAll();
        }
    }
}

