using HarmonyLib;

namespace FCP.TitleExtensions;

[StaticConstructorOnStartup]
public class TitleExtensionsPatcher
{
    static TitleExtensionsPatcher()
    {
        var harmony = new Harmony("com.FCP.TitleExtensions");
        harmony.PatchAll();
    }
}