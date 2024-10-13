using System.Collections.Generic;
using Verse;

namespace FCP.Core.Utils;

public static class PawnGenerationUtils
{
    public static Pawn GenerateWithDefinitions(PawnGenerationRequest request, params PawnGenerationDefinition[] definitions)
    {
        ApplyRequestDefinitions(ref request, definitions);
        
        var pawn = PawnGenerator.GeneratePawn(request);
        ApplyPawnDefinitions(pawn, definitions);

        return pawn;
    }
    
    public static void ApplyRequestDefinitions(ref PawnGenerationRequest request, IEnumerable<PawnGenerationDefinition> definitions)
    {
        foreach (var definition in definitions)
        {
            if (!definition.AppliesPreGeneration)
                return;
            
            definition.ApplyToRequest(ref request);
        }
        
        request.ValidateAndFix();
    }
    
    public static void ApplyPawnDefinitions(Pawn pawn, IEnumerable<PawnGenerationDefinition> definitions)
    {
        foreach (var definition in definitions)
        {
            if (!definition.AppliesPostGeneration)
                return;
            
            definition.ApplyToPawn(pawn);
        }
    }
}