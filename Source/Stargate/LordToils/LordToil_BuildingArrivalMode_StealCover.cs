using Verse.AI;

namespace Thek_BuildingArrivalMode
{
    public class LordToil_BuildingArrive_LordToil_StealCover : LordToil_StealCover
    {
        /// <summary>
        /// As far as i understand it, changes the duty given when calling this LordToil from another class into the one referenced by the DefOf
        /// </summary>
        protected override DutyDef DutyDef => DutyDefOfs.Thek_Steal_BuildingArrivalMode;
    }
}
