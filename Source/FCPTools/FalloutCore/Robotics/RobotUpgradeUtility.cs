using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace FCP.Core.Robotics
{
    public class RobotTierExtension : DefModExtension
    {
        public ResearchProjectDef researchPrerequisite;
        public List<ThingDefCountClass> upgradeCost = new List<ThingDefCountClass>();
    }

    public class RobotUpgradeOption
    {
        public string category;
        public string label;
        public List<ThingDefCountClass> cost = new List<ThingDefCountClass>();
        public string disabledReason;
        public Action install;

        public bool Enabled => disabledReason == null;
    }

    public static class RobotUpgradeUtility
    {
        public static bool CanAffordCost(Map map, List<ThingDefCountClass> cost)
        {
            foreach (ThingDefCountClass entry in cost)
            {
                if (map.resourceCounter.GetCount(entry.thingDef) < entry.count)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool TryConsumeCost(Map map, List<ThingDefCountClass> cost)
        {
            if (!CanAffordCost(map, cost))
            {
                return false;
            }

            foreach (ThingDefCountClass entry in cost)
            {
                int remaining = entry.count;
                foreach (Thing thing in map.listerThings.ThingsOfDef(entry.thingDef).ToList())
                {
                    if (remaining <= 0)
                    {
                        break;
                    }
                    if (!thing.Spawned)
                    {
                        continue;
                    }
                    int consume = Math.Min(remaining, thing.stackCount);
                    thing.SplitOff(consume).Destroy();
                    remaining -= consume;
                }
            }
            return true;
        }
    }
}
