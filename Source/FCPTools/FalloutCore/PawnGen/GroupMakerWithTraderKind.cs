
namespace FCP.Core;

public class GroupMakerWithTraderKind : PawnGroupMaker
{
    public List<TraderKindDef> traderKinds = [];
    public List<CharacterDef> characterDefs = [];
    public Dictionary<CharacterDef, TraderKindDef> characterTraderKinds = new();
    public float characterChance = 1f;
}
