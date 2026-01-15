using Verse.AI.Group;

namespace Thek_BuildingArrivalMode
{
    public class TransitionAction_BuildingArrivalMode_EnsureHaveExitDestination : TransitionAction
    {
        /// <summary>
        /// Checks if lordToil_Travel has a destination already, and if it doesn't, sets it to the cell where pawns come and go from the portal
        /// </summary>
        /// <param name="trans"></param>
        public override void DoAction(Transition trans)
        {
            LordToil_Travel lordToil_Travel = (LordToil_Travel)trans.target;
            if (!lordToil_Travel.HasDestination() && lordToil_Travel.lord.ownedPawns.Where((Pawn x) => x.Spawned).TryRandomElement(out var _))
            {
                lordToil_Travel.SetDestination(PawnsArrivalModeWorker_BuildingArrivalMode.modExtension.tileToSpawn);
            }
        }
    }
}