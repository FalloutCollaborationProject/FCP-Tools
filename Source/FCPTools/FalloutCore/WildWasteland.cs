using LudeonTK;
using RimWorld;
using Verse;
using Verse.Sound;

namespace FCP.Core;

public static class WildWasteland
{
    public static WildWastelandType Type => WildWastelandType.Wilder;
    public static void PlaySound(Map map = null) => FCPDefOf.FCP_Sound_WildWasteland.PlayOneShotOnCamera(map);

    public static void DoMessage(LookTargets lookTargets = null)
    {
        PlaySound();
        Messages.Message("...", lookTargets, MessageTypeDefOf.SilentInput);
    }
}

public static class WildWastelandDebugActions
{
    
    [DebugAction("FCP", "Wild Wasteland Message", actionType = DebugActionType.Action,
        allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void WildWastelandPlaySound() => WildWasteland.DoMessage();
}

public enum WildWastelandType
{
    None,
    Wild,
    Wilder,
}