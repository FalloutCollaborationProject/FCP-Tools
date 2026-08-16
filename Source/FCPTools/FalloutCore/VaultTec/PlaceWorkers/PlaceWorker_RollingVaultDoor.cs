using UnityEngine;

namespace FCPVT;

public class PlaceWorker_RollingVaultDoor : PlaceWorker_DoorLearnOpeningSpeed
{
    public override void PostPlace(Map map, BuildableDef def, IntVec3 loc, Rot4 rot)
    {
        base.PostPlace(map, def, loc, rot);

        if (def is not ThingDef def2) return;
        foreach (IntVec3 item in DoorUtility.WallRequirementCells(def2, loc, rot))
        {
            if (!DoorUtility.EncapsulatingWallAt(item, map, includeUnbuilt: true))
            {
                Messages.Message("MessageBuildingRequiresAdjacentWalls".Translate(def).CapitalizeFirst(), MessageTypeDefOf.CautionInput, historical: false);
                break;
            }
        }
    }

    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
    {
        Texture2D texture = def == VTDefOf.FCPVT_RollingVaultDoor_Single
            ? Assets.RVD_SingleTex
            : Assets.RVD_DoubleTex;

        Material material = MaterialPool.MatFrom(texture, ShaderDatabase.Transparent, ghostCol);
        Vector3 doorScale = def.graphicData.drawSize;
        Vector3 doorLoc = GenThing.TrueCenter(center, rot, def.Size, AltitudeLayer.Blueprint.AltitudeFor());
        Matrix4x4 matrix = Matrix4x4.TRS(doorLoc, rot.AsQuat, new Vector3(doorScale.x, 1f, doorScale.y));

        Graphics.DrawMesh(MeshPool.plane10, matrix, material, 0);

        foreach (IntVec3 item in DoorUtility.WallRequirementCells(def, center, rot))
        {
            GhostDrawer.DrawGhostThing(item, Rot4.South, ThingDefOf.Wall,
                null, Color.grey, AltitudeLayer.Blueprint, null, drawPlaceWorkers: false);
        }
    }
}
