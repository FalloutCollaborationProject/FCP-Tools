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

        return holotapes.Select(def =>
            new DebugActionNode(def.label, DebugActionType.ToolMap, () =>
            {
                GenPlace.TryPlaceThing(ThingMaker.MakeThing(def), UI.MouseCell(), Find.CurrentMap, ThingPlaceMode.Near);
            })
        ).ToList();
    }
}
