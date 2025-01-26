using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace FCP_CaravanIncidents
{
    [StaticConstructorOnStartup]
    public static class IncidentUtility
    {
        public static readonly TraderKindDef scavTrader = DefDatabase<TraderKindDef>.GetNamed("FCP_Scavenger_Trader");
        public static Quest GenerateCaravanQuest(QuestScriptDef root, float points, Caravan caravan)
        {
            Slate slate = new Slate();
            slate.Set("points", points);
            slate.Set("caravan", caravan);
            return QuestUtility.GenerateQuestAndMakeAvailable(root, slate);
        }
        public static (int totalWeight, int[] cumulativeWeights) CumulativeWeights(List<PassengerPawnkindChance> list)
        {
            int totalWeight = 0;
            int[] cumulativeWeightsPassengers = new int[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                totalWeight += list[i].weightedChance;
                cumulativeWeightsPassengers[i] = totalWeight;
            }
            return (totalWeight, cumulativeWeightsPassengers);
        }
        public static (int totalWeight, int[] cumulativeWeights) CumulativeWeights(List<Loot> list)
        {
            int totalWeight = 0;
            int[] cumulativeWeightsPassengers = new int[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                totalWeight += list[i].weightedChance;
                cumulativeWeightsPassengers[i] = totalWeight;
            }
            return (totalWeight, cumulativeWeightsPassengers);
        }
    }
}
