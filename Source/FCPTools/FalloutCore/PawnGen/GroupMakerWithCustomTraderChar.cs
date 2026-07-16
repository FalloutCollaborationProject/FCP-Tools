// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace FCP.Core;

public class GroupMakerWithCustomTraderChar : PawnGroupMaker
{
    public List<CharacterDef> characterDefs = [];
    public TraderKindDef traderKind;
}
