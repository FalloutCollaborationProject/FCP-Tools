using RimWorld.Planet;

namespace FCP.Core;

public class UniqueCharacter : IExposable
{
    public CharacterDef def;
    public Pawn pawn;
    
    public UniqueCharacter(CharacterDef def)
    {
        this.def = def;
    }

    /// <summary>
    /// Check that a pawn is not null and not destroyed
    /// </summary>
    public bool PawnExists()
    {
        if (pawn == null) return false;
        return !pawn.Discarded;
    }

    /// <summary>
    /// Check if the pawn is available in the world (not dead and tracked).
    /// </summary>
    public bool PawnAvailableInWorld()
    {
        return pawn != null && !pawn.Dead && pawn.IsWorldPawn();
    }

    #region Generation
    
    /// <summary>
    /// Ensure the pawn exists, generating it if necessary.
    /// </summary>
    public Pawn EnsurePawnExists(PawnGenerationRequest? baseParams = null, 
        Func<PawnGenerationRequest, PawnGenerationRequest> modifyGenRequest = null)
    {
        if (PawnExists()) 
            return pawn;

        pawn = GeneratePawn(baseParams, modifyGenRequest);

        // Ensure the pawn is managed by the world pawns and kept from gc (also makes sure it's stored somewhere and saved)
        Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.KeepForever);

        return pawn;
    }

    /// <summary>
    /// Generate a new pawn based on this character's definition.
    /// </summary>
    private Pawn GeneratePawn(
        PawnGenerationRequest? baseParams = null,
        Func<PawnGenerationRequest, PawnGenerationRequest> modifyGenRequest = null)
    {
        FCPLog.Message($"Generating Unique Pawn: {def.defName}");
        PawnGenerationRequest request = baseParams ?? new PawnGenerationRequest(def.pawnKind);

        // Defaults
        request.KindDef ??= def.pawnKind;
        request.Faction ??= Find.FactionManager.FirstFactionOfDef(def.faction); // May be null for initial faction leaders.
        request.ForceGenerateNewPawn = true;

        // Apply custom definitions or modifications to the generation request.
        CharacterDefinitionUtils.ApplyRequestDefinitions(ref request, def.definitions);
        request = modifyGenRequest?.Invoke(request) ?? request;

        // Generate the pawn and apply post gen definitions
        Pawn generatedPawn = PawnGenerator.GeneratePawn(request);
        CharacterDefinitionUtils.ApplyPawnDefinitions(generatedPawn, def.definitions);

        return generatedPawn;
    }
    
    #endregion

    public void ExposeData()
    {
        Scribe_Defs.Look(ref def, "def");
        Scribe_References.Look(ref pawn, "pawn");
    }
}