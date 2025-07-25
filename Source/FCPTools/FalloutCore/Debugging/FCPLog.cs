﻿namespace FCP.Core;

public static class FCPLog
{
    private const string ErrorPrefix = "<color=#7F66FFFF>[FCP Core/Tools] </color>";
    private const string WarnPrefix = "<color=#B266FFFF>[FCP Core/Tools] </color>";
    private const string MsgPrefix = "<color=#66ff7fFF>[FCP Core/Tools] </color>";
    
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