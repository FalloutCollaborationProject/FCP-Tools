namespace FCP.Core;

public class PawnRenderNodeWorker_Flying : PawnRenderNodeWorker
{
    public override Graphic GetGraphic(PawnRenderNode node, PawnDrawParms parms)
    {
        PawnRenderNode_Flying casted = (PawnRenderNode_Flying)node;

        if (parms.pawn.IsFlyingPawn(out CompFlyingPawn comp) && comp.isFlyingCurrently)
        {
            return casted.FlyingGraphic;
        }
        return base.GetGraphic(node, parms);
    }
}