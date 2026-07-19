using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace FCP.Core.Robotics
{
    public class JobGiver_EyebotMode : JobGiver_AIFightEnemy
    {
        private const int ExploreScanIntervalTicks = 2500;
        private const int ExploreScanRadius = 8;
        private const int MusicJoyIntervalTicks = 2000;
        private const float MusicJoyRadius = 12f;
        private const float MusicJoyAmount = 0.03f;

        protected override Job TryGiveJob(Pawn pawn)
        {
            CompRobotUpgrade upgradeComp = pawn.GetComp<CompRobotUpgrade>();
            if (upgradeComp?.PendingBench != null)
            {
                if (!upgradeComp.PendingBench.Spawned)
                {
                    upgradeComp.PendingBench = null;
                }
                else
                {
                    return JobMaker.MakeJob(JobDefOf_Robotics.FCP_RobotDock, upgradeComp.PendingBench);
                }
            }

            CompRefuelable fuel = pawn.GetComp<CompRefuelable>();
            if (fuel != null && !fuel.HasFuel)
            {
                return null;
            }

            if (!RobotUtility.IsPoweredOn(pawn))
            {
                return null;
            }

            if (pawn.Faction == null)
            {
                return null;
            }

            CompEyebotMode modeComp = pawn.GetComp<CompEyebotMode>();
            if (modeComp == null)
            {
                return null;
            }

            return modeComp.Mode switch
            {
                EyebotMode.Defend => base.TryGiveJob(pawn),
                EyebotMode.Scavenge => TryScavenge(pawn),
                EyebotMode.Explore => TryExplore(pawn),
                EyebotMode.Music => TryMusic(pawn),
                _ => null,
            };
        }

        protected override bool ExtraTargetValidator(Pawn pawn, Thing target)
        {
            if (!base.ExtraTargetValidator(pawn, target))
            {
                return false;
            }
            return pawn.Map.areaManager.Home[target.Position];
        }

        protected override bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest, Verb verbToUse = null)
        {
            Thing enemyTarget = pawn.mindState.enemyTarget;
            Verb verb = verbToUse ?? pawn.TryGetAttackVerb(enemyTarget, !pawn.IsColonist);
            if (verb == null)
            {
                dest = IntVec3.Invalid;
                return false;
            }

            return CastPositionFinder.TryFindCastPosition(new CastPositionRequest
            {
                caster = pawn,
                target = enemyTarget,
                verb = verb,
                maxRangeFromTarget = 9999f,
                locus = pawn.Position,
                maxRangeFromLocus = 9999f,
                wantCoverFromTarget = verb.EffectiveRange > 7f,
            }, out dest);
        }

        private static Job TryScavenge(Pawn pawn)
        {
            WorkGiver_HaulGeneral haulGiver = (WorkGiver_HaulGeneral)DefDatabase<WorkGiverDef>.GetNamed("HaulGeneral").Worker;
            Thing haulable = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), PathEndMode.ClosestTouch,
                TraverseParms.For(pawn), 9999f,
                thing => haulGiver.HasJobOnThing(pawn, thing));

            return haulable == null ? null : haulGiver.JobOnThing(pawn, haulable);
        }

        private static Job TryExplore(Pawn pawn)
        {
            CompEyebotExplorer explorer = pawn.GetComp<CompEyebotExplorer>();
            if (explorer != null && Find.TickManager.TicksGame >= explorer.NextScanTick)
            {
                explorer.NextScanTick = Find.TickManager.TicksGame + ExploreScanIntervalTicks;
                explorer.ScanForSites(ExploreScanRadius);
            }

            IntVec3 wanderRoot = pawn.Map.IsPlayerHome ? pawn.Map.Center : pawn.Position;
            IntVec3 wanderCell = RCellFinder.RandomWanderDestFor(pawn, wanderRoot, 10f, null, Danger.None);
            if (!wanderCell.IsValid)
            {
                return null;
            }

            return JobMaker.MakeJob(JobDefOf.GotoWander, wanderCell);
        }

        private static Job TryMusic(Pawn pawn)
        {
            CompEyebotMusicPlayer musicPlayer = pawn.GetComp<CompEyebotMusicPlayer>();
            if (musicPlayer?.LoadedTapeDef != null && Find.TickManager.TicksGame >= musicPlayer.NextJoyTick)
            {
                musicPlayer.NextJoyTick = Find.TickManager.TicksGame + MusicJoyIntervalTicks;
                foreach (Pawn nearby in GenRadial.RadialDistinctThingsAround(pawn.Position, pawn.Map, MusicJoyRadius, true).OfType<Pawn>())
                {
                    if (nearby.Faction == Faction.OfPlayer && nearby.needs?.joy != null)
                    {
                        nearby.needs.joy.GainJoy(MusicJoyAmount, JoyKindDefOf.Social);
                    }
                }
            }

            IntVec3 wanderRoot = pawn.Map.IsPlayerHome ? pawn.Map.Center : pawn.Position;
            IntVec3 wanderCell = RCellFinder.RandomWanderDestFor(pawn, wanderRoot, 10f, null, Danger.None);
            if (!wanderCell.IsValid)
            {
                return null;
            }

            return JobMaker.MakeJob(JobDefOf.GotoWander, wanderCell);
        }
    }
}
