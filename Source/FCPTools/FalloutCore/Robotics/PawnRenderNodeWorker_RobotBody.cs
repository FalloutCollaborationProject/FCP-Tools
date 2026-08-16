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
                // GetColoredVersion can create a new cached graphic under the hood, which touches the
                // Unity API and isn't safe from the parallel pawn-render threads introduced in 1.6.
                // Off the main thread we fall back to the uncolored graphic for that frame; a later
                // main-thread call populates the cache and colors it correctly from then on.
                if (UnityData.IsInMainThread)
                {
                    return baseGraphic.GetColoredVersion(baseGraphic.Shader, colorable.Color, baseGraphic.ColorTwo);
                }
            }
            return baseGraphic;
        }
    }
}
