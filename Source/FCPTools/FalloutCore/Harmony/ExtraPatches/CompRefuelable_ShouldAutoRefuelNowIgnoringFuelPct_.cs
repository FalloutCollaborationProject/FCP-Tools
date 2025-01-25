// Decompiled with JetBrains decompiler
// Type: RangerRick_PowerArmor.CompRefuelable_ShouldAutoRefuelNowIgnoringFuelPct_Patch
// Assembly: RangerRick_PowerArmor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2F9A23E8-63EA-4090-93FB-DECA3711786E
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\FCP-Tools\1.5\Assemblies\RangerRick_PowerArmor.dll

using FCP.Core;
using HarmonyLib;
using RimWorld;

#nullable disable
namespace FCP.Core
{
    [HarmonyPatch]
    public static class CompRefuelable_ShouldAutoRefuelNowIgnoringFuelPct_Patch
    {
        public static bool Prefix(CompRefuelable __instance, ref bool __result)
        {
            if (__instance.parent.Spawned || __instance.parent.GetComp<CompPowerArmor>() == null)
                return true;
            __result = !__instance.parent.IsBurning();
            return false;
        }
    }
}
