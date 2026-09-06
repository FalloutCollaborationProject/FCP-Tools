using UnityEngine;

namespace FCP.Core.Synths;

public class Gene_SynthBody : Gene
{
    private Graphic cachedGraphic;
    private string cachedBodyType;
    private Color cachedSkinColor;

    public override void PostAdd()
    {
        base.PostAdd();
        pawn.Drawer.renderer.SetAllGraphicsDirty();
    }

    public override void PostRemove()
    {
        base.PostRemove();
        pawn.Drawer.renderer.SetAllGraphicsDirty();
    }

    public Graphic GetBodyOverlay(Pawn pawn)
    {
        string bodyType = (pawn.story?.bodyType ?? BodyTypeDefOf.Male).defName;
        Color skinColor = pawn.story?.SkinColor ?? Color.white;

        if (cachedGraphic != null && cachedBodyType == bodyType && cachedSkinColor == skinColor)
            return cachedGraphic;

        string path = $"FCP_Synth/GenII/Bodies/Naked_{bodyType}";

        cachedGraphic = GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.CutoutSkin, Vector2.one, skinColor);
        cachedBodyType = bodyType;
        cachedSkinColor = skinColor;

        return cachedGraphic;
    }
}
