using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using static UnityEngine.GraphicsBuffer;

namespace GauntletSpawners
{
    public class CompGauntletSpawner : ThingComp
    {
        public CompProperties_GauntletSpawner Props => (CompProperties_GauntletSpawner)props;

        public List<Pawn> spawnedPawns = new List<Pawn>();

        public bool IsActive;

        public Thing targetForAttack;

        public List<Thing> groupOfTarget = new List<Thing>();

        public CompRefuelable compRefuelable
        {
            get
            {
                return parent.TryGetComp<CompRefuelable>();
            }
        }
        public bool isHostileInRange = false;
        public override void PostPostMake()
        {
            base.PostPostMake();
            IsActive = true;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref IsActive, "isActive", true);
            Scribe_Collections.Look(ref spawnedPawns, "spawnedPawns", LookMode.Reference);
        }
        public override void CompTick()
        {
            base.CompTick();
            if(parent.IsHashIntervalTick(Props.hostileCheckInterval > 0 ? Props.hostileCheckInterval : Props.interval))
            {
                if(Props.isOnlyActiveWhenHostileNear)
                {
                    if(UtilityCore.NearbyPawnInLineOfSight(parent.PositionHeld,parent.MapHeld,Props.activeRadius,true).EnumerableCount() > 0)
                    {
                        isHostileInRange = true;
                    }
                    else
                    {
                        isHostileInRange = false;
                    }
                    if (!isHostileInRange) return;
                }
            }
            if(parent.IsHashIntervalTick(Props.interval))
            {
                CheckExistingPawns();
                if(spawnedPawns.Count < Props.maxCount)
                {
                    if(IsActive)
                    {
                        DoTrigger();
                    }
                }
            }
            if (parent.IsHashIntervalTick(Props.autoHuntingInterval))
            {
                if (groupOfTarget.Count > 0)
                {
                    CheckMarkedTarget();
                }
                if (groupOfTarget.Count > 0 && spawnedPawns.Count > 0)
                {
                    DoAttack();
                }
            }            
        }
        public void CheckMarkedTarget()
        {
            IReadOnlyList<Thing> list = new List<Thing>(groupOfTarget);
            foreach(var item in list)
            {
                if (item is Pawn pawn)
                {
                    if(pawn.Dead)
                    {
                        groupOfTarget.Remove(pawn);
                    }
                }
                if (item.DestroyedOrNull())
                {
                    if(groupOfTarget.Contains(item))
                    {
                        groupOfTarget.Remove(item);
                    }                    
                }                
            }
        }
        public void DoAttack()
        {
            foreach (var item in spawnedPawns)
            {
                if(item.jobs?.curJob?.def == JobDefOf.AttackMelee)
                {
                    continue;
                }
                Job job_attackMelee = JobMaker.MakeJob(JobDefOf.AttackMelee, groupOfTarget.RandomElement());
                item.jobs.StartJob(job_attackMelee, JobCondition.InterruptForced);
            }
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if(parent.Faction == Faction.OfPlayer || DebugSettings.godMode)
            {
                Command_Toggle command_Toggle = new Command_Toggle();
                if (IsActive)
                {
                    command_Toggle.defaultLabel = "spawn enabled";
                }
                else
                {
                    command_Toggle.defaultLabel = "spawn disabled";
                }
                command_Toggle.defaultDesc = "toggle spawning";
                command_Toggle.icon = ContentFinder<Texture2D>.Get(Props.uiIcon);
                command_Toggle.isActive = () => IsActive;
                command_Toggle.toggleAction = delegate
                {
                    IsActive = !IsActive;
                };
                yield return command_Toggle;

                Command_Action command_goto = new Command_Action();
                command_goto.defaultLabel = "order: goto";
                command_goto.defaultDesc = "order all unit to go to position";
                command_goto.icon = ContentFinder<Texture2D>.Get(Props.gotoIcon);
                command_goto.action = delegate
                {
                    IntVec3 targetPos = new IntVec3();
                    Find.Targeter.BeginTargeting(GetTargetingParameters(), delegate (LocalTargetInfo t)
                    {
                        targetPos = t.Cell;
                        if(!spawnedPawns.NullOrEmpty())
                        {
                            if (Props.effectOnMark != null)
                            {
                                Log.Message("effecter not null");
                                Effecter effecter = Props.effectOnMark.Spawn(targetPos, parent.Map);
                                Log.Message("effecter spawned");
                                effecter.Cleanup();
                            }
                            foreach (var item in spawnedPawns)
                            {
                                Job job = JobMaker.MakeJob(JobDefOf.Goto,targetPos);
                                item.jobs.StartJob(job, JobCondition.InterruptForced);
                            }
                        }
                    });
                };
                yield return command_goto;

                Command_Action command_attackMelee = new Command_Action();
                command_attackMelee.defaultLabel = "order: attack - melee";
                command_attackMelee.defaultDesc = "order all minion to attack target with melee";
                command_attackMelee.icon = ContentFinder<Texture2D>.Get(Props.attackIcon);
                command_attackMelee.action = delegate
                {
                    Thing target = null;
                    Find.Targeter.BeginTargeting(GetTargetingParametersAttack(), delegate (LocalTargetInfo t)
                    {
                        if(t.HasThing && t.Thing != null)
                        {
                            target = t.Thing;
                            targetForAttack = t.Thing;
                            if (!spawnedPawns.NullOrEmpty())
                            {
                                if (Props.effectOnMark != null)
                                {
                                    Effecter effecter = Props.effectOnMark.Spawn(target, parent.Map);
                                    effecter.Cleanup();
                                }
                                foreach (var item in spawnedPawns)
                                {
                                    Job job_attackMelee = JobMaker.MakeJob(JobDefOf.AttackMelee,target);                         
                                    item.jobs.StartJob(job_attackMelee,JobCondition.InterruptForced);
                                }
                            }
                        }
                        else
                        {
                            foreach(var item in GenRadial.RadialDistinctThingsAround(t.Cell,parent.Map,Props.groupMarkingRadius,true))
                            {
                                if(item.Faction.HostileTo(parent.Faction) || item.HostileTo(parent))
                                {
                                    if(Props.effectOnMark != null)
                                    {
                                        Effecter effecter = Props.effectOnMark.Spawn(item,parent.Map);
                                        effecter.Cleanup();
                                    }
                                    groupOfTarget.Add(item);
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            foreach (var item in spawnedPawns)
                            {
                                Job job_attackMelee = JobMaker.MakeJob(JobDefOf.AttackMelee, groupOfTarget.RandomElement());
                                item.jobs.StartJob(job_attackMelee, JobCondition.InterruptForced);
                            }
                        }
                    });
                };
                yield return command_attackMelee;

                Command_Action command_attackRanged = new Command_Action();
                command_attackRanged.defaultLabel = "order: attack - ranged";
                command_attackRanged.defaultDesc = "order all minion to attack target with ranged attack from it position";
                command_attackRanged.icon = ContentFinder<Texture2D>.Get(Props.attackIcon);
                command_attackRanged.action = delegate
                {
                    Thing target = null;
                    Find.Targeter.BeginTargeting(GetTargetingParametersAttack(), delegate (LocalTargetInfo t)
                    {
                        if (t.HasThing && t.Thing != null)
                        {
                            target = t.Thing;
                            targetForAttack = t.Thing;
                            if (!spawnedPawns.NullOrEmpty())
                            {
                                if (Props.effectOnMark != null)
                                {
                                    Effecter effecter = Props.effectOnMark.Spawn(t.Cell, parent.Map);
                                    effecter.Cleanup();
                                }
                                foreach (var item in spawnedPawns)
                                {
                                    Job job_attackRanged = JobMaker.MakeJob(JobDefOf.AttackStatic, target);                                    
                                    item.jobs.StartJob(job_attackRanged, JobCondition.InterruptForced);
                                }
                            }
                        }
                    });
                };
                yield return command_attackRanged;
            }            
        }
        public TargetingParameters GetTargetingParametersAttack()
        {
            return new TargetingParameters
            {
                canTargetPawns = true,
                canTargetAnimals = true,
                canTargetBuildings = true,
                canTargetItems = true,
                canTargetLocations = true,
                mapObjectTargetsMustBeAutoAttackable = true,
                validator = (TargetInfo x) => !(x.Cell.Fogged(parent.Map))
            };
        }
        public TargetingParameters GetTargetingParameters()
        {
            return new TargetingParameters
            {
                canTargetPawns = false,
                canTargetAnimals = false,
                canTargetBuildings = false,
                canTargetItems = false,
                canTargetLocations = true,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = (TargetInfo x) => !(x.Cell.Impassable(parent.MapHeld))
            };
        }
        public void CheckExistingPawns()
        {
            if (spawnedPawns.NullOrEmpty())
            {
                return;
            }
            IReadOnlyList<Pawn> tempList = spawnedPawns.ToList();
            foreach(var item in tempList)
            {
                if(item.DestroyedOrNull())
                {
                    spawnedPawns.Remove(item);
                }
                else if(item.Dead)
                {
                    spawnedPawns.Remove(item);
                }
                else
                {
                    continue;
                }
            }
        }
        public void DoTrigger()
        {
            if(compRefuelable != null)
            {
                if(compRefuelable.HasFuel && compRefuelable.Fuel - Props.fuelUsed >= 0)
                {
                    if (spawnedPawns.Count < Props.maxCount)
                    {
                        for (int i = 0; i < Props.spawnPerTrigger; i++)
                        {
                            Pawn newPawn = PawnGenerator.GeneratePawn(Props.possibleKindDefToSpawn.RandomElement(), parent.Faction);
                            newPawn.mindState.mentalStateHandler.TryStartMentalState(Props.mentalStateDef,forced:true,forceWake:true);
                            if (newPawn.Faction != parent.Faction)
                            {
                                newPawn.SetFaction(parent.Faction);
                            }
                            if (Props.spawnAsDroppod)
                            {
                                SpawnDropPod(parent.MapHeld,newPawn, parent.RandomAdjacentCell8Way());
                            }
                            else
                            {
                                GenSpawn.Spawn(newPawn, parent.RandomAdjacentCell8Way(), parent.MapHeld);
                            }
                            spawnedPawns.Add(newPawn);
                            if (spawnedPawns.Count >= Props.maxCount)
                            {
                                break;
                            }
                        }
                        compRefuelable.ConsumeFuel(Props.fuelUsed);
                    }
                }
            }
            else
            {
                if (spawnedPawns.Count < Props.maxCount)
                {
                    for (int i = 0; i < Props.spawnPerTrigger; i++)
                    {
                        Pawn newPawn = PawnGenerator.GeneratePawn(Props.possibleKindDefToSpawn.RandomElement(), parent.Faction);
                         newPawn.mindState.mentalStateHandler.TryStartMentalState(Props.mentalStateDef,forced:true,forceWake:true);
                        if (newPawn.Faction != parent.Faction)
                        {
                            newPawn.SetFaction(parent.Faction);
                        }
                        if (Props.spawnAsDroppod)
                        {
                            SpawnDropPod(parent.MapHeld, newPawn, parent.RandomAdjacentCell8Way());
                        }
                        else
                        {
                            GenSpawn.Spawn(newPawn, parent.RandomAdjacentCell8Way(), parent.MapHeld);
                        }
                        spawnedPawns.Add(newPawn);
                        if (spawnedPawns.Count >= Props.maxCount)
                        {
                            break;
                        }
                    }
                }
            }                       
        }
        public void SpawnDropPod(Map map, Thing t, IntVec3 dropPos)
        {
            Thing newThing = t;
            if (newThing.def.CanHaveFaction)
            {
                newThing.SetFaction(parent.Faction);
            }
            ActiveTransporterInfo activeDropPodInfo = new ActiveTransporterInfo();
            activeDropPodInfo.SingleContainedThing = newThing;
            activeDropPodInfo.leaveSlag = false;
            DropPodUtility.MakeDropPodAt(dropPos, map, activeDropPodInfo);
        }
    }
}
