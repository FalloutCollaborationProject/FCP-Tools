using FCP.Ranks;

namespace FCP.Core;

// Purely a data holder: `tier` is read directly by MainTabWindow_RankRoster to label notable
// characters (e.g. "Arthur Maxson - Elder") in the Lineage tab. It must NOT enroll the pawn into
// GameComponent_PawnRanks - that dictionary represents the player's own colonists' rank progress,
// and every faction leader/notable NPC generated during world gen would otherwise get silently
// added to it the moment their pawn exists, regardless of the player or scenario.
[UsedImplicitly]
public class CharacterRankDefinition : CharacterBaseDefinition
{
    public RankTierDef tier;

    public override bool AppliesPreGeneration => false;
    public override bool AppliesPostGeneration => false;
}
