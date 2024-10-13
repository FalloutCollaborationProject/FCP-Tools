namespace FCP.Core.PawnGen;

public abstract class PawnGenerationDefinition
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