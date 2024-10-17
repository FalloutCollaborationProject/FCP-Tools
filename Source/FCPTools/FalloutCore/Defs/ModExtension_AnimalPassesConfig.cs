using System.Diagnostics.CodeAnalysis;

namespace FCP.Core;

/// <summary>
/// Configuration ModExtension for the IncidentWorker_AnimalPasses
/// </summary>
[UsedImplicitly, SuppressMessage("ReSharper", "UnassignedField.Global")]
public class ModExtension_AnimalPassesConfig : DefModExtension
{
    public ThingDef animalThing;
    public PawnKindDef animalPawnKind;

    public int minCount = 1;
    public IntRange maxRangeInclusive = new IntRange(3, 6);
    public IntRange ticksToLeave = new IntRange(90000, 150000);

    public bool ignoreTemperature = false;
    public bool ignoreToxicFallout = false;

    public override IEnumerable<string> ConfigErrors()
    {
        if (animalThing == null)
            yield return "[ModExtension_AnimalPassesConfig] animalThing is null.";
        if (animalPawnKind == null)
            yield return "[ModExtension_AnimalPassesConfig] animalPawnKind is null.";
    }
}