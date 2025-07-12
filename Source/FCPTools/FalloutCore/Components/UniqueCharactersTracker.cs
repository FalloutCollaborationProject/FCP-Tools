using RimWorld.Planet;

namespace FCP.Core;

[UsedImplicitly]
public class UniqueCharactersTracker : WorldComponent
{
    public static UniqueCharactersTracker Instance { get; private set; }
    
    private List<UniqueCharacter> characters = [];
    
    public UniqueCharactersTracker(World world) : base(world)
    {
        Instance = this;
    }
    
    /// <summary>
    /// Check for a UniqueCharacter entry in the tracker and if the entry has a non-destroyed/discarded pawn.
    /// </summary>
    public bool CharacterPawnExists(CharacterDef charDef)
    {
        UniqueCharacter character = characters.Find(chr => chr.def == charDef);
        return character != null && character.PawnExists();
    }
    
    /// <summary>
    /// Check for a UniqueCharacter entry in the tracker and if the entry has a non destroyed/discarded living pawn.
    /// </summary>
    public bool CharacterPawnExistsAlive(CharacterDef charDef)
    {
        UniqueCharacter character = characters.Find(chr => chr.def == charDef);
        return character != null && character.PawnExists() && !character.pawn.Dead;
    }
    
    /// <summary>
    /// Check for a UniqueCharacter entry in the tracker and if the entry has a non destroyed/discarded dead pawn.
    /// </summary>
    public bool CharacterPawnDead(CharacterDef charDef)
    {
        for (int i = 0; i < characters.Count; i++)
        {
            if (characters[i].def != charDef) continue;
            var character = characters[i];
            return character.pawn == null || character.pawn is { Dead: true };
        }
        return true;
    }
    
    /// <summary>
    /// Check for a UniqueCharacter entry in the tracker and if the entry has a non destroyed/discarded dead pawn.
    /// </summary>
    public bool CharacterPawnSpawned(CharacterDef charDef)
    {
        for (int i = 0; i < characters.Count; i++)
        {
            if (characters[i].def != charDef) continue;
            var character = characters[i];
            return character != null && character.PawnExists() && character.pawn.Spawned;
        }
        return false;
    }
    
    /// <summary>
    /// Try to find a matching UniqueCharacter for a given pawn
    /// </summary>
    public bool TryGetPawnCharacter(Pawn pawn, out UniqueCharacter character)
    {
        character = characters.Find(chr => chr.pawn == pawn);
        return character != null;
    }
    
    public bool IsUniquePawn(Pawn pawn)
    {
        return TryGetPawnCharacter(pawn, out _);
    }

    public Pawn GetOrGenPawn(CharacterDef charDef, PawnGenerationRequest? requestParams = null, Faction forcedFaction = null)
    {
        // If the character entry doesn't exist make one, if it does and has a pawn, return that.
        UniqueCharacter character = characters.Find(chr => chr.def == charDef);
        
        if (character == null)
        {
            character = new UniqueCharacter(charDef);
            characters.Add(character);
        }
        else if (character.PawnExists())
        {
            return character.pawn;
        }

        // Time to generate one then.
        #if DEBUG
        FCPLog.Message($"Generating Unique Pawn: {charDef.defName}");
        #endif
        
        // Create a new request if one wasn't provided, also ensure it's valid.
        PawnGenerationRequest request = requestParams ?? new PawnGenerationRequest(charDef.pawnKind);
        request.KindDef ??= charDef.pawnKind;
        request.Faction ??= Find.FactionManager.FirstFactionOfDef(charDef.faction);
        request.ForceGenerateNewPawn = true;
        
        // Generate the pawn.
        CharacterDefinitionUtils.ApplyRequestDefinitions(ref request, charDef.definitions);
        character.pawn = PawnGenerator.GeneratePawn(request);
        CharacterDefinitionUtils.ApplyPawnDefinitions(character.pawn, charDef.definitions);

        // Set the pawn to be ignored by the World Pawn GC and pass it to the world so it has somewhere to be saved.
        Find.WorldPawns.PassToWorld(character.pawn, PawnDiscardDecideMode.KeepForever);
        
        return character.pawn;
    }

    public override void FinalizeInit(bool fromLoad)
    {
        base.FinalizeInit(fromLoad);
        Instance = this;
    }
    
    public override void ExposeData()
    {
        Scribe_Collections.Look(ref characters, "character", lookMode: LookMode.Deep, saveDestroyedThings: true);
    }
}