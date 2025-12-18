namespace FCP.Core;

public static class CharacterDefinitionUtils
{
    public static Pawn GenerateWithDefinitions(PawnGenerationRequest request, List<CharacterBaseDefinition> definitions)
    {
        ApplyRequestDefinitions(ref request, definitions);
        Pawn pawn = PawnGenerator.GeneratePawn(request);
        ApplyPawnDefinitions(pawn, definitions);

        return pawn;
    }
    
    public static void ApplyRequestDefinitions(ref PawnGenerationRequest request, List<CharacterBaseDefinition> definitions)
    {
        foreach (CharacterBaseDefinition definition in definitions)
        {
            if (!definition.AppliesPreGeneration) 
                continue;
            
            definition.ApplyToRequest(ref request);
        }
        request.ValidateAndFix();
    }
    
    public static void ApplyPawnDefinitions(Pawn pawn, List<CharacterBaseDefinition> definitions)
    {
        foreach (CharacterBaseDefinition definition in definitions)
        {
            if (!definition.AppliesPostGeneration) 
                continue;
            
            definition.ApplyToPawn(pawn);
        }
    }
}