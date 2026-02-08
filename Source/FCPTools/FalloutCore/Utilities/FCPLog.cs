namespace FCP.Core;

public static class FCPLog
{
    private const string ErrorPrefix = "<color=#7F66FFFF>[FCP Tools] </color>";
    private const string WarnPrefix = "<color=#B266FFFF>[FCP Tools] </color>";
    private const string MsgPrefix = "<color=#66ff7fFF>[FCP Tools] </color>";
    private const string VerbosePrefix = "<color=#66ccffFF>[FCP Verbose] </color>";

    private const int VerboseLogMax = 4000;
    private static int verboseCount;

    public static bool VerboseEnabled =>
        FCPCoreMod.SettingsTab<DebugSettings>()?.verboseLogging ?? false;

    public static void Error(string msg)
    {
        Log.Error(ErrorPrefix + msg);
    }

    public static void Warning(string msg)
    {
        Log.Warning(WarnPrefix + msg);
    }

    public static void Message(string msg)
    {
        Log.Message(MsgPrefix + msg);
    }

    public static void Verbose(string msg)
    {
        if (!VerboseEnabled || verboseCount++ >= VerboseLogMax) return;
        Log.Message(VerbosePrefix + msg);
    }
}