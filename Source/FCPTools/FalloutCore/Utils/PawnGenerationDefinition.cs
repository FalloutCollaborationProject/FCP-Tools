using JetBrains.Annotations;
using Verse;

namespace FCP.Core.Utils;

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