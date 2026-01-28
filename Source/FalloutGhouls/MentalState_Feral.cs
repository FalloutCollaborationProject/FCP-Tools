using Verse;
using Verse.AI;

namespace FCP_Ghoul
{
    public class MentalState_Feral : MentalState_Berserk
    {
        private IntVec3 homePosition;
        
        public override void PostStart(string reason)
        {
            base.PostStart(reason);
            homePosition = pawn.Position;
        }
        
        public override bool ForceHostileTo(Thing t)
        {
            if (t.def.race?.Humanlike != true || t.def.race.IsMechanoid || t.def.race.IsAnomalyEntity)
                return true;
            
            Pawn p = t as Pawn;
            
            if (p?.genes?.Xenotype?.HasModExtension<TurnFeral_ModExtension>() == true)
                return false;
            
            return base.ForceHostileTo(t);
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref homePosition, "homePosition");
        }
    }
}
