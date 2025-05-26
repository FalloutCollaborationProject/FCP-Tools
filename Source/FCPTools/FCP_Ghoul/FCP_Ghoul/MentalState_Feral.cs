using Verse;
using Verse.AI;

namespace FCP_Ghoul
{
    public class MentalState_Feral : MentalState_Berserk
    {
        public override bool ForceHostileTo(Thing t)
        {
            Pawn p;
            if (t.def.race?.Humanlike!=true||t.def.race.IsMechanoid||t.def.race.IsAnomalyEntity)
            {
                return true;
                
            }
            p = t as Pawn;
            if (p?.genes?.Xenotype.HasModExtension< TurnFeral_ModExtension>()==true)
            {
                return false;
            }
            return base.ForceHostileTo(t);

        }
    }
}
