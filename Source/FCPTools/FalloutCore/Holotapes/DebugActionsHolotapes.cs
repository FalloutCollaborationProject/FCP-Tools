using HarmonyLib;
using LudeonTK;
using RimWorld;
using Verse;

namespace FCP.Core.Holotapes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public static class DebugActionsHolotapes
{
    [DebugAction("FCP: Holotapes", "Spawn holotape", allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static List<DebugActionNode> SpawnHolotape()
    {
        var holotapes = DefDatabase<ThingDef>.AllDefsListForReading
            .Where(def => def.comps != null && def.comps.Any(c => c is CompProperties_Holotape))
            .OrderBy(def => def.label);

        return holotapes
            .Where(def => def != null)
            .Select(def =>
            {
                string label = def.label.NullOrEmpty() ? def.defName : def.label;
                return new DebugActionNode(label, DebugActionType.ToolMap, () =>
                {
                    var map = Find.CurrentMap;
                    if (map == null) return;
                    GenPlace.TryPlaceThing(ThingMaker.MakeThing(def), UI.MouseCell(), map, ThingPlaceMode.Near);
                });
            })
            .ToList();
    }
}
