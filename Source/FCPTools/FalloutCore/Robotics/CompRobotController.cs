using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace FCP.Core.Robotics
{
    public class CompProperties_RobotController : CompProperties
    {
        public CompProperties_RobotController()
        {
            compClass = typeof(CompRobotController);
        }
    }

    public class CompRobotController : ThingComp
    {
        public CompProperties_RobotController Props => (CompProperties_RobotController)props;

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (!selPawn.CanReach(parent, PathEndMode.InteractionCell, Danger.Deadly))
            {
                yield return new FloatMenuOption("FCP_ControlRobots_Gizmo".Translate() + ": " + "NoPath".Translate(), null);
                yield break;
            }

            yield return new FloatMenuOption("FCP_ControlRobots_Gizmo".Translate(), delegate
            {
                Job job = JobMaker.MakeJob(JobDefOf_Robotics.FCP_ControlRobots, parent);
                selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            });
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
                defaultLabel = "FCP_ControlRobots_Gizmo".Translate(),
                defaultDesc = "FCP_ControlRobots_GizmoDesc".Translate(),
                icon = TexButton.Info,
                action = OpenRobotSelectMenu,
            };
        }

        public void OpenRobotSelectMenu()
        {
            Find.WindowStack.Add(new Dialog_RobotControl(parent.Map));
        }
    }
}
