using System.Runtime.InteropServices;
using UnityEngine;

namespace FCP.Core;

[UsedImplicitly]
public class FCPCoreMod : Mod
{
    public static FCPCoreMod mod;

    public static FCP_Settings Settings { get; private set; }

    public FCPCoreMod(ModContentPack content) : base(content)
    {
        mod = this;
        Settings = GetSettings<FCP_Settings>();
        FCPLog.Message("Welcome home...");
        FCPLog.Error("This is an unstable beta version and bugs are very likely, if not guaranteed!\nRemember to report bugs on steam page or our discord: 3HEXN3Qbn4");
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        base.DoSettingsWindowContents(inRect);
        Settings.DoWindowContents(inRect);
    }

    public override string SettingsCategory()
    {
        return "FCP_Settings_Category".Translate();
    }

    private AssetBundle bundleInt;

    public AssetBundle MainBundle
    {
        get
        {
            if(bundleInt != null) return bundleInt;

            string platform = "";
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                platform = "StandaloneOSX";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                platform = "StandaloneWindows64";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                platform = "StandaloneLinux64";
            }

            string bundlePath = Path.Combine(Content.RootDir, $@"AssetBundles\{platform}\fcpshaders");
            Log.Message("Bundle Path: " + bundlePath);

            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);

            if (bundle == null)
            {
                FCPLog.Error("Failed to load bundle at path: " + bundlePath);
            }

            bundleInt = bundle;

            return bundleInt;
        }
    }
}