using UnityEngine;

namespace FCP.Core;

public class CompTopGraphicOverhang : ThingComp
{
    private Graphic overhangGraphic;

    private CompProperties_TopGraphicOverhang Props => (CompProperties_TopGraphicOverhang)props;

    private Graphic OverhangGraphic
    {
        get
        {
            if (overhangGraphic == null)
            {
                System.Type graphicClass = GenTypes.GetTypeInAnyAssembly(Props.graphicClass, "Verse");
                overhangGraphic = GraphicDatabase.Get(graphicClass, Props.texPath, ShaderDatabase.Cutout,
                    parent.def.graphicData.drawSize, Color.white, Color.white);
            }
            return overhangGraphic;
        }
    }

    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        Vector3 overhangLoc = new(drawLoc.x, Props.altitudeLayer.AltitudeFor(), drawLoc.z);
        OverhangGraphic.Draw(overhangLoc, flip ? parent.Rotation.Opposite : parent.Rotation, parent);
    }
}
