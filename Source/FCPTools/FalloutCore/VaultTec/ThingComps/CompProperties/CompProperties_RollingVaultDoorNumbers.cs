using UnityEngine;

namespace FCPVT;

public class CompProperties_RollingVaultDoorNumbers : CompProperties
{
    public CompProperties_RollingVaultDoorNumbers() => compClass = typeof(ThingComp_RollingVaultDoorNumbers);

    public float fadeFactor = 0f;
    public float rotationFactor = 0f;
    public bool shouldFade = false;
    public Vector2 drawSize = Vector2.one;
    public float numberSpacing = 1f;
    public Color numbersColor = Color.black;
}
