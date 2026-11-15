using UnityEngine;

namespace FCP.Core.VATS;

public class Graphic_Zoomer : Graphic_Single
{
    public Thing Parent;
    public override Material MatSingle => VATSMaterials.ZoomMat;

    public override Material MatWest => VATSMaterials.ZoomMat;

    public override Material MatSouth => VATSMaterials.ZoomMat;

    public override Material MatEast => VATSMaterials.ZoomMat;

    public override Material MatNorth => VATSMaterials.ZoomMat;

    public override Material MatAt(Rot4 rot, Thing thing = null)
    {
        return VATSMaterials.ZoomMat;
    }

    public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
    {
        if (Parent != null)
        {
            loc = Parent.DrawPos;
            rot = Parent.Rotation;
        }

        loc.y = AltitudeLayer.MetaOverlays.AltitudeFor();

        base.DrawWorker(loc, rot, thingDef, thing, extraRotation);
    }
}
