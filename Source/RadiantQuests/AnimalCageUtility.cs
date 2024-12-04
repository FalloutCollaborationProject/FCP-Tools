using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;
using RimWorld;
using UnityEngine;

namespace FCP_RadiantQuests
{
    public static class AnimalCageUtility
    {
        public static void TargetCageForAnimal(Pawn carrier, Thing animal, bool transferBetweenCages = false, Thing sourceCage = null)
        {
            Find.Targeter.BeginTargeting(TargetingParameters.ForBuilding(), delegate (LocalTargetInfo t)
            {
                if (carrier != null && !CanReserveForTransfer(t))
                {
                    Messages.Message("MessageHolderReserved".Translate(t.Thing.Label), MessageTypeDefOf.RejectInput);
                }
                else
                {
                    foreach (Thing item in Find.CurrentMap.listerBuildings.allBuildingsColonist.Where(c=> c.HasComp<CompAnimalCage>()))
                    {
                        CompAnimalCage compAnimalCage;
                        if (!item.TryGetComp(out compAnimalCage) && animal != compAnimalCage.Occupant)
                        {
                            Messages.Message("MessageHolderReserved".Translate(t.Thing.Label), MessageTypeDefOf.RejectInput);
                            return;
                        }
                    }
                    if (carrier != null)
                    {
                        Log.Message("Issuing job");
                        Job job = (transferBetweenCages ? JobMaker.MakeJob(DefOfs.FCP_TransferAnimalBetweenCages, sourceCage, t, animal) : JobMaker.MakeJob(DefOfs.FCP_CarryAnimalToCage, t, animal));
                        job.count = 1;
                        carrier.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    }
                }
            }, delegate (LocalTargetInfo t)
            {
                if (ValidateTarget(t))
                {
                    GenDraw.DrawTargetHighlight(t);
                }
            }, ValidateTarget, null, null, BaseContent.ClearTex, playSoundOnAction: true, delegate (LocalTargetInfo t)
            {
                CompAnimalCage compAnimalCage = t.Thing?.TryGetComp<CompAnimalCage>();
                if (compAnimalCage == null)
                {
                    TaggedString label = "FCP_ChooseAnimalCage".Translate().CapitalizeFirst() + "...";
                    Widgets.MouseAttachedLabel(label);
                }
                else
                {
                    TaggedString label = "FCP_SelectThisCage".Translate();

                    Widgets.MouseAttachedLabel(label);
                }
            }, delegate
            {
                foreach (Building item2 in animal.MapHeld.listerBuildings.allBuildingsColonist.Where(c=> c.HasComp<CompAnimalCage>()))
                {
                    if (ValidateTarget(item2) && (carrier == null || CanReserveForTransfer(item2)))
                    {
                        GenDraw.DrawArrowPointingAt(item2.DrawPos);
                    }
                }
            });
            bool CanReserveForTransfer(LocalTargetInfo t)
            {
                if (transferBetweenCages)
                {
                    if (t.HasThing)
                    {
                        return carrier.CanReserve(t.Thing);
                    }
                    return false;
                }
                return true;
            }
            bool ValidateTarget(LocalTargetInfo t)
            {
                if (t.HasThing && t.Thing.TryGetComp(out CompAnimalCage comp) && comp.Occupant == null && comp.Props.minBodySize <= animal.def.race.baseBodySize && comp.Props.maxBodySize >= animal.def.race.baseBodySize && carrier != null)
                {
                    return carrier.CanReserveAndReach(t.Thing, PathEndMode.Touch, Danger.Some);
                }
                return false;
            }
        }

