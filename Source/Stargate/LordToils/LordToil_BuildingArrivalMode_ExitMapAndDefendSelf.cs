using Verse.AI;

namespace Thek_BuildingArrivalMode
{
    public class LordToil_BuildingArrivalMode_ExitMapAndDefendSelf : LordToil_ExitMapAndDefendSelf
    {
        public override void UpdateAllDuties()
        {
            for (int i = 0; i < lord.ownedPawns.Count; i++)
            {
                lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOfs.Thek_ExitMapAndDefendSelf_BuildingArrivalMode);
            }
        }
    }
}
