using UnityEngine;

namespace FCPVT;

public static class VTLog
{
    public static readonly Color ErrorMsgCol = new (0.4f, 0.54902f, 1.0f);
    public static readonly Color WarningMsgCol = new (0.70196f, 0.4f, 1.0f);
    public static readonly Color MessageMsgCol = new (0.4f, 1.0f, 0.54902f);

    public static void Error(string msg)
    {
        Log.Error("[FCP Vault Tec] ".Colorize(ErrorMsgCol) + msg);
    }

    public static void Warning(string msg)
    {
        Log.Warning("[FCP Vault Tec] ".Colorize(WarningMsgCol) + msg);
    }

    public static void Message(string msg)
    {
        Log.Message("[FCP Vault Tec] ".Colorize(MessageMsgCol) + msg);
    }
}
