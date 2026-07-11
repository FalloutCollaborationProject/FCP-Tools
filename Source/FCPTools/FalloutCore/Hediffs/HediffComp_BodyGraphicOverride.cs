using UnityEngine;

namespace FCP.Core.Hediffs;

public class HediffComp_BodyGraphicOverride : HediffComp
{
    public HediffCompProperties_BodyGraphicOverride Props =>
        (HediffCompProperties_BodyGraphicOverride)props;

    private Graphic cachedGraphic;

    public Graphic BodyGraphic =>
        cachedGraphic ??= GraphicDatabase.Get<Graphic_Multi>(Props.texPath, ShaderDatabase.Cutout, Props.drawSize, Color.white);

    public override void CompPostPostAdd(DamageInfo? dinfo)
    {
        Pawn.Drawer.renderer.SetAllGraphicsDirty();
    }

    public override void CompPostPostRemoved()
    {
        Pawn.Drawer.renderer.SetAllGraphicsDirty();
    }
}
