using System;
using Verse.AI;
using Verse.AI.Group;

namespace Thek_BuildingArrivalMode
{
    public class LordJob_BuildingArrivalMode_AssistColony : LordJob_AssistColony
    {
        // This is a copy from LordJob_AssistColony
        // I basically change all the LordToils that make pawns leave in any way into my own custom ones
        // I also add my own PanicFlee, ignoring vanilla's

        private Faction faction;
        public override bool AddFleeToil => !PawnsArrivalModeWorker_BuildingArrivalMode.modExtension?.shouldOverrideFleeToil == true;
        private IntVec3 fallbackLocation;

        public LordJob_BuildingArrivalMode_AssistColony(Faction faction, IntVec3 fallbackLocation) : base(faction, fallbackLocation)
        {
            this.faction = faction;
            this.fallbackLocation = fallbackLocation;
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new();
            LordToil_HuntEnemies lordToil_HuntEnemies = new(fallbackLocation);
            stateGraph.AddToil(lordToil_HuntEnemies);
            StateGraph stateGraph2 = new LordJob_Travel(IntVec3.Invalid).CreateGraph();
            LordToil startingToil = stateGraph.AttachSubgraph(stateGraph2).StartingToil;
            LordToil_ExitMap lordToil_ExitMap = new();
            stateGraph.AddToil(lordToil_ExitMap);
            LordToil_ExitMap lordToil_ExitMap2 = new(LocomotionUrgency.Jog, canDig: true);
            stateGraph.AddToil(lordToil_ExitMap2);
            Transition transition = new(lordToil_HuntEnemies, startingToil);
            transition.AddPreAction(new TransitionAction_Message("MessageVisitorsDangerousTemperature".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
            transition.AddPreAction(new TransitionAction_BuildingArrivalMode_EnsureHaveExitDestination());
            // This was changed from ensuring a path to the edge to ensuring a path to the portal
            transition.AddTrigger(new Trigger_PawnExperiencingDangerousTemperatures());
            transition.AddPostAction(new TransitionAction_EndAllJobs());
            stateGraph.AddTransition(transition);
            Transition transition2 = new(lordToil_HuntEnemies, lordToil_ExitMap2);
            transition2.AddSource(lordToil_ExitMap);
            transition2.AddSources(stateGraph2.lordToils);
            transition2.AddPreAction(new TransitionAction_Message("MessageVisitorsTrappedLeaving".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
            transition2.AddTrigger(new Trigger_PawnCannotReachMapEdge());
            stateGraph.AddTransition(transition2);
            Transition transition3 = new(lordToil_ExitMap2, startingToil);
            transition3.AddTrigger(new Trigger_PawnCanReachMapEdge());
            transition3.AddPreAction(new TransitionAction_BuildingArrivalMode_EnsureHaveExitDestination());
            // This was changed from ensuring a path to the edge to ensuring a path to the portal
            stateGraph.AddTransition(transition3);
            Transition transition4 = new(lordToil_HuntEnemies, startingToil);
            transition4.AddPreAction(new TransitionAction_Message("MessageFriendlyFightersLeaving".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
            transition4.AddTrigger(new Trigger_TicksPassed(25000));
            transition4.AddPreAction(new TransitionAction_BuildingArrivalMode_EnsureHaveExitDestination());
            // This was changed from ensuring a path to the edge to ensuring a path to the portal
            stateGraph.AddTransition(transition4);
            Transition transition5 = new(startingToil, lordToil_ExitMap);
            transition5.AddTrigger(new Trigger_Memo("TravelArrived"));
            stateGraph.AddTransition(transition5);

            LordToil_BuildingArrivalMode_PanicFlee lordToil_PanicFlee = new()
            {
                // The fleeing lord that happens when you defeat a bunch of the raiders changed so they go in the portal
                useAvoidGrid = true
            };

            //The for loop underneath makes all the transitions aim towards the above PanicFlee, same thing vanilla's PanicFlee does
            for (int i = 0; i < stateGraph.lordToils.Count; i++)
            {
                Transition fleeTransition = new(stateGraph.lordToils[i], lordToil_PanicFlee);
                fleeTransition.AddPreAction(new TransitionAction_Message("MessageFightersFleeing".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
                fleeTransition.AddTrigger(new Trigger_FractionPawnsLost(0.5f));
                fleeTransition.AddPostAction(new TransitionAction_Custom((Action)delegate
                {
                    QuestUtility.SendQuestTargetSignals(lord.questTags, "Fleeing", lord.Named("SUBJECT"));
                }));
                stateGraph.AddTransition(fleeTransition, highPriority: true);
            }
            stateGraph.AddToil(lordToil_PanicFlee);

            return stateGraph;
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }
    }
}
