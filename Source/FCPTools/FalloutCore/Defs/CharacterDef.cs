// ReSharper disable UnassignedField.Global
namespace FCP.Core;

[UsedImplicitly]
public class CharacterDef : Def
{
    public PawnKindDef pawnKind;
    public XenotypeDef xenotype;
    public List<CharacterBaseDefinition> definitions;
}