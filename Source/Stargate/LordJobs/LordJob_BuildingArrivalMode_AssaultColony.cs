using System;
using Verse.AI;
using Verse.AI.Group;

namespace Thek_BuildingArrivalMode
{
    public class LordJob_BuildingArrivalMode_AssaultColony : LordJob_AssaultColony
    {
        // This is a copy from LordJob_AssaultColony
        // I basically change all the LordToils that make pawns leave in any way into my own custom ones
        // I also add my own PanicFlee, ignoring vanilla's

        private Faction assaulterFaction;
        private bool canKidnap = true;
        private bool canTimeoutOrFlee = true;
        private bool sappers;
        private bool useAvoidGridSmart;
        private bool canSteal = true;
        private bool breachers;
        private bool canPickUpOpportunisticWeapons;

        private static readonly IntRange AssaultTimeBeforeGiveUp = new(26000, 38000);
        private static readonly IntRange SapTimeBeforeGiveUp = new(33000, 38000);
        private static readonly IntRange BreachTimeBeforeGiveUp = new(33000, 38000);
        public override bool AddFleeToil => false;

        public LordJob_BuildingArrivalMode_AssaultColony(Faction assaulterFaction, bool canKidnap = true, bool canTimeoutOrFlee = true, bool sappers = false, bool useAvoidGridSmart = false, bool canSteal = true, bool breachers = false, bool canPickUpOpportunisticWeapons = false) : base(assaulterFaction, canKidnap, canTimeoutOrFlee, sappers, useAvoidGridSmart, canSteal, breachers, canPickUpOpportunisticWeapons)
        {
            this.assaulterFaction = assaulterFaction;
            this.canKidnap = canKidnap;
            this.canTimeoutOrFlee = canTimeoutOrFlee;
            this.sappers = sappers;
            this.useAvoidGridSmart = useAvoidGridSmart;
            this.canSteal = canSteal;
            this.breachers = breachers;
            this.canPickUpOpportunisticWeapons = canPickUpOpportunisticWeapons;
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new();
            List<LordToil> list = new();
            LordToil lordToil = null;
            if (sappers)
            {
                lordToil = new LordToil_AssaultColonySappers();
                if (useAvoidGridSmart)
                {
                    lordToil.useAvoidGrid = true;
                }
                stateGraph.AddToil(lordToil);
                list.Add(lordToil);
                Transition transition = new(lordToil, lordToil, canMoveToSameState: true);
                transition.AddTrigger(new Trigger_PawnLost());
                stateGraph.AddTransition(transition);
            }
            LordToil lordToil2 = null;
            if (breachers)
            {
                lordToil2 = new LordToil_AssaultColonyBreaching();
                if (useAvoidGridSmart)
                {
                    lordToil2.useAvoidGrid = useAvoidGridSmart;
                }
                stateGraph.AddToil(lordToil2);
                list.Add(lordToil2);
            }
            LordToil lordToil3 = new LordToil_AssaultColony(attackDownedIfStarving: false, canPickUpOpportunisticWeapons);
            if (useAvoidGridSmart)
            {
                lordToil3.useAvoidGrid = true;
            }
            stateGraph.AddToil(lordToil3);
            LordToil_BuildingArrivalMode_ExitMap lordToilExit = new(LocomotionUrgency.Jog, canDig: false, interruptCurrentJob: true)
            {
                // The giving up lord that makes them go back to the portal
                useAvoidGrid = true
            };
            stateGraph.AddToil(lordToilExit);
            if (sappers)
            {
                Transition transition2 = new Transition(lordToil, lordToil3);
                transition2.AddTrigger(new Trigger_NoFightingSappers());
                stateGraph.AddTransition(transition2);
            }
            if (assaulterFaction != null && assaulterFaction.def.humanlikeFaction)
            {
                if (canTimeoutOrFlee)
                {
                    Transition transition3 = new Transition(lordToil3, lordToilExit);
                    transition3.AddSources(list);
                    transition3.AddTrigger(new Trigger_TicksPassed((sappers ? SapTimeBeforeGiveUp : ((!breachers) ? AssaultTimeBeforeGiveUp : BreachTimeBeforeGiveUp)).RandomInRange));
                    transition3.AddPreAction(new TransitionAction_Message("MessageRaidersGivenUpLeaving".Translate(assaulterFaction.def.pawnsPlural.CapitalizeFirst(), assaulterFaction.Name)));
                    stateGraph.AddTransition(transition3);
                    Transition transition4 = new Transition(lordToil3, lordToilExit);
                    transition4.AddSources(list);
                    float randomInRange = new FloatRange(0.25f, 0.35f).RandomInRange;
                    transition4.AddTrigger(new Trigger_FractionColonyDamageTaken(randomInRange, 900f));
                    transition4.AddPreAction(new TransitionAction_Message("MessageRaidersSatisfiedLeaving".Translate(assaulterFaction.def.pawnsPlural.CapitalizeFirst(), assaulterFaction.Name)));
                    stateGraph.AddTransition(transition4);
                }
                if (canKidnap)
                {
                    LordToil startingToil = stateGraph.AttachSubgraph(new LordJob_BuildingArrivalMode_Kidnap().CreateGraph()).StartingToil;
                    // Changes kidnapping to my own since kidnap makes pawns leave from the border
                    Transition transition5 = new Transition(lordToil3, startingToil);
                    transition5.AddSources(list);
                    transition5.AddPreAction(new TransitionAction_Message("MessageRaidersKidnapping".Translate(assaulterFaction.def.pawnsPlural.CapitalizeFirst(), assaulterFaction.Name)));
                    transition5.AddTrigger(new Trigger_KidnapVictimPresent());
                    stateGraph.AddTransition(transition5);
                }
                if (canSteal)
                {
                    LordToil startingToil2 = stateGraph.AttachSubgraph(new LordJob_BuildingArrivalMode_Steal().CreateGraph()).StartingToil;
                    // Changes stealing to my own since stealing makes pawns leave from the border
                    Transition transition6 = new Transition(lordToil3, startingToil2);
                    transition6.AddSources(list);
                    transition6.AddPreAction(new TransitionAction_Message("MessageRaidersStealing".Translate(assaulterFaction.def.pawnsPlural.CapitalizeFirst(), assaulterFaction.Name)));
                    transition6.AddTrigger(new Trigger_HighValueThingsAround());
                    stateGraph.AddTransition(transition6);
                }
            }
            if (assaulterFaction != null)
            {
                Transition transition7 = new Transition(lordToil3, lordToilExit);
                transition7.AddSources(list);
                transition7.AddTrigger(new Trigger_BecameNonHostileToPlayer());
                transition7.AddPreAction(new TransitionAction_Message("MessageRaidersLeaving".Translate(assaulterFaction.def.pawnsPlural.CapitalizeFirst(), assaulterFaction.Name)));
                stateGraph.AddTransition(transition7);
            }
            LordToil_BuildingArrivalMode_PanicFlee lordToil_PanicFlee = new()
            {
                // The fleeing lord that happens when you defeat a bunch of the raiders changed so they go in the portal
                useAvoidGrid = true
            };

            //The for loop underneath makes all the transitions aim towards the above PanicFlee, same thing vanilla's PanicFlee does
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
