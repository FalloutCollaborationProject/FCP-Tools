namespace FCP.Core.Birds;

public class PawnRenderNodeWorker_Flying : PawnRenderNodeWorker
{
    protected override Graphic GetGraphic(PawnRenderNode node, PawnDrawParms parms)
    {
        var casted = (PawnRenderNode_Flying)node;

        if (parms.pawn.IsFlyingPawn(out var comp) && comp.isFlyingCurrently)
        {
            return casted.FlyingGraphic;
        }

        return base.GetGraphic(node, parms);
    }
}