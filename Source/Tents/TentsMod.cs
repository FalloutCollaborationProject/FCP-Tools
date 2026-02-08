using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace FCP.Tents;

public class TentsMod : Mod
{
    public TentsMod(ModContentPack content) : base(content)
    {
        GetSettings<ModSettings>();
        var harmony = new Harmony("FCP.Tents");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
}