namespace FCP.Core;

public class CompProperties_TopGraphicOverhang : CompProperties
{
    public string texPath;
    public string graphicClass = "Graphic_Multi";
    public AltitudeLayer altitudeLayer = AltitudeLayer.PawnState;

    public CompProperties_TopGraphicOverhang()
    {
        compClass = typeof(CompTopGraphicOverhang);
    }
}
