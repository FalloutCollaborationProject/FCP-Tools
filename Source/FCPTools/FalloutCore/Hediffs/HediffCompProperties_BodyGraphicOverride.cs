using UnityEngine;

namespace FCP.Core.Hediffs;

public class HediffCompProperties_BodyGraphicOverride : HediffCompProperties
{
    public string texPath;

    public Vector2 drawSize = Vector2.one;

    public HediffCompProperties_BodyGraphicOverride() =>
        compClass = typeof(HediffComp_BodyGraphicOverride);
}
