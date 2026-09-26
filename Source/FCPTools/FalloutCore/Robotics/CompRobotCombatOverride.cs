using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace FCP.Core.Robotics
{
    public class CompProperties_RobotCombatOverride : CompProperties
    {
        public CompProperties_RobotCombatOverride()
        {
            compClass = typeof(CompRobotCombatOverride);
        }
    }

    public class CompRobotCombatOverride : ThingComp
    {
        public Thing ForcedTarget;
        public bool FallBack;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref ForcedTarget, "combatOverrideForcedTarget");
            Scribe_Values.Look(ref FallBack, "combatOverrideFallBack");
        }

        public bool HasValidForcedTarget => parent is Pawn pawn && RobotUtility.IsValidForcedTarget(pawn, ForcedTarget);

        public void ClearForcedTarget()
        {
            ForcedTarget = null;
        }

        public override string CompInspectStringExtra()
        {
            if (FallBack)
            {
                return "FCP_RobotFallBack_Active".Translate();
            }
            if (HasValidForcedTarget)
            {
                return "FCP_RobotFocusTarget_Active".Translate(ForcedTarget.LabelShort);
            }
            return null;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            if (!(parent is Pawn robot) || robot.Faction != Faction.OfPlayer)
            {
                yield break;
            }

            yield return new Command_Action
            {
                defaultLabel = "FCP_RobotFocusTarget_Gizmo".Translate(),
                defaultDesc = "FCP_RobotFocusTarget_GizmoDesc".Translate(),
                icon = TexCommand.Attack,
                action = () => BeginFocusTargeting(robot),
            };

            yield return new Command_Toggle
            {
                defaultLabel = "FCP_RobotFallBack_Gizmo".Translate(),
                defaultDesc = "FCP_RobotFallBack_GizmoDesc".Translate(),
                icon = TexButton.Info,
                isActive = () => FallBack,
                toggleAction = () =>
                {
                    FallBack = !FallBack;
                    if (FallBack)
                    {
                        ClearForcedTarget();
                    }
                },
            };
        }

        private void BeginFocusTargeting(Pawn robot)
        {
            TargetingParameters targetParams = new TargetingParameters
            {
                canTargetPawns = true,
                canTargetBuildings = true,
                canTargetLocations = false,
                canTargetSelf = false,
                validator = target => target.Thing != null && robot.HostileTo(target.Thing),
            };

            Find.Targeter.BeginTargeting(targetParams, target =>
            {
                ForcedTarget = target.Thing;
                FallBack = false;
            });
        }
    }
}
