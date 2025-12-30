using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

namespace FCP_Shuttles
{
    public class ScenPart_PlayerPawnsArriveInShuttle : ScenPart
    {
        public override void GenerateIntoMap(Map map)
        {
            if (Find.GameInitData == null)
            {
                return;
            }

            var extension = Faction.OfPlayer.def.GetModExtension<FactionModExtension>();

            List<Thing> allThings = new List<Thing>();
            allThings.AddRange(Find.GameInitData.startingAndOptionalPawns);
            foreach (ScenPart part in Find.Scenario.AllParts)
            {
                allThings.AddRange(part.PlayerStartingThings());
            }
            foreach (var thing in allThings)
            {
                if (thing.def.CanHaveFaction)
                {
                    thing.SetFactionDirect(Faction.OfPlayer);
                }
            }
            ShuttleArrivalAction.Arrive(allThings, map, Faction.OfPlayer, extension, MapGenerator.PlayerStartSpot);
        }
    }
}
