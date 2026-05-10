using UnityEngine;
using Verse;

namespace FCP.Core.Buildings;

public class CompProperties_PowerLine : CompProperties
{
    public Vector3 drawOffset;
    
    public CompProperties_PowerLine()
    {
        compClass = typeof(CompPowerLine);
    }
}
