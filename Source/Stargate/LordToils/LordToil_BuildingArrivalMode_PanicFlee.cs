using Verse.AI;
using Verse.AI.Group;


namespace Thek_BuildingArrivalMode
{
    public class LordToil_BuildingArrivalMode_PanicFlee : LordToil
    {
        // This is an edited copy of vanilla's LordToil_PanicFlee, replacing whatever makes them go to the border of the map with my own things
        public override bool AllowSatisfyLongNeeds => false;
        public override bool AllowSelfTend => false;

        public override void Init()
        {
            base.Init();
            for (int i = 0; i < lord.ownedPawns.Count; i++)
            {
                Pawn pawn = lord.ownedPawns[i];
                if (!pawn.InAggroMentalState && (!HasFleeingDuty(pawn) || pawn.mindState.duty.def == DutyDefOfs.Thek_PanicFlee_BuildingArrivalMode))
                {
                    pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOfs.Thek_PanicFlee_BuildingArrivalMode);
                }
            }
        }

        /// <summary>
        /// As far as i understand it, changes all the duties for all the pawns from the raid into the one set referenced by the defOf
        /// </summary>
        public override void UpdateAllDuties()
        {
            for (int i = 0; i < lord.ownedPawns.Count; i++)
            {
                Pawn pawn = lord.ownedPawns[i];
                if (!HasFleeingDuty(pawn))
                {
                    pawn.mindState.duty = new PawnDuty(DutyDefOfs.Thek_PanicFlee_BuildingArrivalMode);
                }
            }
        }

        private bool HasFleeingDuty(Pawn pawn)
        {
            if (pawn.mindState.duty == null)
            {
                return false;
            }
            if (pawn.mindState.duty.def == DutyDefOfs.Thek_PanicFlee_BuildingArrivalMode
                || pawn.mindState.duty.def == DutyDefOfs.Thek_Steal_BuildingArrivalMode
                || pawn.mindState.duty.def == DutyDefOfs.Thek_Kidnap_BuildingArrivalMode)
            {
                return true;
            }
            return false;
        }
    }
}
