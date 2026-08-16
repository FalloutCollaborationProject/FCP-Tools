namespace FCP.Core;

[UsedImplicitly]
public class CharacterRelation
{
    public PawnRelationDef relation;
    public CharacterDef otherCharacter;
}

// Declares social relations (Parent, Child, Spouse, Sibling, etc.) between this character and
// another unique CharacterDef. Only declare a relationship from ONE side - this automatically
// adds the matching opposite relation to the other pawn directly, so declaring it on both
// characters would double up (and for Parent/Child specifically, could recurse forever through
// UniqueCharactersTracker.GetOrGenPawn's "reapply definitions" cache-hit path).
[UsedImplicitly]
public class CharacterRelationDefinition : CharacterBaseDefinition
{
    public List<CharacterRelation> relations = [];

    public override bool AppliesPreGeneration => false;
    public override bool AppliesPostGeneration => !relations.NullOrEmpty();

    public override void ApplyToPawn(Pawn pawn)
    {
        foreach (CharacterRelation rel in relations)
        {
            if (rel?.relation == null || rel.otherCharacter == null)
                continue;

            Pawn otherPawn = UniqueCharactersTracker.Instance.GetOrGenPawn(rel.otherCharacter);
            if (otherPawn == null || otherPawn == pawn)
                continue;

            if (!pawn.relations.DirectRelationExists(rel.relation, otherPawn))
                pawn.relations.AddDirectRelation(rel.relation, otherPawn);

            PawnRelationDef opposite = OppositeOf(rel.relation);
            if (opposite != null && !otherPawn.relations.DirectRelationExists(opposite, pawn))
                otherPawn.relations.AddDirectRelation(opposite, pawn);
        }
    }

    private static PawnRelationDef OppositeOf(PawnRelationDef def)
    {
        if (def == PawnRelationDefOf.Parent) return PawnRelationDefOf.Child;
        if (def == PawnRelationDefOf.Child) return PawnRelationDefOf.Parent;
        if (def == PawnRelationDefOf.Spouse) return PawnRelationDefOf.Spouse;
        if (def == PawnRelationDefOf.Fiance) return PawnRelationDefOf.Fiance;
        if (def == PawnRelationDefOf.Lover) return PawnRelationDefOf.Lover;
        if (def == PawnRelationDefOf.Sibling) return PawnRelationDefOf.Sibling;
        if (def == PawnRelationDefOf.ExSpouse) return PawnRelationDefOf.ExSpouse;
        if (def == PawnRelationDefOf.ExLover) return PawnRelationDefOf.ExLover;
        return null;
    }
}
