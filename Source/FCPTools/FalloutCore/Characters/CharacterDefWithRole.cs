namespace FCP.Core;

public class CharacterDefWithRole<TRole> where TRole : CharacterRole
{
    public CharacterDef characterDef;
    public TRole role;

    public CharacterDefWithRole(CharacterDef characterDef, TRole role)
    {
        this.characterDef = characterDef;
        this.role = role;
    }
}