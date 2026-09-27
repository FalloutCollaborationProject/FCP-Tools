using System.Collections.Generic;
using RimWorld;
using Verse;

namespace FCP.Core.Robotics
{
    public class CompProperties_RobotAreaRestriction : CompProperties
    {
        public CompProperties_RobotAreaRestriction()
        {
            compClass = typeof(CompRobotAreaRestriction);
        }
    }

    public class CompRobotAreaRestriction : ThingComp
    {
        public Area RestrictedArea;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref RestrictedArea, "robotRestrictedArea");
        }

        public override string CompInspectStringExtra()
        {
            return RestrictedArea != null ? "FCP_RobotAreaRestriction_Active".Translate(RestrictedArea.Label) : null;
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
                defaultLabel = "FCP_RobotAreaRestriction_Gizmo".Translate(RestrictedArea?.Label ?? "FCP_RobotAreaRestriction_None".Translate()),
                defaultDesc = "FCP_RobotAreaRestriction_GizmoDesc".Translate(),
                icon = TexButton.Info,
                action = () => OpenAreaMenu(robot),
            };
        }

        private void OpenAreaMenu(Pawn robot)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>
            {
                new FloatMenuOption("FCP_RobotAreaRestriction_None".Translate(), () => RestrictedArea = null),
            };

            foreach (Area area in robot.Map.areaManager.AllAreas)
            {
                Area capturedArea = area;
                options.Add(new FloatMenuOption(capturedArea.Label, () => RestrictedArea = capturedArea));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }
    }
}
