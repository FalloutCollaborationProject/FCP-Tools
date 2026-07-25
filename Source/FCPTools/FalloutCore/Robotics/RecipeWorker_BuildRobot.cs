using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace FCP.Core.Robotics
{
    public class RobotBuildExtension : DefModExtension
    {
        public PawnKindDef kindDef;

        // Only meaningful for robots with a CompProtectronLoadout (e.g. protectrons); ignored otherwise.
        public string presetId;
    }

    public class RecipeWorker_BuildRobot : RecipeWorker
    {
        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            RobotBuildExtension ext = recipe.GetModExtension<RobotBuildExtension>();
            if (ext?.kindDef == null)
            {
                return;
            }

            Thing bench = billDoer.CurJob.GetTarget(TargetIndex.A).Thing;
            IntVec3 spawnCell = bench?.InteractionCell ?? billDoer.Position;

            PawnGenerationRequest request = new PawnGenerationRequest(ext.kindDef, Faction.OfPlayer);
            Pawn robot = PawnGenerator.GeneratePawn(request);
            if (!ext.presetId.NullOrEmpty())
            {
                robot.GetComp<CompProtectronLoadout>()?.ApplyPreset(ext.presetId);
            }
            GenSpawn.Spawn(robot, spawnCell, billDoer.Map);

            CompRefuelable fuel = robot.GetComp<CompRefuelable>();
            if (fuel != null && fuel.Fuel > 0f)
            {
                fuel.ConsumeFuel(fuel.Fuel);
            }

            billDoer.skills.Learn(SkillDefOf.Crafting, 1f);
            Messages.Message("FCP_BuildRobot_Done".Translate(billDoer.LabelShort, robot.LabelShort), robot, MessageTypeDefOf.PositiveEvent, historical: false);
        }
    }
}
