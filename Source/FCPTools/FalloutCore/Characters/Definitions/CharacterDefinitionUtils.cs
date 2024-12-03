namespace FCP.Core;

public static class CharacterDefinitionUtils
{
    public static void ApplyRequestDefinitions(ref PawnGenerationRequest request, List<CharacterBaseDefinition> definitions)
    {
        foreach (CharacterBaseDefinition definition in definitions)
        {
            if (!definition.AppliesPreGeneration) return;
            definition.ApplyToRequest(ref request);
        }
        request.ValidateAndFix();
    }
    
    public static void ApplyPawnDefinitions(Pawn pawn, List<CharacterBaseDefinition> definitions)
    {
        foreach (CharacterBaseDefinition definition in definitions)
        {
            if (!definition.AppliesPostGeneration) return;
            definition.ApplyToPawn(pawn);
        }
    }
}