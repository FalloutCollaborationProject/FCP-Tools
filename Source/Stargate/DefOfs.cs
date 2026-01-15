using Verse.AI;

namespace Thek_BuildingArrivalMode
{
    [DefOf]
    sealed class DutyDefOfs
    {
        public static DutyDef Thek_ExitMap_BuildingArrivalMode; // Duty to leave when they give up
        public static DutyDef Thek_ExitMapAndDefendSelf_BuildingArrivalMode; // Duty to leave and protect themselves when they give up
        public static DutyDef Thek_PanicFlee_BuildingArrivalMode; // Duty to leave when enough raiders have been defeated
        public static DutyDef Thek_Kidnap_BuildingArrivalMode; // Duty to leave while kidnapping
        public static DutyDef Thek_Steal_BuildingArrivalMode; // Duty to leave while stealing
    }

    [DefOf]
    sealed class JobDefOfs
    {
        public static JobDef Thek_GotoNoExitCellCheck; // Goto job that doesn't require the pawn to be on the borders of the tile to leave the map
        public static JobDef Thek_BuildingArrivalMode_Kidnap; // Kidnap job that inherits from stealthing
        public static JobDef Thek_BuildingArrivalMode_StealThing; // Job that handles going to a thing and grabbing it, then calling goto
    }

    [DefOf]
    sealed class MentalStateDefOfs
    {
        public static MentalStateDef Thek_PanicFlee_BuildingArrivalMode; // It's just to mimmick the flee mentalstate from vanilla raids, doesn't do anything at all, the def is a copypaste from vanilla panicflee lol
    }
}
