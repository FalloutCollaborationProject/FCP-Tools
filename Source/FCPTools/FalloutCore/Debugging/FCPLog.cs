namespace FCP.Core;

public static class FCPLog
{
    private const string ErrorPrefix = "<color=#7F66FF>[FCP Core] </color>";
    private const string WarnPrefix = "<color=#B266FF>[FCP Core] </color>";
    private const string MsgPrefix = "<color=##66ff7f>[FCP Core] </color>";
    
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
}