namespace FCP.Core;

public abstract class CharacterBaseDefinition
{
    public abstract bool AppliesPreGeneration { get; }
    public abstract bool AppliesPostGeneration { get; }

    public virtual void ApplyToPawn(Pawn pawn)
    {
    }

    public virtual void ApplyToRequest(ref PawnGenerationRequest request)
    {
    }
}