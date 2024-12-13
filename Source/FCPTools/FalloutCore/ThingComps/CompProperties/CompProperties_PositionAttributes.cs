using UnityEngine;

namespace FCP.Core;

public class CompProperties_PositionAttributes : CompProperties
{
    public Vector2 DraftedDrawOffset = Vector2.zero;
    public Vector2 HeldDrawOffset = Vector2.zero;

    public Vector2 HeldDrawOffsetAbsolute = Vector2.zero;
    public Vector2 DraftedDrawOffsetAbsolute = Vector2.zero;

    public CompProperties_PositionAttributes()
    {
        compClass = typeof(CompPositionAttributes);
    }
}