using LudeonTK;

namespace FCP.Core.Robotics;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public static class DebugActionsProtectronSpawning
{
    private const string CategoryName = "FCP: Robotics";

    [DebugAction(CategoryName, "Spawn Protectron (pick preset)", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void SpawnProtectronPreset()
    {
        ProtectronPresetExtension presetExt = PawnKindDefOf_Protectron.FCP_Pawnkind_Protectron.GetModExtension<ProtectronPresetExtension>();
        if (presetExt == null || presetExt.presets.Count == 0)
        {
            Messages.Message("No protectron presets found.", MessageTypeDefOf.RejectInput, false);
            return;
        }

        var options = new List<DebugMenuOption>();
        foreach (ProtectronLoadoutPreset preset in presetExt.presets)
        {
            string id = preset.id;
            options.Add(new DebugMenuOption(id, DebugMenuOptionMode.Tool, delegate
            {
                IntVec3 cell = UI.MouseCell();
                Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf_Protectron.FCP_Pawnkind_Protectron, Faction.OfPlayer));
                GenSpawn.Spawn(pawn, cell, Find.CurrentMap);
                pawn.GetComp<CompProtectronLoadout>()?.ApplyPreset(id);
            }));
        }

        Find.WindowStack.Add(new Dialog_DebugOptionListLister(options, "Protectron Presets"));
    }
}
