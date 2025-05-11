using Verse;

namespace FCP_Ghoul
{
    public class PawnRenderNodeWorker_CustomColor : PawnRenderNodeWorker_Fur
    {
        public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
        {
            return base.CanDrawNow(node, parms);
        }
    }
}
