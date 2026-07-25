using FCP.Core.Robotics;

namespace FCP.Core;

[UsedImplicitly]
public class CharacterRobotLoadoutDefinition : CharacterBaseDefinition
{
    public string presetId;

    public override bool AppliesPreGeneration => false;
    public override bool AppliesPostGeneration => !presetId.NullOrEmpty();

    public override void ApplyToPawn(Pawn pawn)
    {
        pawn.GetComp<CompProtectronLoadout>()?.ApplyPreset(presetId);
    }
}
