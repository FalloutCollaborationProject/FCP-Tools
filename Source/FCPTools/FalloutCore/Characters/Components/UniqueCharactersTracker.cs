using RimWorld.Planet;

namespace FCP.Core;

[UsedImplicitly]
public class UniqueCharactersTracker : WorldComponent
{
    public static UniqueCharactersTracker Instance { get; private set; }

    private List<UniqueCharacter> characters = [];
    private HashSet<ThingDef> spawnedUniqueThings = [];

    private readonly Dictionary<CharacterDef, UniqueCharacter> charactersByDef = new Dictionary<CharacterDef, UniqueCharacter>();
    private readonly Dictionary<Pawn, UniqueCharacter> charactersByPawn = new Dictionary<Pawn, UniqueCharacter>();

    public UniqueCharactersTracker(World world) : base(world)
    {
        Instance = this;
    }

    public bool CharacterPawnExists(CharacterDef charDef)
    {
        return charactersByDef.TryGetValue(charDef, out var character) && character.PawnExists();
    }

    public bool CharacterPawnExistsAlive(CharacterDef charDef)
    {
        return charactersByDef.TryGetValue(charDef, out var character) && character.PawnExists() && !character.pawn.Dead;
    }

    public bool CharacterPawnDead(CharacterDef charDef)
    {
        if (!charactersByDef.TryGetValue(charDef, out var character))
            return true;
        return character.pawn == null || character.pawn is { Dead: true };
    }

    public bool CharacterPawnSpawned(CharacterDef charDef)
    {
        return charactersByDef.TryGetValue(charDef, out var character) && character.PawnExists() && character.pawn.Spawned;
    }

    public bool TryGetPawnCharacter(Pawn pawn, out UniqueCharacter character)
    {
        return charactersByPawn.TryGetValue(pawn, out character);
    }

    public bool IsUniquePawn(Pawn pawn)
    {
        return TryGetPawnCharacter(pawn, out _);
    }

    public bool TryGetExistingPawn(CharacterDef charDef, out Pawn pawn)
    {
        if (charactersByDef.TryGetValue(charDef, out var character) && character.PawnExists())
        {
            pawn = character.pawn;
            return true;
        }

        pawn = null;
        return false;
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
        if (!charactersByDef.TryGetValue(charDef, out var character))
        {
            character = new UniqueCharacter(charDef);
            characters.Add(character);
            charactersByDef[charDef] = character;
        }
        else if (character.PawnExists())
        {
            FCPLog.Warning($"GetOrGenPawn: {charDef.defName} cache hit, reapplying definitions to {character.pawn}");
            CharacterDefinitionUtils.ApplyPawnDefinitions(character.pawn, charDef.definitions);
            return character.pawn;
        }

#if DEBUG
        FCPLog.Message($"Generating Unique Pawn: {charDef.defName}");
#endif

        PawnGenerationRequest request = requestParams ?? new PawnGenerationRequest(charDef.pawnKind);
        request.KindDef ??= charDef.pawnKind;
        request.Faction ??= Find.FactionManager.FirstFactionOfDef(charDef.faction);
        request.ForceGenerateNewPawn = true;

        FCPLog.Warning($"GetOrGenPawn: {charDef.defName} generating fresh pawn ({charDef.definitions.Count} definitions)");
        CharacterDefinitionUtils.ApplyRequestDefinitions(ref request, charDef.definitions);
        character.pawn = PawnGenerator.GeneratePawn(request);

        if (charDef.xenotype != null)
            character.pawn.genes?.SetXenotype(charDef.xenotype);

        CharacterDefinitionUtils.ApplyPawnDefinitions(character.pawn, charDef.definitions);

        charactersByPawn[character.pawn] = character;

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
