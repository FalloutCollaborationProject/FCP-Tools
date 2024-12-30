using System.Runtime.InteropServices;
using UnityEngine;

namespace FCP.Core;

public class FCPCoreMod : Mod
{
    public static FCPCoreMod mod;
    
    public static FCP_Settings Settings;
    
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

    public AssetBundle bundleInt;

    public AssetBundle MainBundle
    {
        get
        {
            if(bundleInt != null) return bundleInt;

            string text = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                text = "StandaloneOSX";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                text = "StandaloneWindows64";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                text = "StandaloneLinux64";
            }

            string bundlePath = Path.Combine(Content.RootDir, $@"FCP-UnityAssets\Materials\{text}\fcpshaders");
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