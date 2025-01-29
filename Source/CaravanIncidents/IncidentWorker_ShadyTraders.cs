using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using static System.Collections.Specialized.BitVector32;

namespace FCP_CaravanIncidents
{
    public class IncidentWorker_ShadyTraders : IncidentWorker
    {
        List<Pawn> traderPawns;
        List<Pawn> ambushPawns;
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Log.Message(CaravanIncidentUtility.CanFireIncidentWhichWantsToGenerateMapAt(parms.target.Tile));
            if (!CaravanIncidentUtility.CanFireIncidentWhichWantsToGenerateMapAt(parms.target.Tile))
            {
                return false;
            }
            if (!CaravanIncidents_Settings.enableShadyTraders) // Shady traders
            {
                return false;
            }
            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Caravan caravan = (Caravan)parms.target;
            CameraJumper.TryJumpAndSelect(caravan);
            bool doAmbush = Rand.Chance(CaravanIncidents_Settings.shadyTradersAmbushChance);
            IncidentDefExtension ext = def.GetModExtension<IncidentDefExtension>();
            traderPawns = GenerateCaravanPawns(ext.traderKindDef, parms.points);
            ambushPawns = GenerateAmbushPawns(ext.factionDef, ext.pawnGroupKindDef, parms.points);

            Caravan metCaravan = CaravanMaker.MakeCaravan(traderPawns, traderPawns[0].Faction, -1, addToWorldPawnsIfNotAlready: false);
            Pawn bestPlayerNegotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan, traderPawns[0].Faction, metCaravan.TraderKind);
            CameraJumper.TryJumpAndSelect(caravan);
            DiaNode diaNode = new DiaNode("FCP_Incidents_ShadyTradersDesc".Translate());
            DiaOption diaOption = new DiaOption("CaravanMeeting_Trade".Translate());
            diaOption.action = delegate
            {
                Log.Message("Trading");
                Find.WindowStack.Add(new Dialog_TradePostAction(bestPlayerNegotiator, metCaravan, postCloseAction: delegate
                {
                    if (doAmbush)
                    {
                        DoAmbush(caravan, metCaravan, ext);
                    }
                }));
                PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(metCaravan.Goods.OfType<Pawn>(), "LetterRelatedPawnsTradingWithOtherCaravan".Translate(Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent);
            };
            diaOption.resolveTree = true;
            diaNode.options.Add(diaOption);
            DiaOption diaOption1 = new DiaOption("CaravanMeeting_Attack".Translate());
            diaOption1.action = delegate
            {
                DoAttack(caravan, metCaravan, ext);
                                
            };
            diaOption1.resolveTree = true;
            diaNode.options.Add(diaOption1);
            DiaOption diaOption2 = new DiaOption("CaravanMeeting_MoveOn".Translate());
            diaOption2.action = delegate
            {
                if (doAmbush)
                {
                    DoAmbush(caravan, metCaravan, ext);
                }
            };
            diaOption2.resolveTree = true;
            diaNode.options.Add(diaOption2);
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, "FCP_Incidents_ShadyTradersLabel".Translate()));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, "FCP_Incidents_ShadyTradersLabel".Translate()));
            return true;
        }

        private void DoAttack(Caravan caravan, Caravan metCaravan, IncidentDefExtension ext)
        {
            DiaNode diaNode = new DiaNode("FCP_Incidents_ShadyTradersAttackDesc".Translate());
            DiaOption diaOption = new DiaOption("OK".Translate());
            diaOption.action = delegate
            {
                GenerateCombat(caravan, metCaravan, ext);
            };
            diaOption.resolveTree = true;
            diaNode.options.Add(diaOption);
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, "FCP_Incidents_ShadyTradersAmbushLabel".Translate()));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, "FCP_Incidents_ShadyTradersAmbushLabel".Translate()));
        }
        private void DoAmbush(Caravan caravan, Caravan metCaravan, IncidentDefExtension ext)
        {
            DiaNode diaNode = new DiaNode("FCP_Incidents_ShadyTradersAmbushDesc".Translate());
            DiaOption diaOption = new DiaOption("OK".Translate());
            diaOption.action = delegate
            {
                GenerateCombat(caravan,metCaravan,ext);
            };
            diaOption.resolveTree = true;
            diaNode.options.Add(diaOption);
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, "FCP_Incidents_ShadyTradersAmbushLabel".Translate()));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, "FCP_Incidents_ShadyTradersAmbushLabel".Translate()));


        }

        private void GenerateCombat(Caravan caravan, Caravan metCaravan, IncidentDefExtension ext)
        {
            Faction ambushFaction = Find.FactionManager.FirstFactionOfDef(ext.factionDef);

            LongEventHandler.QueueLongEvent(delegate
            {
                Pawn pawn = caravan.PawnsListForReading[0];
                Map map = CaravanIncidentUtility.GetOrGenerateMapForIncident(caravan, new IntVec3(100, 1, 100), WorldObjectDefOf.Ambush);
                map.Parent.SetFaction(ambushFaction);
                MultipleCaravansCellFinder.FindStartingCellsFor2Groups(map, out var playerSpot, out var enemySpot);
                foreach (Pawn p in metCaravan.pawns)
                {

                    p.SetFaction(ambushFaction);
                }
                CaravanEnterMapUtility.Enter(caravan, map, (Pawn p) => CellFinder.RandomClosewalkCellNear(playerSpot, map, 12), CaravanDropInventoryMode.DoNotDrop, draftColonists: true);
                List<Pawn> list2 = metCaravan.PawnsListForReading.ToList();
                CaravanEnterMapUtility.Enter(metCaravan, map, (Pawn p) => CellFinder.RandomClosewalkCellNear(enemySpot, map, 12));
                LordMaker.MakeNewLord(ambushFaction, new LordJob_DefendAttackedTraderCaravan(list2[0].Position), map, list2);
                IntVec3 cell = FindEdgeCellWalkableToCell(map, playerSpot);
                foreach (Pawn ambusher in ambushPawns)
                {
                    if (!RCellFinder.TryFindRandomCellNearWith(cell, (IntVec3 c) => c.Standable(map), map, out var result, 10))
                    {
                        break;
                    }
                    GenSpawn.Spawn(ambusher, result, map);
                }
                LordMaker.MakeNewLord(ambushFaction, new LordJob_AssaultColony(ambushFaction, true, true), map, ambushPawns);

                Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
                CameraJumper.TryJumpAndSelect(pawn);
                PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(list2, "LetterRelatedPawnsGroupGeneric".Translate(Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent, informEvenIfSeenBefore: true);
            }, "GeneratingMapForNewEncounter", doAsynchronously: false, null);
        }


        private IntVec3 FindEdgeCellWalkableToCell(Map map, IntVec3 cell)
        {
            IntVec3 result = IntVec3.Invalid;
            TraverseParms traverseParams = TraverseParms.For(TraverseMode.NoPassClosedDoors).WithFenceblocked(forceFenceblocked: true);
            for(int i = 0; i < 25; i++)
            {
                if (CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => x.Standable(map) && !x.Fogged(map) && map.reachability.CanReach(x, cell, PathEndMode.OnCell, traverseParams), map, CellFinder.EdgeRoadChance_Neutral, out result))
                {
                    break;
                }
            }
            if(result == IntVec3.Invalid)
            {
                result = CellFinder.RandomEdgeCell(map);
            }
            return result;

        }
        private List<Pawn> GenerateAmbushPawns(FactionDef factionDef, PawnGroupKindDef pawnGroupKindDef, float points = 1f)
        {
            Faction ambushFaction = Find.FactionManager.FirstFactionOfDef(factionDef);
            return PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
            {
                groupKind = pawnGroupKindDef,
                faction = ambushFaction,
                points = points,
                dontUseSingleUseRocketLaunchers = true
            }).ToList();
        }
        private List<Pawn> GenerateCaravanPawns(TraderKindDef traderKind, float points = 1f)
        {
            Faction faction = Find.FactionManager.FirstFactionOfDef(DefDatabase<FactionDef>.GetNamed("FCP_Faction_Hidden_Wastelanders"));//Find.FactionManager.FirstFactionOfDef(factionDef);

            return PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
            {
                //traderKind = traderKind,
                groupKind = PawnGroupKindDefOf.Trader,
                faction = faction,
                points = points,
                dontUseSingleUseRocketLaunchers = true
            }).ToList();
        }


    }
}
