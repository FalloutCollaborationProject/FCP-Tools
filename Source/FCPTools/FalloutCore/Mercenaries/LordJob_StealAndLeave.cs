using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;

namespace FCP.Core
{
    public class LordJob_StealAndLeave : LordJob
    {
        private Faction faction;
        private IntVec3 entryPoint;

        public LordJob_StealAndLeave() { }

        public LordJob_StealAndLeave(Faction faction, IntVec3 entryPoint)
        {
            this.faction = faction;
            this.entryPoint = entryPoint;
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();

            LordToil_Steal toilSteal = new LordToil_Steal();
            stateGraph.StartingToil = toilSteal;

            LordToil_ExitMap toilExit = new LordToil_ExitMap(LocomotionUrgency.Jog, true, true);
            stateGraph.AddToil(toilExit);
            Transition stealToExit = new Transition(toilSteal, toilExit);
            stealToExit.AddTrigger(new Trigger_TicksPassed(GenDate.TicksPerHour * 2));
            stateGraph.AddTransition(stealToExit);

            Transition cleanupTransitionSteal = new Transition(toilSteal, null);
            cleanupTransitionSteal.AddTrigger(new Trigger_TickCondition(() => lord.ownedPawns.Count == 0, 1000));
            stateGraph.AddTransition(cleanupTransitionSteal);

            Transition cleanupTransitionExit = new Transition(toilExit, null);
            cleanupTransitionExit.AddTrigger(new Trigger_TickCondition(() => lord.ownedPawns.Count == 0, 1000));
            stateGraph.AddTransition(cleanupTransitionExit);


            return stateGraph;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref faction, "faction");
            Scribe_Values.Look(ref entryPoint, "entryPoint");
        }
    }


    public class LordToil_Steal : LordToil
    {
        public override void UpdateAllDuties()
        {
            foreach (Pawn pawn in lord.ownedPawns)
            {
                pawn.mindState.duty = new PawnDuty(DutyDefOf.Steal);
            }
        }
    }


}