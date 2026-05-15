using System.Collections.Generic;

namespace FCP.Core;

public class CompProperties_HatcherCustom : CompProperties
{
    public float hatcherDaystoHatch = 1f;

    public PawnKindDef hatcherPawn;
    public float secondaryOverrideChance = 0f;
    public PawnKindDef secondaryPawn;
    public List<WeightedHatchEntry> weightedPawns;

    public CompProperties_HatcherCustom()
    {
        compClass = typeof(CompHatcherCustom);
    }
}

public class WeightedHatchEntry
{
    public PawnKindDef pawnKind;
    public float weight = 1f;
}