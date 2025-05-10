using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;
namespace FCP_Ghoul
{
    [HarmonyPatch(typeof(GameCondition_ToxicFallout),nameof(GameCondition_ToxicFallout.DoPawnToxicDamage),new[] { 
    typeof(Pawn),typeof(bool),typeof(float)
    })]
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
