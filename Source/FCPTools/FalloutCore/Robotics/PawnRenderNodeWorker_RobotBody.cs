using System.Linq;
using Verse;

namespace FCP.Core.Robotics
{
    public class PawnRenderNodeWorker_RobotBody : PawnRenderNodeWorker_AnimalBody
    {
        protected override Graphic GetGraphic(PawnRenderNode node, PawnDrawParms parms)
        {
            Graphic baseGraphic = base.GetGraphic(node, parms);

            Hediff overrideHediff = parms.pawn?.health?.hediffSet?.hediffs.FirstOrDefault(h =>
                h.Part == null && h.def.GetModExtension<RobotHediffGraphic>() is RobotHediffGraphic rhg && rhg.isBodyOverride);
            if (overrideHediff != null)
            {
                Graphic overrideGraphic = RobotHediffGraphicCache.GetFor(overrideHediff.def);
                if (overrideGraphic != null)
                {
                    baseGraphic = overrideGraphic;
                }
            }

            CompColorable colorable = parms.pawn?.GetComp<CompColorable>();
            if (baseGraphic != null && colorable != null && colorable.Active && colorable.Color != baseGraphic.Color)
            {
                return baseGraphic.GetColoredVersion(baseGraphic.Shader, colorable.Color, baseGraphic.ColorTwo);
            }
            return baseGraphic;
        }
    }
}
