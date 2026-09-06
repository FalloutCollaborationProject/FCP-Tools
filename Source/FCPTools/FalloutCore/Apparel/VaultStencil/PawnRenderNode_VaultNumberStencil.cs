using UnityEngine;

namespace FCP.Core;

public class Graphic_VaultNumberStencil : Graphic_Single
{
    public Material material;

    public override Material MatSingle => material;
    public override Material MatWest => material;
    public override Material MatSouth => material;
    public override Material MatEast => material;
    public override Material MatNorth => material;

    public override Material MatAt(Rot4 rot, Thing thing = null)
    {
        return material;
    }
}

public class PawnRenderNodeProperties_VaultNumberStencil : PawnRenderNodeProperties
{
    public BodyPartGroupDef targetGroup;
}

public class PawnRenderNode_VaultNumberStencil : PawnRenderNode
{
    public PawnRenderNode_VaultNumberStencil(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
        : base(pawn, props, tree)
    {
    }

    private PawnRenderNodeProperties_VaultNumberStencil StencilProps => (PawnRenderNodeProperties_VaultNumberStencil)props;

    public CompVaultNumberStencil CurrentStencil(Pawn pawn)
    {
        BodyPartGroupDef group = StencilProps.targetGroup;
        if (pawn?.apparel == null || group == null)
            return null;

        foreach (Apparel apparel in pawn.apparel.WornApparel)
        {
            if (apparel.def.apparel.bodyPartGroups.Contains(group))
                return apparel.GetComp<CompVaultNumberStencil>();
        }
        return null;
    }
}

public class PawnRenderNodeWorker_VaultNumberStencil : PawnRenderNodeWorker
{
    public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
    {
        if (parms.facing != Rot4.North)
            return false;

        return base.CanDrawNow(node, parms) && ((PawnRenderNode_VaultNumberStencil)node).CurrentStencil(parms.pawn)?.Graphic != null;
    }

    protected override Graphic GetGraphic(PawnRenderNode node, PawnDrawParms parms)
    {
        return ((PawnRenderNode_VaultNumberStencil)node).CurrentStencil(parms.pawn)?.Graphic;
    }

    public override Vector3 ScaleFor(PawnRenderNode node, PawnDrawParms parms)
    {
        CompVaultNumberStencil stencil = ((PawnRenderNode_VaultNumberStencil)node).CurrentStencil(parms.pawn);
        Vector2 drawSize = stencil?.Graphic?.drawSize ?? Vector2.one;
        float bodyScale = BodyTypeScale(parms.pawn.story?.bodyType);
        return new Vector3(drawSize.x * bodyScale, 1f, drawSize.y * bodyScale);
    }

    private static float BodyTypeScale(BodyTypeDef bodyType)
    {
        if (bodyType == BodyTypeDefOf.Hulk)
            return 1.3f;
        if (bodyType == BodyTypeDefOf.Fat)
            return 1.18f;
        if (bodyType == BodyTypeDefOf.Thin)
            return 0.68f;
        if (bodyType == BodyTypeDefOf.Female)
            return 0.78f;
        return 1f;
    }
}