        public static bool TryGetCagesThatFitBodySize(float bodySize, Map map, out List<CompAnimalCage> cages)
        {
            Log.Message(bodySize);
            if(bodySize == 0 || map == null)
            {
                cages = null;
                return false;
            }
            if (!map.listerBuildings.allBuildingsColonist.Any(c => c.HasComp<CompAnimalCage>()))
            {
                cages = null;
                return false;
            }
            cages = new List<CompAnimalCage>();
            foreach (Building cage in map.listerBuildings.allBuildingsColonist.Where(c => c.HasComp<CompAnimalCage>()))
            {
                CompAnimalCage comp = cage.GetComp<CompAnimalCage>();
                if(bodySize <= comp.Props.maxBodySize)
                {
                    cages.Add(comp);
                }

            }
            return true;
        }
        public static Job RefuelJob(Pawn pawn, Thing t, bool forced = false, JobDef customRefuelJob = null, JobDef customAtomicRefuelJob = null)
        {
            if (!t.TryGetComp<CompAnimalCage>().Props.atomicFueling)
            {
                Thing thing = FindBestFuel(pawn, t);
                return JobMaker.MakeJob(customRefuelJob ?? JobDefOf.Refuel, t, thing);
            }
            List<Thing> source = FindAllFuel(pawn, t);
            Job job = JobMaker.MakeJob(customAtomicRefuelJob ?? JobDefOf.RefuelAtomic, t);
            job.targetQueueB = source.Select((Thing f) => new LocalTargetInfo(f)).ToList();
            return job;
        }
        public static bool CanRefuel(Pawn pawn, Thing t, bool forced = false)
        {
            CompAnimalCage compAnimalCage = t.TryGetComp<CompAnimalCage>();
            if (compAnimalCage == null || compAnimalCage.IsFull || (!forced && !compAnimalCage.allowAutoRefuel))
            {
                return false;
            }
            if (compAnimalCage.FuelPercentOfMax > 0f && !compAnimalCage.Props.allowRefuelIfNotEmpty)
            {
                return false;
            }
            if (!forced && !compAnimalCage.ShouldAutoRefuelNow)
            {
                return false;
            }
            if (t.IsForbidden(pawn) || !pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }
            if (t.Faction != pawn.Faction)
            {
                return false;
            }
            if (FindBestFuel(pawn, t) == null)
            {
                ThingFilter fuelFilter = t.TryGetComp<CompAnimalCage>().FuelFilter;
                JobFailReason.Is("NoFuelToRefuel".Translate(fuelFilter.Summary));
                return false;
            }
            if (t.TryGetComp<CompAnimalCage>().Props.atomicFueling && FindAllFuel(pawn, t) == null)
            {
                ThingFilter fuelFilter2 = t.TryGetComp<CompAnimalCage>().FuelFilter;
                JobFailReason.Is("NoFuelToRefuel".Translate(fuelFilter2.Summary));
                return false;
            }
            return true;
        }
        private static Thing FindBestFuel(Pawn pawn, Thing refuelable)
        {
            ThingFilter filter = refuelable.TryGetComp<CompAnimalCage>().FuelFilter;
            Predicate<Thing> validator = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x) && (filter.Allows(x) ? true : false);
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, filter.BestThingRequest, PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, validator);
        }
        private static List<Thing> FindAllFuel(Pawn pawn, Thing refuelable)
        {
            int fuelCountToFullyRefuel = refuelable.TryGetComp<CompAnimalCage>().GetFuelCountToFullyRefuel();
            ThingFilter filter = refuelable.TryGetComp<CompAnimalCage>().FuelFilter;
            return FindEnoughReservableThings(pawn, refuelable.Position, new IntRange(fuelCountToFullyRefuel, fuelCountToFullyRefuel), (Thing t) => filter.Allows(t));
        }
        public static List<Thing> FindEnoughReservableThings(Pawn pawn, IntVec3 rootCell, IntRange desiredQuantity, Predicate<Thing> validThing)
        {
            Predicate<Thing> validator = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x) && (validThing(x) ? true : false);
            Region region2 = rootCell.GetRegion(pawn.Map);
            TraverseParms traverseParams = TraverseParms.For(pawn);
            RegionEntryPredicate entryCondition = (Region from, Region r) => r.Allows(traverseParams, isDestination: false);
            List<Thing> chosenThings = new List<Thing>();
            int accumulatedQuantity = 0;
            ThingListProcessor(rootCell.GetThingList(region2.Map), region2);
            if (accumulatedQuantity < desiredQuantity.max)
            {
                RegionTraverser.BreadthFirstTraverse(region2, entryCondition, RegionProcessor, 99999);
            }
            if (accumulatedQuantity >= desiredQuantity.min)
            {
                return chosenThings;
            }
            return null;
            bool RegionProcessor(Region r)
            {
                List<Thing> things2 = r.ListerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.HaulableEver));
                return ThingListProcessor(things2, r);
            }
            bool ThingListProcessor(List<Thing> things, Region region)
            {
                for (int i = 0; i < things.Count; i++)
                {
                    Thing thing = things[i];
                    if (validator(thing) && !chosenThings.Contains(thing) && ReachabilityWithinRegion.ThingFromRegionListerReachable(thing, region, PathEndMode.ClosestTouch, pawn))
                    {
                        chosenThings.Add(thing);
                        accumulatedQuantity += thing.stackCount;
                        if (accumulatedQuantity >= desiredQuantity.max)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }
    }
}
