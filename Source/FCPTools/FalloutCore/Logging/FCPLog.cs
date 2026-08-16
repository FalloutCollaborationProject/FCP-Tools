using System.Runtime.CompilerServices;
using FCP.Core;

namespace FCP.Core.Logging;

public static class FCPLog
{
    private const string errorPrefix = "<color=#7F66FFFF>[FCP Tools] </color>";
    private const string warnPrefix = "<color=#B266FFFF>[FCP Tools] </color>";
    private const string msgPrefix = "<color=#66ff7fFF>[FCP Tools] </color>";
    private const string verbosePrefix = "<color=#66ccffFF>[FCP Verbose] </color>";

    internal const int VerboseLogMax = 4000;

    internal static int verboseCount;

    public static bool VerboseEnabled =>
        FCPCoreMod.SettingsTab<DebugSettings>()?.verboseLogging ?? false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Error(object msg) => Log.Error(errorPrefix + msg);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warning(object msg) => Log.Warning(warnPrefix + msg);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Message(object msg) => Log.Message(msgPrefix + msg);

    public static void Verbose(object msg)
    {
        if (!VerboseEnabled || verboseCount++ >= VerboseLogMax) return;
        Log.Message(verbosePrefix + msg);
    }

    public static void Verbose(ref VerboseInterpolatedStringHandler handler)
    {
        if (!handler._isEnabled) return;
        verboseCount++;
        Log.Message(verbosePrefix + handler.GetResult());
    }
}

[InterpolatedStringHandler]
public ref struct VerboseInterpolatedStringHandler
{
    private System.Text.StringBuilder _sb;

    internal readonly bool _isEnabled;

    public VerboseInterpolatedStringHandler(int literalLength, int formattedCount, out bool shouldAppend)
    {
        _isEnabled = shouldAppend = FCPLog.VerboseEnabled && FCPLog.verboseCount < FCPLog.VerboseLogMax;
        _sb = _isEnabled ? new System.Text.StringBuilder(literalLength) : null;
    }

    public void AppendLiteral(string s) => _sb.Append(s);

    public void AppendFormatted<T>(T value) => _sb.Append(value);

    public void AppendFormatted<T>(T value, string format) where T : IFormattable
        => _sb.Append(value?.ToString(format, null));

    internal string GetResult() => _sb.ToString();
}
