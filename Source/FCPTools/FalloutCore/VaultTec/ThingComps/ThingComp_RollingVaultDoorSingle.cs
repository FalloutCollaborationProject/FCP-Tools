using UnityEngine;

namespace FCPVT;

public class ThingComp_RollingVaultDoorSingle : ThingComp_RollingVaultDoorBase
{
    public CompProperties_RollingVaultDoorSingle Props => (CompProperties_RollingVaultDoorSingle)props;
    public Building_RollingVaultDoor VaultDoor;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        VaultDoor = parent as Building_RollingVaultDoor;
    }

    public override void PostDraw()
    {
        base.PostDraw();

        if (Props is not { extraDoorGraphics: not null }) return;
        foreach (GraphicData_RollingVaultDoor gD in Props.extraDoorGraphics)
        {
            FadeMultiplier = 1f - (VaultDoor.OpenPct * gD.fadeFactor);
            Graphic graphic = gD.Graphic;
            Material material = graphic.MatSingle;

            switch (Rotation.AsInt)
            {
                case 0:
                    float xMoveS = gD.isLeftSideGraphic ? -gD.xMoveAmount : gD.xMoveAmount;
                    MoveDir = new Vector3(xMoveS, 0f, 0f);
                    break;
                case 1:
                    float zMoveW = gD.isLeftSideGraphic ? gD.xMoveAmount : -gD.xMoveAmount;
                    MoveDir = new Vector3(0f, 0f, zMoveW);
                    break;
                case 2:
                    float xMoveN = gD.isLeftSideGraphic ? gD.xMoveAmount : -gD.xMoveAmount;
                    MoveDir = new Vector3(xMoveN, 0f, 0f);
                    break;
                case 3:
                    float zMoveE = gD.isLeftSideGraphic ? -gD.xMoveAmount : gD.xMoveAmount;
                    MoveDir = new Vector3(0f, 0f, zMoveE);
                    break;
                default:
                    MoveDir = Vector3.zero;
                    break;
            }

            DrawExtraDoorGraphics(MoveDir, gD.maxAngle, gD.rotationFactor, gD.shouldFade, FadeMultiplier,
                VaultDoor.OpenPct, material, gD.drawSize);
        }
    }

    private void DrawExtraDoorGraphics(Vector3 xMoveAmount, float maxAngle, float rotationFactor, bool shouldFade, float opacity,
        float openPct, Material material, Vector3 drawSize)
    {
        DrawPos = parent.DrawPos + xMoveAmount * openPct;
        RotationAngle = maxAngle * openPct;
        Matrix = Matrix4x4.TRS(DrawPos, Rotation.AsQuat * Quaternion.Euler(0f, RotationAngle * rotationFactor, 0f), new Vector3(drawSize.x, 1f, drawSize.y));
        FinalMat = shouldFade ? FadedMaterialPool.FadedVersionOf(material, opacity) : material;

        Mpb.Clear();

        Mpb.SetColor(ShaderPropertyIDs.Color, new Color(material.color.r, material.color.g, material.color.b, opacity));
        Graphics.DrawMesh(MeshPool.plane10, Matrix, FinalMat, 0, null, 0, Mpb);
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref FadeMultiplier, "FadeMultiplier");
        Scribe_Values.Look(ref MoveDir, "MoveDir");
    }
}
