using HarmonyLib;
using RimWorld;
using Verse;

namespace taranchuk_homingprojectiles
{
    public class taranchuk_homingprojectilesMod : Mod
    {
        public taranchuk_homingprojectilesMod(ModContentPack pack) : base(pack)
        {
			new Harmony("taranchuk_homingprojectilesMod").PatchAll();
        }
    }
}
