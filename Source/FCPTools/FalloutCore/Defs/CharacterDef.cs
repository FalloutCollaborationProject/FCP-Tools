// ReSharper disable UnassignedField.Global
namespace FCP.Core;

[UsedImplicitly]
public class CharacterDef : Def
{
    public PawnKindDef pawnKind;
    public XenotypeDef xenotype;
    public FactionDef faction;
    [UsedImplicitly] public List<CharacterBaseDefinition> definitions = [];
    [UsedImplicitly] public List<CharacterRole> roles = [];
}