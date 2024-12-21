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
    /// Try find a matching UniqueCharacter for a given pawn
    /// </summary>
    public bool TryGetCharacterFromPawn(Pawn pawn, out UniqueCharacter character)
    {
        character = characters.Find(chr => chr.pawn == pawn);
        return character != null;
    }

    /// <summary>
    /// Retrieve or create a UniqueCharacter for a given CharacterDef.
    /// </summary>
    public UniqueCharacter GetUniqueCharacter(CharacterDef charDef)
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
    public Pawn GetOrGenPawn(CharacterDef charDef, PawnGenerationRequest? baseParams = null, 
        Func<PawnGenerationRequest, PawnGenerationRequest> modifyGenRequest = null)
    {
        UniqueCharacter character = GetUniqueCharacter(charDef);
        return character.EnsurePawnExists(baseParams, modifyGenRequest);
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