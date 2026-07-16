namespace FCP.Core;

[UsedImplicitly]
public class CharacterRobotNameDefinition : CharacterBaseDefinition
{
    public string name;

    public override bool AppliesPreGeneration => false;
    public override bool AppliesPostGeneration => !name.NullOrEmpty();

    public override void ApplyToPawn(Pawn pawn)
    {
        pawn.Name = new NameSingle(name);
    }
}
