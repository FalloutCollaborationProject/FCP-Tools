using System;
using Verse.AI;
using Verse.AI.Group;

namespace Thek_BuildingArrivalMode
{
    public class LordJob_BuildingArrivalMode_Steal : LordJob_Steal
    {
        // Modified LordJob_Steal, calling custom LordToils that go back to the portal
        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new();
            LordToil_StealCover lordToil_StealCover = new LordToil_BuildingArrive_LordToil_StealCover
            {
                useAvoidGrid = true
            };
            stateGraph.AddToil(lordToil_StealCover);
            LordToil_StealCover lordToil_StealCover2 = new LordToil_BuildingArrive_LordToil_StealCover
            {
                cover = false,
                useAvoidGrid = true
            };
            stateGraph.AddToil(lordToil_StealCover2);
            Transition transition = new(lordToil_StealCover, lordToil_StealCover2);
            transition.AddTrigger(new Trigger_TicksPassedAndNoRecentHarm(1200));
            stateGraph.AddTransition(transition);
            return stateGraph;
        }
    }
}
