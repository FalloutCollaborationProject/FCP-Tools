using Verse.AI;

namespace Thek_BuildingArrivalMode
{
    public class LordToil_BuildingArrivalMode_ExitMapAndDefendSelf : LordToil_ExitMapAndDefendSelf
    {
        /// <summary>
        /// As far as i understand it, changes all the duties for all the pawns from the raid to the one in that defOf
        /// </summary>
        public override void UpdateAllDuties()
        {
            for (int i = 0; i < lord.ownedPawns.Count; i++)
            {
                lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOfs.Thek_ExitMapAndDefendSelf_BuildingArrivalMode);
            }
        }
    }
}
