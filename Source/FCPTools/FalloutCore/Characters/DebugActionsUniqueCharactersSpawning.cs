using LudeonTK;

namespace FCP.Core;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public static class DebugActionsUniqueCharactersSpawning
{
    private const string CategoryName = "FCP: Characters";

    [DebugAction(CategoryName, "Spawn Character", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void SpawnCharacter()
    {
        var options = new List<DebugMenuOption>
        {
            new DebugMenuOption("By Faction", DebugMenuOptionMode.Action, ShowCharactersByFaction),
            new DebugMenuOption("By PawnKind", DebugMenuOptionMode.Action, ShowCharactersByPawnKind),
            new DebugMenuOption("With Definitions", DebugMenuOptionMode.Action, ShowCharactersWithDefinitions),
            new DebugMenuOption("With Roles", DebugMenuOptionMode.Action, ShowCharactersWithRoles)
        };

        Find.WindowStack.Add(new Dialog_DebugOptionListLister(options, "Browse By"));
    }

    private static void ShowCharactersByFaction()
    {
        var allDefs = DefDatabase<CharacterDef>.AllDefsListForReading;
        var byFaction = allDefs.GroupBy(def => def.faction).OrderBy(g => g.Key?.defName ?? "None");

        var options = new List<DebugMenuOption>
        {
            new DebugMenuOption("Back", DebugMenuOptionMode.Action, SpawnCharacter)
        };
        foreach (var group in byFaction)
        {
            string factionName = group.Key?.defName ?? "No Faction";
            int count = group.Count();
            options.Add(new DebugMenuOption($"{factionName} ({count})", DebugMenuOptionMode.Action, () =>
            {
                ShowCharacterList(group.ToList(), factionName, ShowCharactersByFaction);
            }));
        }

        Find.WindowStack.Add(new Dialog_DebugOptionListLister(options, "Factions"));
    }

    private static void ShowCharactersByPawnKind()
    {
        var allDefs = DefDatabase<CharacterDef>.AllDefsListForReading;
        var byPawnKind = allDefs.GroupBy(def => def.pawnKind).OrderBy(g => g.Key?.defName ?? "None");

        var options = new List<DebugMenuOption>
        {
            new DebugMenuOption("Back", DebugMenuOptionMode.Action, SpawnCharacter)
        };
        foreach (var group in byPawnKind)
        {
            string kindName = group.Key?.defName ?? "No PawnKind";
            int count = group.Count();
            options.Add(new DebugMenuOption($"{kindName} ({count})", DebugMenuOptionMode.Action, () =>
            {
                ShowCharacterList(group.ToList(), kindName, ShowCharactersByPawnKind);
            }));
        }

        Find.WindowStack.Add(new Dialog_DebugOptionListLister(options, "PawnKinds"));
    }

    private static void ShowCharactersWithDefinitions()
    {
        var allDefs = DefDatabase<CharacterDef>.AllDefsListForReading;
        var withDefinitions = allDefs.Where(def => def.definitions.Count > 0).ToList();

        if (!withDefinitions.Any())
        {
            Messages.Message("No characters have definitions.", MessageTypeDefOf.RejectInput, false);
            return;
        }

        var byDefinitionType = withDefinitions
            .SelectMany(def => def.definitions.Select(d => new { Character = def, DefinitionType = d.GetType() }))
            .GroupBy(x => x.DefinitionType)
            .OrderBy(g => g.Key.Name);

        var options = new List<DebugMenuOption>
        {
            new DebugMenuOption("Back", DebugMenuOptionMode.Action, SpawnCharacter)
        };
        foreach (var group in byDefinitionType)
        {
            string typeName = group.Key.Name;
            var characters = group.Select(x => x.Character).Distinct().ToList();
            options.Add(new DebugMenuOption($"{typeName} ({characters.Count})", DebugMenuOptionMode.Action, () =>
            {
                ShowCharacterList(characters, typeName, ShowCharactersWithDefinitions);
            }));
        }

        Find.WindowStack.Add(new Dialog_DebugOptionListLister(options, "Definition Types"));
    }

    private static void ShowCharactersWithRoles()
    {
        var roleTypes = CharacterRoleUtils.GetAllRoleTypes();

        if (!roleTypes.Any())
        {
            Messages.Message("No characters have roles.", MessageTypeDefOf.RejectInput, false);
            return;
        }

        var options = new List<DebugMenuOption>
        {
            new DebugMenuOption("Back", DebugMenuOptionMode.Action, SpawnCharacter)
        };
        foreach (Type roleType in roleTypes.OrderBy(type => type.Name))
        {
            var charactersWithRole = DefDatabase<CharacterDef>.AllDefsListForReading
                .Where(def => def.roles.Any(role => role.GetType() == roleType))
                .ToList();

            options.Add(new DebugMenuOption($"{roleType.Name} ({charactersWithRole.Count})", DebugMenuOptionMode.Action, () =>
            {
                ShowCharacterList(charactersWithRole, roleType.Name, ShowCharactersWithRoles);
            }));
        }

        Find.WindowStack.Add(new Dialog_DebugOptionListLister(options, "Role Types"));
    }

    private static void ShowCharacterList(List<CharacterDef> characters, string title, Action backAction)
    {
        var options = new List<DebugMenuOption>
        {
            new DebugMenuOption("Back", DebugMenuOptionMode.Action, backAction)
        };

        foreach (CharacterDef def in characters.OrderBy(def => def.defName))
        {
            options.Add(new DebugMenuOption(def.defName, DebugMenuOptionMode.Tool, () =>
            {
                IntVec3 cell = UI.MouseCell();
                Pawn pawn = UniqueCharactersTracker.Instance.GetOrGenPawn(def);
                GenSpawn.Spawn(pawn, cell, Find.CurrentMap);
            }));
        }

        Find.WindowStack.Add(new Dialog_DebugOptionListLister(options, title));
    }

    [DebugAction(CategoryName, "Open Character Browser", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void OpenCharacterBrowser()
    {
        Find.WindowStack.Add(new Dialog_CharacterBrowser());
    }

    [DebugAction(CategoryName, "Spawn Character from Def", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void SpawnCharacterFromDef()
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
}