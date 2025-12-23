using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RecipeGoodwillLimiter
{
    [HarmonyPatch]
    public class Bill_Production_ShouldDoNow_Patch
    {
        [HarmonyPatch(typeof(Bill_Production), "ShouldDoNow")]
        [HarmonyPrefix]
        public static void ShouldDoNow_GoodWillCheck(ref bool __result, Bill_Production __instance)
        {
            Faction playerFaction = Faction.OfPlayer;
            if (playerFaction == null)
            {
                __instance.suspended = true;
                __result = false;
            }
            RecipeExtension_GoodwillCheck modExtension = __instance.recipe.GetModExtension<RecipeExtension_GoodwillCheck>();
            Faction targetFact = Find.FactionManager.FirstFactionOfDef(modExtension.requireFaction);
            if (targetFact == null)
            {
                if (!__instance.suspended)
                {
                    Messages.Message("GoodwillUnmet".Translate(targetFact.Name, modExtension.minimumGoodwill), MessageTypeDefOf.NegativeEvent);
                }
                __instance.suspended = true;
                __result = false;
            }
            else
            {
                if (targetFact.defeated)
                {
                    if (modExtension.uncraftableIfFactionDefeated)
                    {
                        if (!__instance.suspended)
                        {
                            Messages.Message("GoodwillUnmet".Translate(targetFact.Name, modExtension.minimumGoodwill), MessageTypeDefOf.NegativeEvent);
                        }
                        __instance.suspended = true;
                        __result = false;
                    }
                }
                else if (targetFact.GoodwillWith(playerFaction) < modExtension.minimumGoodwill)
                {
                    if (!__instance.suspended)
                    {
                        Messages.Message("GoodwillUnmet".Translate(targetFact.Name,modExtension.minimumGoodwill), MessageTypeDefOf.NegativeEvent);
                    }
                    __instance.suspended = true;
                    __result = false;
                }
            }
        }
    }
}
