using Verse.AI;

namespace Thek_BuildingArrivalMode
{
    public class LordToil_BuildingArrivalMode_KidnapCover : LordToil_KidnapCover
    {
        /// <summary>
        /// As far as i understand it, changes the duty given when calling this LordToil from another class into the one referenced by the DefOf
        /// </summary>
        protected override DutyDef DutyDef => DutyDefOfs.Thek_Kidnap_BuildingArrivalMode;
    }
}