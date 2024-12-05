// ReSharper disable UnassignedField.Global
namespace FCP.Core;

public class CharacterRole
{
    public PawnKindDef changePawnKind;

    public virtual bool PawnIsValid(Pawn pawn)
    {
        return true;
    }
    
    public virtual void ApplyRole(Pawn pawn)
    {
        if (changePawnKind != null)
        {
            pawn.ChangeKind(changePawnKind);
        }
    }
}