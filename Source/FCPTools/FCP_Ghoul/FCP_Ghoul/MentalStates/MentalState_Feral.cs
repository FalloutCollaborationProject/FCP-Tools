using RimWorld;
using Verse;
using Verse.AI;

namespace FCP_Ghoul
{
    public class MentalState_Feral : MentalState_Berserk
    {
        public override bool ForceHostileTo(Thing t)
        {
            if (!t.def.race.Humanlike 
                || t.def.race.IsMechanoid 
                || t.def.race.IsAnomalyEntity)
            {
                return true;
            }
            
            if (t is not Pawn p) 
                return true;
            
            XenotypeDef xenotype = p.genes?.Xenotype;
            bool isGhoulXenotype = xenotype == FCPGDefOf.FCP_Xenotype_Ghoul 
                                   || xenotype == FCPGDefOf.FCP_Xenotype_Ghoul_Glowing_One;
            
            return !isGhoulXenotype && base.ForceHostileTo(t);
        }
    }
}