using RimWorld.Planet;

namespace FCP.Core;

[UsedImplicitly]
public class UniqueCharactersTracker : WorldComponent
{
    public static UniqueCharactersTracker Instance { get; private set; }

    private List<UniqueCharacter> characters = [];
    private HashSet<ThingDef> spawnedUniqueThings = [];

    // Dictionary indices for O(1) lookups
    private readonly Dictionary<CharacterDef, UniqueCharacter> charactersByDef = new Dictionary<CharacterDef, UniqueCharacter>();
    private readonly Dictionary<Pawn, UniqueCharacter> charactersByPawn = new Dictionary<Pawn, UniqueCharacter>();

    public UniqueCharactersTracker(World world) : base(world)
    {
        Instance = this;
    }

    /// <summary>
    /// Check for a UniqueCharacter entry in the tracker and if the entry has a non-destroyed/discarded pawn.
    /// </summary>
    public bool CharacterPawnExists(CharacterDef charDef)
    {
        return charactersByDef.TryGetValue(charDef, out var character) && character.PawnExists();
    }

    /// <summary>
    /// Check for a UniqueCharacter entry in the tracker and if the entry has a non destroyed/discarded living pawn.
    /// </summary>
    public bool CharacterPawnExistsAlive(CharacterDef charDef)
    {
        return charactersByDef.TryGetValue(charDef, out var character) && character.PawnExists() && !character.pawn.Dead;
    }

    /// <summary>
    /// Check for a UniqueCharacter entry in the tracker and if the entry has a non destroyed/discarded dead pawn.
    /// </summary>
    public bool CharacterPawnDead(CharacterDef charDef)
    {
        if (!charactersByDef.TryGetValue(charDef, out var character))
            return true;
        return character.pawn == null || character.pawn is { Dead: true };
    }

    /// <summary>
    /// Check for a UniqueCharacter entry in the tracker and if the entry has a spawned pawn.
    /// </summary>
    public bool CharacterPawnSpawned(CharacterDef charDef)
    {
        return charactersByDef.TryGetValue(charDef, out var character) && character.PawnExists() && character.pawn.Spawned;
    }

    /// <summary>
    /// Try to find a matching UniqueCharacter for a given pawn
    /// </summary>
    public bool TryGetPawnCharacter(Pawn pawn, out UniqueCharacter character)
    {
        return charactersByPawn.TryGetValue(pawn, out character);
    }

    public bool IsUniquePawn(Pawn pawn)
    {
        return TryGetPawnCharacter(pawn, out _);
    }

    public bool IsUniqueThingCreated(ThingDef def)
    {
        return spawnedUniqueThings.Contains(def);
    }

    public void Notify_UniqueThingSpawned(ThingDef def)
    {
        spawnedUniqueThings.Add(def);
    }

    public void Notify_UniqueThingDestroyed(ThingDef def)
    {
        spawnedUniqueThings.Remove(def);
    }

    public Pawn GetOrGenPawn(CharacterDef charDef, PawnGenerationRequest? requestParams = null, Faction forcedFaction = null)
    {
        // If the character entry doesn't exist make one, if it does and has a pawn, return that.
        if (!charactersByDef.TryGetValue(charDef, out var character))
        {
            character = new UniqueCharacter(charDef);
            characters.Add(character);
            charactersByDef[charDef] = character;
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

        // Add to pawn lookup dictionary
        charactersByPawn[character.pawn] = character;

        // Set the pawn to be ignored by the World Pawn GC and pass it to the world so it has somewhere to be saved.
        Find.WorldPawns.PassToWorld(character.pawn, PawnDiscardDecideMode.KeepForever);

        return character.pawn;
    }

    public override void FinalizeInit(bool fromLoad)
    {
        base.FinalizeInit(fromLoad);
        Instance = this;

        if (fromLoad)
            RebuildDictionaries();
    }

    private void RebuildDictionaries()
    {
        charactersByDef.Clear();
        charactersByPawn.Clear();

        foreach (UniqueCharacter character in characters)
        {
            if (character.def != null)
                charactersByDef[character.def] = character;
            if (character.pawn != null)
                charactersByPawn[character.pawn] = character;
        }
    }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref characters, "character", lookMode: LookMode.Deep, saveDestroyedThings: true);
        Scribe_Collections.Look(ref spawnedUniqueThings, "spawnedUniqueThings", LookMode.Def);
    }
}
