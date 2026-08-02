namespace FCP.Core;

[UsedImplicitly]
public class CharacterHediffDefinition : CharacterBaseDefinition
{
    public HediffDef hediffDef;
    public BodyPartDef part;

    public override bool AppliesPreGeneration => false;
    public override bool AppliesPostGeneration => hediffDef != null;

    public override void ApplyToPawn(Pawn pawn)
    {
        if (pawn.health.hediffSet.HasHediff(hediffDef))
        {
            return;
        }

        BodyPartRecord bodyPart = part != null
            ? pawn.health.hediffSet.GetNotMissingParts().FirstOrFallback(p => p.def == part)
            : null;

        pawn.health.AddHediff(hediffDef, bodyPart);
    }
}
