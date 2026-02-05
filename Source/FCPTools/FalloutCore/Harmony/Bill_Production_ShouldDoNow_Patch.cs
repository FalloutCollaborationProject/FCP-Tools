using HarmonyLib;

namespace FCP.Core;

[HarmonyPatch]
public class Bill_Production_ShouldDoNow_Patch
{
    [HarmonyPatch(typeof(Bill_Production), "ShouldDoNow")]
    [HarmonyPrefix]
    public static void ShouldDoNow_GoodWillCheck(ref bool __result, Bill_Production __instance)
    {
        var modExtension = __instance.recipe.GetModExtension<RecipeExtension_GoodwillCheck>();
        if (modExtension == null)
            return;
        
        Faction faction = Find.FactionManager.FirstFactionOfDef(modExtension.requireFaction);
        if (faction == null)
        {
            if (!__instance.suspended)
            {
                Messages.Message("Goodwill_FactionDontExist".Translate(modExtension.requireFaction.defName), MessageTypeDefOf.NegativeEvent);
            }
            __instance.suspended = true;
            __result = false;
        }
        else
        {
            if (faction.defeated && modExtension.uncraftableIfFactionDefeated)
            {
                if (!__instance.suspended)
                {
                    Messages.Message("Goodwill_FactionDefeated".Translate(faction.Name),
                        MessageTypeDefOf.NegativeEvent);
                }

                __instance.suspended = true;
                __result = false;
            }
            else if (faction.GoodwillWith(Faction.OfPlayer) < modExtension.minimumGoodwill)
            {
                if (!__instance.suspended)
                {
                    Messages.Message("GoodwillUnmet".Translate(faction.Name,modExtension.minimumGoodwill), MessageTypeDefOf.NegativeEvent);
                }
                __instance.suspended = true;
                __result = false;
            }
        }
    }
}