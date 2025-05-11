using Verse;
using HarmonyLib;

namespace FCP_Ghoul
{
    public class HarmonyConstructor : Mod
    {
        public HarmonyConstructor(ModContentPack content) : base(content)
        {
            new Harmony("FCP_Ghoul").PatchAll();
        }
    }

}
