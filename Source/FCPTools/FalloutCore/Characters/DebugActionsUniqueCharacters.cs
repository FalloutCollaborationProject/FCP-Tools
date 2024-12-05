using HarmonyLib;
using LudeonTK;

namespace FCP.Core;

public static class DebugActionsUniqueCharacters
{
    public const string CategoryName = "FCP: Characters";
    
    [DebugAction(CategoryName, "Spawn Character", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void GenerateCharacter()
    {
        var charOptions = new List<DebugMenuOption>();

        foreach (CharacterDef def in DefDatabase<CharacterDef>.AllDefsListForReading)
        {
            charOptions.Add(new DebugMenuOption(def.defName, DebugMenuOptionMode.Tool, delegate
            {
                IntVec3 cell = UI.MouseCell();
                Pawn pawn = UniqueCharactersTracker.Instance.GetOrGenPawn(def);
                GenSpawn.Spawn(pawn, cell, Find.CurrentMap);
            }));
        }
        
        Find.WindowStack.Add(new Dialog_DebugOptionListLister(charOptions, "Characters"));
    }
    
    [DebugAction(CategoryName, "Log Characters", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Playing)]
    private static void LogCharacters()
    {
        var characterDefs = DefDatabase<CharacterDef>.AllDefsListForReading;

        FCPLog.Message("Logging Characters: ");
        foreach (CharacterDef charDef in characterDefs)
        {
            FCPLog.Message($"{charDef.defName} with {charDef.definitions.Count} definitions and {charDef.roles.Count} roles");
            FCPLog.Message($"{charDef.defName} definitions: ");
            foreach (CharacterBaseDefinition definition in charDef.definitions)
            {
                FCPLog.Message($"- {definition.GetType().Name}");
            }
            FCPLog.Message($"{charDef.defName} roles: ");
            foreach (CharacterRole role in charDef.roles)
            {
                FCPLog.Message($"- {role.GetType().Name}");
            }
        }
    }
    
    [DebugAction(CategoryName, "Log Roles", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Playing)]
    private static void LogRoles()
    {
        FCPLog.Message("Logging Roles: ");
        foreach (Type roleType in CharacterRoleUtils.GetAllRoleTypes())
        {
            var charactersWithRole = DefDatabase<CharacterDef>.AllDefsListForReading
                .Where(x => x.roles.Any(role => role.GetType() == roleType)).ToList();
            
            FCPLog.Message($"Role Type: {roleType.Name} ({charactersWithRole.Count}):\nChars: {charactersWithRole.Join()}");
        }
    }
}