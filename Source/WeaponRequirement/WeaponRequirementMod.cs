using HarmonyLib;
using Verse;

namespace WeaponRequirement
{
    public class WeaponRequirementMod : Mod
    {
        public WeaponRequirementMod(ModContentPack pack) : base(pack)
        {
            new Harmony("WeaponRequirementMod").PatchAll();
        }
    }
}
