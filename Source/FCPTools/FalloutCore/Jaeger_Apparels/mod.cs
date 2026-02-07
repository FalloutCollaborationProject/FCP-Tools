using HarmonyLib;
using Verse;

namespace FCP.Core.TemperatureApparelPreference
{
    public class Mod : Verse.Mod
    {
        public static Mod Instance;
        public Settings Settings;

        private bool patched;

        public Mod(ModContentPack content) : base(content)
        {
            Instance = this;
            Settings = GetSettings<Settings>();

            // Patch after defs/DefOfs are loaded; patching too early can trigger type initializers prematurely.
            LongEventHandler.ExecuteWhenFinished(EnsurePatched);
        }

        public override string SettingsCategory()
        {
            return "Temperature Apparel Preference";
        }

        public override void DoSettingsWindowContents(UnityEngine.Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Settings.DoWindowContents(inRect);
        }

        private void EnsurePatched()
        {
            if (patched) return;
            patched = true;

            var harmony = FCPCoreMod.Harmony;
            harmony.Patch(original: AccessTools.Method(typeof(PawnApparelGenerator), "CanUsePair"), prefix: new HarmonyMethod(typeof(TemperatureApparelPreferencePatches), nameof(TemperatureApparelPreferencePatches.Patch_PawnApparelGenerator_CanUsePair)));
        }
    }
}
