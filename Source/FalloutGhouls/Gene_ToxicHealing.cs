using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace FCP_Ghoul
{
    public class Gene_ToxicHealing : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();
            
            // Add a hediff that does the actual work
            if (!pawn.health.hediffSet.HasHediff(HediffDefOf_Ghoul.ToxicHealing))
            {
                pawn.health.AddHediff(HediffDefOf_Ghoul.ToxicHealing);
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf_Ghoul.ToxicHealing);
            if (hediff != null)
            {
                pawn.health.RemoveHediff(hediff);
            }
        }
    }
    
    [DefOf]
    public static class HediffDefOf_Ghoul
    {
        public static HediffDef ToxicHealing;
    }
}
