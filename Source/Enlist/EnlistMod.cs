using HarmonyLib;
using Verse;

namespace FCP.Enlist;

[StaticConstructorOnStartup]
static class EnlistMod
{
    static EnlistMod()
    {
        new Harmony("ChickenPlucker.Enlist").PatchAll();
    }
}
