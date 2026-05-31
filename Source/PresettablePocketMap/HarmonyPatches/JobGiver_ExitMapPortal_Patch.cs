using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using Verse.AI;

namespace FCP.PocketMaps
{
    [HarmonyPatch]
    public static class PocketMapExitPatch
    {
        private static MethodBase targetMethod;
        
        public static bool Prepare()
        {
            targetMethod = GenTypes.AllTypes
                .Where(t => t.Name == "JobGiver_ExitMapPortal")
                .Select(t => AccessTools.Method(t, "TryGiveJob"))
                .FirstOrDefault(m => m != null);
            
            return targetMethod != null;
        }
        
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            if (targetMethod != null)
                yield return targetMethod;
        }

        public static void Postfix(Pawn pawn, ref Job __result)
        {
            if (__result == null || pawn?.Map == null)
                return;
            
            var comp = pawn.Map.GetComponent<MapComponent_PocketMapEntrance>();
            if (comp?.portal == null)
                return;
            
            var exitCell = comp.portal.InteractionCell;
            if (!exitCell.IsValid)
                return;
            
            __result.targetA = exitCell;
        }
    }
}
