namespace FCP.Core;

[StaticConstructorOnStartup]
public static class PackAnimalRestriction
{
    private const string BrahminDefName = "FCP_Animal_Brahmin";

    static PackAnimalRestriction()
    {
        foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
        {
            if (def.race == null || !def.race.packAnimal || def.defName == BrahminDefName)
                continue;

            def.race.packAnimal = false;
        }
    }
}
