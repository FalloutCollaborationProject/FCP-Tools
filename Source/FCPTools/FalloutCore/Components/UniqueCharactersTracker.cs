using RimWorld.Planet;

namespace FCP.Core;

[UsedImplicitly]
public class UniqueCharactersTracker : WorldComponent
{
    
    public static UniqueCharactersTracker Instance { get; private set; }

    private List<UniqueCharacter> characters = new List<UniqueCharacter>();

    public UniqueCharactersTracker(World world) : base(world)
    {
        Instance = this;
    }

    /// <summary>
    /// Check for a UniqueCharacter entry in the tracker and if the entry has a non destroyed/discarded pawn.
    /// </summary>
    public bool CharacterPawnExists(CharacterDef charDef, bool aliveOnly = false)
    {
        UniqueCharacter character = characters.Find(chr => chr.def == charDef);
        return character != null && character.PawnExists() && (!aliveOnly || !character.pawn.Dead);
    }

    /// <summary>
    /// Check for a UniqueCharacter entry in the tracker and if the entry has a non destroyed/discarded living world pawn.
    /// </summary>
    public bool CharacterWorldPawnAvailable(CharacterDef charDef)
    {
        UniqueCharacter character = characters.Find(chr => chr.def == charDef);
        return character.PawnAvailableInWorld();
    }
    
    /// <summary>
    /// Try find a matching UniqueCharacter for a given pawn
    /// </summary>
    public bool IsUniquePawn(Pawn pawn, out UniqueCharacter character)
    {
        character = characters.Find(chr => chr.pawn == pawn);
        return character != null;
    }

    /// <summary>
    /// Get an existing UniqueCharacter for the given CharacterDef, or create a new one if it doesn't exist.
    /// </summary>
    private UniqueCharacter GetOrCreateUniqueCharacter(CharacterDef charDef)
    {
        UniqueCharacter character = characters.Find(chr => chr.def == charDef);

        // If it doesn't exist, create a new entry and add it to the tracker
        if (character == null)
        {
            character = new UniqueCharacter(charDef);
            characters.Add(character);
        }

        return character;
    }

    /// <summary>
    /// Get an existing pawn or generate a new one for the given CharacterDef.
    /// </summary>
    public Pawn GetOrGenPawn(CharacterDef charDef, 
        PawnGenerationRequest? baseParams = null,
        Func<PawnGenerationRequest, PawnGenerationRequest> modifyGenRequest = null)
    {
        // Create a new, or get an existing unique character from the tracker
        UniqueCharacter character = GetOrCreateUniqueCharacter(charDef);
        
        if (character.PawnExists())
        {
            return character.pawn;
        }
        
        character.pawn = GenerateUniquePawn(charDef, baseParams, modifyGenRequest);

        // Makes sure the pawn is saved somewhere and immune from GC.
        Find.WorldPawns.PassToWorld(character.pawn, PawnDiscardDecideMode.KeepForever);

        return character.pawn;
    }
    
    /// <summary>
    /// Generate a new pawn for the given CharacterDef.
    /// </summary>
    private static Pawn GenerateUniquePawn(CharacterDef charDef, 
        PawnGenerationRequest? baseParams = null,
        Func<PawnGenerationRequest, PawnGenerationRequest> modifyGenRequest = null)
    {
        
        FCPLog.Message($"Generating Unique Pawn: {charDef.defName}");
        PawnGenerationRequest request = baseParams ?? new PawnGenerationRequest(charDef.pawnKind);

        // Defaults
        request.KindDef ??= charDef.pawnKind;
        request.Faction ??= Find.FactionManager.FirstFactionOfDef(charDef.faction); // May be null for initial faction leaders.
        request.ForceGenerateNewPawn = true;

        // Apply any custom definitions or modifications to the generation request
        CharacterDefinitionUtils.ApplyRequestDefinitions(ref request, charDef.definitions);
        request = modifyGenRequest?.Invoke(request) ?? request;

        // Actually Generate the pawn
        Pawn pawn = PawnGenerator.GeneratePawn(request);

        // Post generation definitions to the actual Pawn
        CharacterDefinitionUtils.ApplyPawnDefinitions(pawn, charDef.definitions);

        return pawn;
    }
    
    public override void FinalizeInit()
    {
        base.FinalizeInit();
        Instance = this;

    }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref characters, "character", lookMode: LookMode.Deep, saveDestroyedThings: true);
    }
    
}