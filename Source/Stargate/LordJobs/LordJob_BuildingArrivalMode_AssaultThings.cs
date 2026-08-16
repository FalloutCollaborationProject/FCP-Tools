using System;
using Verse.AI.Group;

namespace Thek_BuildingArrivalMode
{
    public class LordJob_BuildingArrivalMode_AssaultThings : LordJob_AssaultThings
    {

        private Faction assaulterFaction;
        private List<Thing> things;
        private bool useAvoidGridSmart;
        private float damageFraction;
        public override bool AddFleeToil => false;
        public LordJob_BuildingArrivalMode_AssaultThings(Faction assaulterFaction, List<Thing> things, float damageFraction = 1f, bool useAvoidGridSmart = false) : base(assaulterFaction, things, damageFraction, useAvoidGridSmart)
        {
            this.assaulterFaction = assaulterFaction;
            this.things = things;
            this.useAvoidGridSmart = useAvoidGridSmart;
            this.damageFraction = damageFraction;
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new();
            LordToil lordToil = new LordToil_AssaultThings(things);
            if (useAvoidGridSmart)
            {
                lordToil.useAvoidGrid = true;
            }
            stateGraph.AddToil(lordToil);
            LordToil_BuildingArrivalMode_ExitMapAndDefendSelf lordToilEscape = new()
            {
                useAvoidGrid = true
            };
            stateGraph.AddToil(lordToilEscape);
            Transition transition = new(lordToil, lordToilEscape);
            transition.AddTrigger(new Trigger_ThingsDamageTaken(things, damageFraction));
            stateGraph.AddTransition(transition);

            LordToil_BuildingArrivalMode_PanicFlee lordToil_PanicFlee = new()
            {
                useAvoidGrid = true
            };

            for (int i = 0; i < stateGraph.lordToils.Count; i++)
            {
                Transition fleeTransition = new(stateGraph.lordToils[i], lordToil_PanicFlee);
                fleeTransition.AddPreAction(new TransitionAction_Message("MessageFightersFleeing".Translate(assaulterFaction.def.pawnsPlural.CapitalizeFirst(), assaulterFaction.Name)));
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
