using HarmonyLib;
using Verse;

namespace BM_HeavyWeapon
{
    public class BM_HeavyWeaponMod : Mod
    {
        public BM_HeavyWeaponMod(ModContentPack pack) : base(pack)
        {
			new Harmony("BM_HeavyWeaponMod").PatchAll();
        }
    }
}
