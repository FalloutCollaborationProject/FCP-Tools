using System.Reflection;
using HarmonyLib;
using RimWorld.Planet;

namespace FCP.Core.Shuttles;

public class CompProperties_AtmosphericOnly : CompProperties
{
    public CompProperties_AtmosphericOnly()
    {
        compClass = typeof(CompAtmosphericOnly);
    }
}

[HotSwappable]
public class CompAtmosphericOnly : ThingComp
{
    private static FieldInfo isOrbitTileField;
    private static FieldInfo tileField;
    
    static CompAtmosphericOnly()
    {
        isOrbitTileField = AccessTools.Field(typeof(WorldObjectDef), "isOrbitTile");
        
        var planetTileType = AccessTools.TypeByName("RimWorld.Planet.PlanetTile");
        if (planetTileType != null)
        {
            tileField = AccessTools.Field(planetTileType, "tileId");
        }
    }
    
    public bool IsSpaceTile(object planetTile)
    {
        if (planetTile == null) return false;
        
        int tile = -1;
        if (tileField != null)
        {
            var tileValue = tileField.GetValue(planetTile);
            if (tileValue is int t) tile = t;
        }
        
        if (tile < 0 || !Find.WorldGrid.InBounds(tile)) return false;
        
        var worldObjects = Find.WorldObjects.ObjectsAt(tile);
        foreach (var obj in worldObjects)
        {
            if (isOrbitTileField != null)
            {
                var isOrbit = isOrbitTileField.GetValue(obj.def);
                if (isOrbit is bool b && b) return true;
            }
            
            var defName = obj.def.defName;
            if (defName.Contains("Asteroid") || defName.Contains("Orbit") || 
                defName.Contains("Planetoid") || defName.Contains("Space"))
            {
                return true;
            }
        }
        
        return false;
    }
}

[StaticConstructorOnStartup]
public static class CompAtmosphericOnly_Patches
{
    private static FieldInfo parentField;
    
    static CompAtmosphericOnly_Patches()
    {
        var harmony = new HarmonyLib.Harmony("fcp.core.atmosphericonly");
        
        var compLaunchableType = AccessTools.TypeByName("RimWorld.CompLaunchable");
        if (compLaunchableType != null)
        {
            parentField = AccessTools.Field(compLaunchableType, "parent");
            
            var tryLaunchMethod = AccessTools.Method(compLaunchableType, "TryLaunch");
            if (tryLaunchMethod != null)
            {
                harmony.Patch(tryLaunchMethod, prefix: new HarmonyMethod(typeof(CompAtmosphericOnly_Patches), nameof(TryLaunch_Prefix)));
            }
        }
    }

    public static bool TryLaunch_Prefix(object __instance, object __0, object __1)
    {
        if (parentField == null) return true;
        
        var parent = parentField.GetValue(__instance) as Thing;
        if (parent == null) return true;
        
        var comp = parent.TryGetComp<CompAtmosphericOnly>();
        if (comp == null) return true;
        
        if (__1 != null)
        {
            var typeName = __1.GetType().Name;
            if (typeName.Contains("VisitSpace") || typeName.Contains("Orbit") || 
                typeName.Contains("Asteroid") || typeName.Contains("Planetoid"))
            {
                Messages.Message("FCP_CannotLaunchToOrbit".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }
        }
        
        if (comp.IsSpaceTile(__0))
        {
            Messages.Message("FCP_CannotLaunchToOrbit".Translate(), MessageTypeDefOf.RejectInput, false);
            return false;
        }
        
        return true;
    }
}
