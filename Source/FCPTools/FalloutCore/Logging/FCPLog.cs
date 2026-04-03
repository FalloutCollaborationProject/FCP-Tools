using System.Runtime.CompilerServices;
using FCP.Core;

namespace FCP.Core.Logging;

/// <summary>
/// Wraps <see cref="Verse.Log"/> with
/// color-coded prefixes and a verbose mode toggled in mod settings.
/// </summary>
public static class FCPLog
{
    private const string errorPrefix = "<color=#7F66FFFF>[FCP Tools] </color>";
    private const string warnPrefix = "<color=#B266FFFF>[FCP Tools] </color>";
    private const string msgPrefix = "<color=#66ff7fFF>[FCP Tools] </color>";
    private const string verbosePrefix = "<color=#66ccffFF>[FCP Verbose] </color>";

    internal const int VerboseLogMax = 4000;

    internal static int verboseCount;

    /// <summary>Whether verbose logging is currently enabled via Debug settings.</summary>
    public static bool VerboseEnabled =>
        FCPCoreMod.SettingsTab<DebugSettings>()?.verboseLogging ?? false;

    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Error(object msg) => Log.Error(errorPrefix + msg);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warning(object msg) => Log.Warning(warnPrefix + msg);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Message(object msg) => Log.Message(msgPrefix + msg);

    /// <summary>
    /// Logs a verbose message if verbose logging is enabled and below the session cap.
    /// When called with an interpolated string literal, the compiler prefers the
    /// Verbose(ref VerboseInterpolatedStringHandler) method. This overload handles pre-built strings.
    /// </summary>
    public static void Verbose(object msg)
    {
        if (!VerboseEnabled || verboseCount++ >= VerboseLogMax) return;
        Log.Message(verbosePrefix + msg);
    }

    /// <inheritdoc cref="Verbose(object)"/>
    /// <remarks>
    /// Handler overload — preferred by the compiler for <c>$"..."</c> literals.
    /// String should not be evaluated when verbose is disabled.
    /// </remarks>
    public static void Verbose(ref VerboseInterpolatedStringHandler handler)
    {
        if (!handler._isEnabled) return;
        verboseCount++;
        Log.Message(verbosePrefix + handler.GetResult());
    }
}

/// <summary>
/// Interpolated string handler for <see cref="FCPLog.Verbose(ref VerboseInterpolatedStringHandler)"/>.
/// When verbose logging is disabled, the compiler skips evaluating all hole expressions —
/// no string is built and no arguments are computed. This is a zero-cost abstraction over
/// the manual <c>if (FCPLog.VerboseEnabled) FCPLog.Verbose($"...")</c> guard pattern.
/// </summary>
[InterpolatedStringHandler]
public ref struct VerboseInterpolatedStringHandler
{
    private System.Text.StringBuilder _sb;

    /// <summary>Whether this handler instance is active (verbose enabled and below the cap).</summary>
    internal readonly bool _isEnabled;

    /// <param name="literalLength">Total character count of literal segments — used to pre-size the builder.</param>
    /// <param name="formattedCount">Number of interpolation holes — unused, required by the handler contract.</param>
    /// <param name="shouldAppend">
    /// Output parameter set to <c>true</c> when verbose logging is on and below the cap.
    /// The compiler uses this to gate all subsequent <c>Append*</c> calls, so hole
    /// expressions are never evaluated when it is <c>false</c>.
    /// </param>
    public VerboseInterpolatedStringHandler(int literalLength, int formattedCount, out bool shouldAppend)
    {
        _isEnabled = shouldAppend = FCPLog.VerboseEnabled && FCPLog.verboseCount < FCPLog.VerboseLogMax;
        _sb = _isEnabled ? new System.Text.StringBuilder(literalLength) : null;
    }

    // Appends a literal text segment from the interpolated string.
    public void AppendLiteral(string s) => _sb.Append(s);

    // Appends an interpolation hole value.
    public void AppendFormatted<T>(T value) => _sb.Append(value);

    /// Appends an interpolation hole value with a format string (e.g. {value:F2}).
    public void AppendFormatted<T>(T value, string format) where T : IFormattable
        => _sb.Append(value?.ToString(format, null));

    /// Returns the fully assembled log string.
    internal string GetResult() => _sb.ToString();
}