using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RecipeGoodwillLimiter
{
    [StaticConstructorOnStartup]
    public static class StartPatch
    {
        static StartPatch()
        {
            Harmony harmony = new Harmony("FarmerJoe.RecipeGoodwillLimiter");
            harmony.PatchAll();
        }
    }
}
