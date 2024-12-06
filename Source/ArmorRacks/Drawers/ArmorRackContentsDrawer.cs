using System.Collections.Generic;
using System.Linq;
using ArmorRacks.Things;
using RimWorld;
using UnityEngine;
using Verse;
using System;

namespace ArmorRacks.Drawers
{
    public class ArmorRackContentsDrawer
    {
        public readonly ArmorRack ArmorRack;
        public List<ApparelGraphicRecord> ApparelGraphics;
        public bool IsApparelResolved = false;

        public ArmorRackContentsDrawer(ArmorRack armorRack)
        {
            ArmorRack = armorRack;
            ApparelGraphics = new List<ApparelGraphicRecord>();
        }

        public virtual void DrawAt(Vector3 drawLoc)
        {
            if (!IsApparelResolved)
            {
                ResolveApparelGraphics();
            }
            DrawApparel(drawLoc);
            DrawWeapon(drawLoc);
        }

        public void DrawApparel(Vector3 drawLoc)
        { 
            float angle = 0.0f;
            Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 vector3_1 = drawLoc;
            Vector3 vector3_2 = drawLoc;
            Mesh mesh = MeshPool.GetMeshSetForWidth(MeshPool.HumanlikeBodyWidth).MeshAt(ArmorRack.Rotation);
            if (ArmorRack.Rotation != Rot4.North)
            {
                vector3_2.y += 7f / 256f;
                vector3_1.y += 3f / 128f;
            }
            else
            {
                vector3_2.y += 3f / 128f;
                vector3_1.y += 7f / 256f;
            }
            Vector3 vector3_3 = quaternion * BaseHeadOffsetAt(ArmorRack.Rotation);
            Vector3 loc = drawLoc;
            loc.y += 1f / 32f;

            for (int index = 0; index < ApparelGraphics.Count; ++index)
            {
                if (ApparelGraphics[index].sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.Shell &&
                    ApparelGraphics[index].sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.Overhead &&
                    ApparelGraphics[index].sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.Belt
                    )
                {
                    Material mat = ApparelGraphics[index].graphic.MatAt(ArmorRack.Rotation);
                    GenDraw.DrawMeshNowOrLater(mesh, loc, quaternion, mat, false);
                    loc.y += 1f / 32f;
                }
            }
            
            Vector3 loc1 = loc + vector3_3;
            loc1.y += 1f / 32f;

            for (int index = 0; index < ApparelGraphics.Count; ++index)
            {
                if (ApparelGraphics[index].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead)
                {
                    Material mat = ApparelGraphics[index].graphic.MatAt(ArmorRack.Rotation);
                    GenDraw.DrawMeshNowOrLater(mesh, loc1, quaternion, mat, false);
                }
                else if (ApparelGraphics[index].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Shell)
                {
                    Material mat = ApparelGraphics[index].graphic.MatAt(ArmorRack.Rotation);
                    GenDraw.DrawMeshNowOrLater(mesh, loc, quaternion, mat, false);
                }
                else if (ApparelGraphics[index].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Belt)
                {
                    Vector3 beltloc = loc;
                    beltloc.y = ArmorRack.Rotation == Rot4.South ? drawLoc.y : loc.y + 1f / 32f;
                    Vector2 vector2_1 = ApparelGraphics[index].sourceApparel.def.apparel.wornGraphicData.BeltOffsetAt(ArmorRack.Rotation, ArmorRack.BodyTypeDef);
                    Vector2 vector2_2 = ApparelGraphics[index].sourceApparel.def.apparel.wornGraphicData.BeltScaleAt(ArmorRack.Rotation, ArmorRack.BodyTypeDef);
                    Matrix4x4 matrix = Matrix4x4.Translate(beltloc) * Matrix4x4.Rotate(quaternion) * Matrix4x4.Translate(new Vector3(vector2_1.x, 0.0f, vector2_1.y)) * Matrix4x4.Scale(new Vector3(vector2_2.x, 1f, vector2_2.y));
                    Material mat = ApparelGraphics[index].graphic.MatAt(ArmorRack.Rotation);
                    GenDraw.DrawMeshNowOrLater(mesh, matrix, mat, false);
                    loc.y += 2f / 32f;
                }
            }
        }

        public void DrawWeapon(Vector3 drawLoc)
        {
            Thing storedWeapon = ArmorRack.GetStoredWeapon();
            if (storedWeapon == null)
            {
                return;
            }

            Vector3 weaponDrawLoc = drawLoc;
            Mesh weaponMesh;
            float angle = -90f;
            if (ArmorRack.Rotation == Rot4.South)
            {
                weaponDrawLoc += new Vector3(0.0f, 2.0f, -0.1f);
                weaponDrawLoc.y += 1f;
                weaponMesh = MeshPool.plane10;
                angle = -25f;
            }
            else if (ArmorRack.Rotation == Rot4.North)
            {
                weaponDrawLoc += new Vector3(0.0f, 0.0f, -0.11f);
                ref Vector3 local = ref weaponDrawLoc;
                weaponMesh = MeshPool.plane10;
            }
            else if (ArmorRack.Rotation == Rot4.East)
            {
                weaponDrawLoc += new Vector3(0.2f, 2.0f, -0.12f);
                weaponDrawLoc.y += 5f / 128f;
                weaponMesh = MeshPool.plane10;
                angle = 25f;
            }
            else
            {
                weaponDrawLoc += new Vector3(-0.2f, 2.0f, -0.12f);
                weaponDrawLoc.y += 5f / 128f;
                weaponMesh = MeshPool.plane10Flip;
                angle = -25f;
            }

            Graphic_StackCount graphic = storedWeapon.Graphic as Graphic_StackCount;
            Material material = graphic == null
                ? storedWeapon.Graphic.MatSingle
                : graphic.SubGraphicForStackCount(1, storedWeapon.def).MatSingle;
            Graphics.DrawMesh(weaponMesh, weaponDrawLoc, Quaternion.AngleAxis(angle, Vector3.up), material, 0);
        }

        public void ResolveApparelGraphics()
        {
            ApparelGraphics.Clear();
            var apparelList = ArmorRack.GetStoredApparel().ToList();
            apparelList.Sort(((a, b) => a.def.apparel.LastLayer.drawOrder.CompareTo(b.def.apparel.LastLayer.drawOrder)));
            foreach (Apparel apparel in apparelList)
            {
                ApparelGraphicRecord rec;
                if (ApparelGraphicRecordGetter.TryGetGraphicApparel(apparel, ArmorRack.BodyTypeDef, out rec))
                {
                    ApparelGraphics.Add(rec);
                }
            }
            IsApparelResolved = true;
        }
        

        public Vector3 BaseHeadOffsetAt(Rot4 rotation)
        {
            Vector2 headOffset = ArmorRack.BodyTypeDef.headOffset;
            switch (rotation.AsInt)
            {
                case 0:
                    return new Vector3(0.0f, 0.0f, headOffset.y);
                case 1:
                    return new Vector3(headOffset.x, 0.0f, headOffset.y);
                case 2:
                    return new Vector3(0.0f, 0.0f, headOffset.y);
                case 3:
                    return new Vector3(-headOffset.x, 0.0f, headOffset.y);
                default:
                    return Vector3.zero;
            }
        }
    }
}