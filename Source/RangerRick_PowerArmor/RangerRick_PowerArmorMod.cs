using HarmonyLib;

namespace RangerRick_PowerArmor
{
    public class RangerRick_PowerArmorMod : Mod
    {
        public RangerRick_PowerArmorMod(ModContentPack pack) : base(pack)
        {
			new Harmony("RangerRick_PowerArmorMod").PatchAll();
        }
    }

}
