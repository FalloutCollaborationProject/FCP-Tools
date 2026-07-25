namespace FCP.Core;

[UsedImplicitly]
public class CharacterAnimalCompanionDefinition : CharacterBaseDefinition
{
    public CharacterDef animalCharacterDef;

    public override bool AppliesPreGeneration => false;
    public override bool AppliesPostGeneration => animalCharacterDef != null;

    public override void ApplyToPawn(Pawn pawn)
    {
        Pawn animal = UniqueCharactersTracker.Instance.GetOrGenPawn(animalCharacterDef);
        if (animal.Dead)
        {
            return;
        }

        if (animal.playerSettings != null)
        {
            animal.playerSettings.Master = pawn;
        }

        if (!pawn.relations.DirectRelationExists(PawnRelationDefOf.Bond, animal))
        {
            pawn.relations.AddDirectRelation(PawnRelationDefOf.Bond, animal);
        }
    }
}
