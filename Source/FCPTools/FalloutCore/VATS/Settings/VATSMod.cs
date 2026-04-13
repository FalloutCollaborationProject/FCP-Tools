using System.Runtime.InteropServices;
using UnityEngine;

namespace FCP.Core.VATS;

[UsedImplicitly]
public class VATSMod : Mod
{
    public static VATSMod Instance { get; private set; }

    public VATSMod(ModContentPack content) : base(content)
    {
        Instance = this;
    }

    private AssetBundle mainBundle;
    public AssetBundle MainBundle
    {
        get
        {
            if (mainBundle != null) return mainBundle;

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

            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);

            if (bundle == null)
            {
                FCPLog.Error("Failed to load bundle at path: " + bundlePath);
            }

            mainBundle = bundle;

            return mainBundle;
        }
    }
}
