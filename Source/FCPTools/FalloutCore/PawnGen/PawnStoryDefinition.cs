// ReSharper disable UnassignedField.Global

namespace FCP.Core;

[UsedImplicitly]
public class PawnStoryDefinition : PawnGenerationDefinition
{
    public string firstName;
    public string lastName;
    public string nickname;

    public Gender? gender = null;
    public float? age = null;
    public float? chronologicalAge = null;

    public override bool AppliesPreGeneration => 
        gender != null || age != null || chronologicalAge != null;

    public override bool AppliesPostGeneration =>
        !firstName.NullOrEmpty() || !lastName.NullOrEmpty() || !nickname.NullOrEmpty();

    public override void ApplyToRequest(ref PawnGenerationRequest request)
    {
        request.FixedGender = gender ?? request.FixedGender;
        request.FixedBiologicalAge = age ?? request.FixedBiologicalAge;
        request.FixedChronologicalAge = chronologicalAge ?? age ?? request.FixedChronologicalAge;
    }

    public override void ApplyToPawn(Pawn pawn)
    {
        if (pawn.Name is NameTriple oldName)
        {
            pawn.Name = new NameTriple(firstName ?? oldName.First, nickname, lastName ?? oldName.Last);
        }
    }
}