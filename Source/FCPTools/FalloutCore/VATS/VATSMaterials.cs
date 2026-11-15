using UnityEngine;

namespace FCP.Core.VATS;

[StaticConstructorOnStartup]
public static class VATSMaterials
{
    private static Material _zoomMat;

    public static Material ZoomMat => _zoomMat ??= CreateZoomMat();

    private static Material CreateZoomMat()
    {
        Shader shader = FCPDefOf.FCP_VATSZoom?.Shader;
        if (shader == null)
        {
            FCPLog.Warning("Could not load VATS zoom shader FCP_VATSZoom");
            return BaseContent.ClearMat;
        }

        var mat = new Material(shader);
        mat.SetTexture("_Mask", ContentFinder<Texture2D>.Get("Things/FCP_VATS_ZoomMask"));
        mat.SetFloat("_Magnification", 3f);
        mat.SetVector("_UVCenterOffset", Vector4.zero);
        return mat;
    }
}
