using ArmorRacks.Things;
using UnityEngine;
using Verse;

namespace ArmorRacks.Drawers
{
    public class MechanizedArmorRackContentsDrawer : ArmorRackContentsDrawer
    {
        public MechanizedArmorRackContentsDrawer(ArmorRack armorRack) : base(armorRack)
        {
        }

        public override void DrawAt(Vector3 drawLoc)
        {
            var newDrawLoc = drawLoc;
            if (ArmorRack.Rotation == Rot4.East)
            {
                newDrawLoc.x += 0.4f;
            }
            else if (ArmorRack.Rotation == Rot4.West)
            {
                newDrawLoc.x -= 0.4f;
            } else if (ArmorRack.Rotation == Rot4.North)
            {
                newDrawLoc.z += 0.3f;
            }
            base.DrawAt(newDrawLoc);
        }
    }
}