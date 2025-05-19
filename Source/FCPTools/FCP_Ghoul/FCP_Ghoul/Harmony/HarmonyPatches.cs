using HarmonyLib;
using RimWorld;
using Verse;

namespace FCP_Ghoul
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony harmony = new ("rimworld.fcp.ghouls");
            
            harmony.Patch(original: AccessTools.Method(typeof(GameCondition_ToxicFallout), "DoPawnToxicDamage"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(GameCondition_ToxicFallout_DoPawnToxicDamage_Postfix)));
        }
        
        public static void GameCondition_ToxicFallout_DoPawnToxicDamage_Postfix(
            Pawn p, bool protectedByRoof, float extraFactor)
        {
            if (p?.genes?.HasActiveGene(FCPGDefOf.FCP_Gene_ToxHeal) == true)
            {
                HealthUtility.AdjustSeverity(p, FCPGDefOf.FCP_Hediff_ToxHeal, 1);
            }
        } 
    }
}