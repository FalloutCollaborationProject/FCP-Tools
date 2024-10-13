using System.Reflection;
using HarmonyLib;

namespace FCP.Factions;

public class PermanentlyHostileToExtension : DefModExtension
{
    [UsedImplicitly] 
    public List<FactionDef> hostileFactionDefs;

    public bool IsHostileTo(FactionDef other) => hostileFactionDefs.Contains(other);
}

/// <summary>
/// Patches goodwill and hostility related stuff for the PermanentlyHostileToExtension
/// </summary>
[HarmonyPatch]
public static class PermanentlyHostileToPatches
{

    /// <summary>
    /// Patch to change the ArePermanentEnemies result to true if they are permanent enemies because of the extension.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GoodwillSituationWorker_PermanentEnemy), "ArePermanentEnemies")]
    public static void GoodwillSituationWorker_PermanentEnemy_ArePermanentEnemies_Postfix(Faction a, Faction b, 
        ref bool __result)
    {
        if (__result == true)
            return;
        
        var aExtension = a.def.GetModExtension<PermanentlyHostileToExtension>();
        var bExtension = a.def.GetModExtension<PermanentlyHostileToExtension>();

        // Check if either are permanently hostile with each other, but if both are null just use the existing result (false)
        __result = aExtension?.IsHostileTo(b.def) ??
                   bExtension?.IsHostileTo(a.def) ?? 
                   __result;
    }
    
    /// <summary>
    /// Patch so that they are unable to change goodwill after the start of the game.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Faction), nameof(Faction.CanChangeGoodwillFor))]
    public static void Faction_CanChangeGoodwillFor_Postfix(Faction other, Faction __instance, ref bool __result)
    {
        if (__result == false)
            return;

        var extension = __instance.def.GetModExtension<PermanentlyHostileToExtension>();
        if (extension == null)
            return;

        __result = !extension.IsHostileTo(other.def);
    }
    
    /// <summary>
    /// I think this is used in some quests
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(FactionDef), nameof(FactionDef.PermanentlyHostileTo))]
    public static void FactionDef_PermanentlyHostileTo_Postfix(FactionDef otherFactionDef, FactionDef __instance, ref bool __result)
    {
        if (__result == true)
            return;

        var extension = __instance.GetModExtension<PermanentlyHostileToExtension>();
        __result = extension?.IsHostileTo(otherFactionDef) ?? false;
    }
    
    /// <summary>
    /// Patches the initial goodwill which is run on faction generation or reset
    /// to return -100 if they have this extension and are in the list.
    /// </summary>
    [HarmonyPatch]
    public static class Faction_TryMakeInitialRelationsWith_GetInitialGoodwill_Patch
    {
        public static MethodBase TargetMethod()
        {
            // Local Method "GetInitialGoodwill" inside of Faction.TryMakeInitialRelationsWith() 
            var method = typeof(Faction).GetDeclaredMethods().FirstOrDefault(mi => mi.Name.Contains("GetInitialGoodwill"));
            if (method == null)
            {
                Log.Error("FCPTools couldn't find the local method GetInitialGoodwill that is defined inside Faction.TryMakeInitialRelationsWith");
            }
            return method;
        }

        public static bool Prefix(Faction a, Faction b, ref int __result)
        {
            var extension = a.def.GetModExtension<PermanentlyHostileToExtension>();
            if (extension == null || extension.IsHostileTo(b.def) == false)
            {
                return true;
            }
            
            // They're hostile, so set to -100 and skip the original.
            __result = -100;
            return false;
        }
    }
}