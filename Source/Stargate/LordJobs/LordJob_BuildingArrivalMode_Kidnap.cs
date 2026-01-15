using System;
using Verse.AI;
using Verse.AI.Group;

namespace Thek_BuildingArrivalMode
{
    public class LordJob_BuildingArrivalMode_Kidnap : LordJob_Kidnap
    {
        // Modified LordJob_Kidnap, calling custom LordToils that go back to the portal
        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new();
            LordToil_KidnapCover lordToil_KidnapCover = new LordToil_BuildingArrivalMode_KidnapCover
            {
                useAvoidGrid = true
            };
            stateGraph.AddToil(lordToil_KidnapCover);
            LordToil_KidnapCover lordToil_KidnapCover2 = new LordToil_BuildingArrivalMode_KidnapCover
            {
                cover = false,
                useAvoidGrid = true
            };
            stateGraph.AddToil(lordToil_KidnapCover2);
            Transition transition = new(lordToil_KidnapCover, lordToil_KidnapCover2);
            transition.AddTrigger(new Trigger_TicksPassed(1200));
            stateGraph.AddTransition(transition);
            return stateGraph;
        }
    }
}
