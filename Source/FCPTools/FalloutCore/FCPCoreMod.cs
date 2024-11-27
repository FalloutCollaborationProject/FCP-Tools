namespace FCP.Core;

public class FCPCoreMod : Mod
{
    public FCPCoreMod(ModContentPack content) : base(content)
    {
        FCPLog.Message("Welcome home...");
        FCPLog.Error("This is an unstable beta version and bugs are very likely, if not guaranteed!\nRemember to report bugs on steam page or our discord: 3HEXN3Qbn4");
    }
}