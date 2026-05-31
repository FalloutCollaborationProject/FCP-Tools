using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;

namespace FCP.Core.Holotapes
{
    [StaticConstructorOnStartup]
    public static class HolotapesHarmonyInit
    {
        static HolotapesHarmonyInit()
        {
            Log.Message("[FCP.Core] Holotapes Harmony patches initializing...");
            new HarmonyLib.Harmony("FCP.Core.Holotapes").PatchAll();
            Log.Message("[FCP.Core] Holotapes Harmony patches complete!");
        }
    }

    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public static class Pawn_GetGizmos_Patch
    {
        [HarmonyPostfix]
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            foreach (Gizmo gizmo in __result)
                yield return gizmo;
            
            if (!__instance.IsColonistPlayerControlled || __instance.apparel == null)
                yield break;
            
            Apparel pipboy = __instance.apparel.WornApparel.Find(a => a.TryGetComp<CompPipboyHolotapeStorage>() != null);
            if (pipboy == null)
                yield break;
            
            CompPipboyHolotapeStorage storage = pipboy.TryGetComp<CompPipboyHolotapeStorage>();
            if (storage == null)
                yield break;
            
            if (storage.Count > 0)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Pip-Boy Archive",
                    defaultDesc = "Access holotapes stored on this Pip-Boy (" + storage.Count + " holotape(s))",
                    icon = ContentFinder<Texture2D>.Get("Things/Items/Techprints/FCP_Techprint_Holotape_Orange", true),
                    action = () => Find.WindowStack.Add(new Dialog_HolotapeBrowser(storage, __instance))
                };
            }
            
            Thing carried = __instance.carryTracker?.CarriedThing;
            if (carried != null && carried.TryGetComp<CompHolotape>() != null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Load holotape",
                    defaultDesc = "Load " + carried.Label + " into Pip-Boy storage",
                    icon = ContentFinder<Texture2D>.Get("Things/Items/Techprints/FCP_Techprint_Holotape_Orange", true),
                    action = () =>
                    {
                        if (storage.TryStoreHolotape(carried))
                            Messages.Message(__instance.LabelShort + " loaded " + carried.Label + " into Pip-Boy.", __instance, MessageTypeDefOf.NeutralEvent);
                    }
                };
            }
        }
    }
}
