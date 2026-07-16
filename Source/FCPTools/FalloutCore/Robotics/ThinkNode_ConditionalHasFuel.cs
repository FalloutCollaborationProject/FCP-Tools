using Verse;
using Verse.AI;

namespace FCP.Core.Robotics
{
    public class ThinkNode_ConditionalHasFuel : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            CompRefuelable fuel = pawn.GetComp<CompRefuelable>();
            return fuel == null || fuel.HasFuel;
        }
    }
}
