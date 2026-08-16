namespace FCP.Core;

public class UniqueCharacter : IExposable
{
    public CharacterDef def;
    public Pawn pawn;

    public UniqueCharacter() { }
    public UniqueCharacter(CharacterDef def)
    {
        this.def = def;
    }
    
    public bool PawnExists()
    {
        if (pawn == null) return false;
        return !pawn.Discarded;
    }

    public void ExposeData()
    {
        Scribe_Defs.Look(ref def, "def");
        Scribe_References.Look(ref pawn, "pawn");
    }   
}