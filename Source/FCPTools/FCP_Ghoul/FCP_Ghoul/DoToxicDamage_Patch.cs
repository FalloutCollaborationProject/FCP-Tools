using RimWorld;
using Verse;
using HarmonyLib;

namespace FCP_Ghoul
{
    [HarmonyPatch(typeof(ToxicUtility),nameof(ToxicUtility.DoPawnToxicDamage))]
    public class DoToxicDamage_Patch 
    {
        public static void Postfix(Pawn p)
        {
            if (p.genes?.HasActiveGene(Ghoul_Cache.ToxHeal) == true)
            {
                HealthUtility.AdjustSeverity(p, Ghoul_Cache.ToxHealHediff, 1);
            }

        } 
    }
}
