using Verse.AI.Group;

namespace Thek_BuildingArrivalMode
{
    public class TransitionAction_BuildingArrivalMode_EnsureHaveExitDestination : TransitionAction
    {
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