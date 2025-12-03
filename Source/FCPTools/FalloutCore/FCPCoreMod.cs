using System.Runtime.InteropServices;
using HarmonyLib;
using UnityEngine;

namespace FCP.Core;

public class FCPCoreMod : Mod
{
    public static FCPCoreMod mod;
    public static FCP_Settings Settings;
    public static Harmony harmony;
    public AssetBundle bundleInt;
    
    public FCPCoreMod(ModContentPack content) : base(content)
    {
        harmony = new Harmony("FCP.Core.Patches"); // PatchesUwU ~ Steve
        PatchAll();
        mod = this;
        Settings = GetSettings<FCP_Settings>();
        FCPLog.Warning("Beta version: bugs likely, if not guaranteed! " +
                       "Report bugs on steam workshop page or on discord: 3HEXN3Qbn4");
    }

    public static void PatchAll()
    {
        AccessTools.GetTypesFromAssembly(typeof(FCPCoreMod).Assembly).Do(delegate (Type type)
        {
            try
            {
                // Only process types that have HarmonyPatch attributes
                if (type.GetCustomAttributes(typeof(HarmonyPatch), true).Any())
                {
                    harmony.CreateClassProcessor(type).Patch();
                }
            }
            catch (Exception e)
            {
                Log.Error("Error patching " + type + " - " + e.ToString());
            }
        });
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
            //FCPLog.Message("Bundle Path: " + bundlePath);

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