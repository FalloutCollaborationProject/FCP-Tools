using HarmonyLib;
using Verse;

namespace FCP_Shuttles
{
    public class FCP_ShuttlesMod : Mod
    {
        public FCP_ShuttlesMod(ModContentPack pack) : base(pack)
        {
            new Harmony("FCP_ShuttlesMod").PatchAll();
        }
    }
}