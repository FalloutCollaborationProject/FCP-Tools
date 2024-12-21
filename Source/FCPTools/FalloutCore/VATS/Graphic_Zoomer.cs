using FCP.Core.Unity;
using UnityEngine;

namespace FCP.Core.VATS;

public class Graphic_Zoomer : Graphic_Single
{
    public Thing Parent;
    public override Material MatSingle => Materials.ZoomMat;

    public override Material MatWest => Materials.ZoomMat;

    public override Material MatSouth => Materials.ZoomMat;

    public override Material MatEast => Materials.ZoomMat;

    public override Material MatNorth => Materials.ZoomMat;

    public override Material MatAt(Rot4 rot, Thing thing = null)
    {
        return Materials.ZoomMat;
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
