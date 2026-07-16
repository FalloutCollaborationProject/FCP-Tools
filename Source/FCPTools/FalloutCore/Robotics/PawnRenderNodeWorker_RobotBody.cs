using Verse;

namespace FCP.Core.Robotics
{
    public class PawnRenderNodeWorker_RobotBody : PawnRenderNodeWorker_AnimalBody
    {
        protected override Graphic GetGraphic(PawnRenderNode node, PawnDrawParms parms)
        {
            Graphic baseGraphic = base.GetGraphic(node, parms);
            CompColorable colorable = parms.pawn?.GetComp<CompColorable>();
            if (baseGraphic != null && colorable != null && colorable.Active && colorable.Color != baseGraphic.Color)
            {
                return baseGraphic.GetColoredVersion(baseGraphic.Shader, colorable.Color, baseGraphic.ColorTwo);
            }
            return baseGraphic;
        }
    }
}
