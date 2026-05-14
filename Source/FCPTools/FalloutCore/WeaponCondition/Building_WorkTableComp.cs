using UnityEngine;

namespace FCP.Core.WeaponCondition;

public class Building_WorkTableComp : Building_WorkTable
{
    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        base.DrawAt(drawLoc, flip);
        Comps_PostDraw();
    }
}
