using Verse.AI;
using Verse.AI.Group;

namespace Thek_BuildingArrivalMode
{
    public class LordToil_BuildingArrivalMode_ExitMap : LordToil_ExitMap
    {
        //Modified LordToil_ExitMap calling custom duties
        public override DutyDef ExitDuty => DutyDefOfs.Thek_ExitMap_BuildingArrivalMode;

        public LordToil_BuildingArrivalMode_ExitMap(LocomotionUrgency locomotion = LocomotionUrgency.None, bool canDig = false, bool interruptCurrentJob = false)
        {
            data = new LordToilData_ExitMap();
            Data.locomotion = locomotion;
            Data.canDig = canDig;
            Data.interruptCurrentJob = interruptCurrentJob;
        }

        /// <summary>
        /// As far as i understand it, changes all the duties for all the pawns from the raid into the one set in ExitDuty's overrides
        /// </summary>
        public override void UpdateAllDuties()
        {
            LordToilData_ExitMap lordToilData_ExitMap = Data;
            for (int i = 0; i < lord.ownedPawns.Count; i++)
            {
                PawnDuty pawnDuty = new(ExitDuty)
                {
                    locomotion = lordToilData_ExitMap.locomotion,
                    canDig = lordToilData_ExitMap.canDig
                };
                Pawn pawn = lord.ownedPawns[i];
                pawn.mindState.duty = pawnDuty;
                if (Data.interruptCurrentJob && pawn.jobs.curJob != null)
                {
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                }
            }
        }
    }
}
