using Verse;
using Verse.AI;

namespace FCP.Core.Robotics
{
    public class ThinkNode_ConditionalShouldMeleeReflex : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (!RobotUtility.IsPoweredOn(pawn))
            {
                return false;
            }

            CompRefuelable fuel = pawn.GetComp<CompRefuelable>();
            if (fuel != null && !fuel.HasFuel)
            {
                return false;
            }

            return !RobotUtility.HasRangedAttack(pawn);
        }
    }
}
