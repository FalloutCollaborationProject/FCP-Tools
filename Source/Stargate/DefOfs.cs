using Verse.AI;

namespace Thek_BuildingArrivalMode
{
    [DefOf]
    sealed class DutyDefOfs
    {
        public static DutyDef Thek_ExitMap_BuildingArrivalMode; 
        public static DutyDef Thek_ExitMapAndDefendSelf_BuildingArrivalMode; 
        public static DutyDef Thek_PanicFlee_BuildingArrivalMode; 
        public static DutyDef Thek_Kidnap_BuildingArrivalMode; 
        public static DutyDef Thek_Steal_BuildingArrivalMode; 
    }

    [DefOf]
    sealed class JobDefOfs
    {
        public static JobDef Thek_GotoNoExitCellCheck; 
        public static JobDef Thek_BuildingArrivalMode_Kidnap; 
        public static JobDef Thek_BuildingArrivalMode_StealThing; 
    }

    [DefOf]
    sealed class MentalStateDefOfs
    {
        public static MentalStateDef Thek_PanicFlee_BuildingArrivalMode; 
    }
}
