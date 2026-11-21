using FCP.Ranks;

namespace FCP.Core;

[UsedImplicitly]
public class CharacterRankDefinition : CharacterBaseDefinition
{
    public RankTierDef tier;

    public override bool AppliesPreGeneration => false;
    public override bool AppliesPostGeneration => false;
}
