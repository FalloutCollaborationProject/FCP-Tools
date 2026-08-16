using UnityEngine;

namespace FCPVT;

public class ThingComp_RollingVaultDoorBase : ThingComp
{
    public float FadeMultiplier;
    public float RotationAngle;
    public Rot4 Rotation;
    public Vector3 MoveDir;
    public Vector3 DrawPos;
    public Matrix4x4 Matrix;
    public Material FinalMat;
    public readonly MaterialPropertyBlock Mpb = new();

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        Rotation = parent.Rotation;
    }
}
