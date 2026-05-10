using Verse;

namespace FCP.Core.Buildings;

public class CompProperties_PowerLineGizmos : CompProperties
{
    public float iconScale = 0.65f;
    
    public CompProperties_PowerLineGizmos()
    {
        compClass = typeof(CompPowerLineGizmos);
    }
}
