using UnityEngine;

namespace FCP.Core;

[UsedImplicitly]
public class Dialog_CharacterBrowser : Window
{
    private const float RowHeight = 30f;
    private const float HeaderHeight = 24f;
    private const float PanelMargin = 10f;
    private const float FilterHeight = 28f;
    private const float IconSize = 22f;

    private List<CharacterDef> allCharacters = [];
    private List<CharacterDef> filteredCharacters = [];
    private CharacterDef selectedCharacter;
    private Vector2 listScrollPos;
    private Vector2 detailsScrollPos;

    private string searchText = "";
    private FactionDef filterFaction;
    private StatusFilter statusFilter = StatusFilter.All;

    private List<FactionDef> allFactions = [];

    private enum StatusFilter
    {
        All,
        Alive,
        Dead,
        NotSpawned
    }

    public override Vector2 InitialSize => new Vector2(1600f, 900f);

    private static Color DimPrimary => new Color(TerminalColors.PrimaryColor.r, TerminalColors.PrimaryColor.g, TerminalColors.PrimaryColor.b, 0.5f);

    public Dialog_CharacterBrowser()
    {
        doCloseX = true;
        absorbInputAroundWindow = true;
        closeOnClickedOutside = false;
        forcePause = false;
        draggable = true;
        resizeable = true;
    }

    public override void PreOpen()
    {
        base.PreOpen();
        RefreshCharacterList();
    }

    private void RefreshCharacterList()
    {
        allCharacters = DefDatabase<CharacterDef>.AllDefsListForReading.ToList();
        allFactions = allCharacters
            .Select(def => def.faction)
            .Where(factionDef => factionDef != null)
            .Distinct()
            .OrderBy(factionDef => factionDef.defName)
            .ToList();

        ApplyFilters();
    }

    private void ApplyFilters()
    {
        filteredCharacters = allCharacters
            .Where(MatchesFilters)
            .OrderBy(def => def.faction?.defName ?? "z")
            .ThenBy(def => def.defName)
            .ToList();
    }

    private bool MatchesFilters(CharacterDef charDef)
    {
        if (!searchText.NullOrEmpty())
        {
            bool matches = charDef.defName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;

            var story = charDef.definitions.OfType<CharacterStoryDefinition>().FirstOrDefault();
            if (story != null)
            {
                if (!story.firstName.NullOrEmpty() && story.firstName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    matches = true;
                if (!story.lastName.NullOrEmpty() && story.lastName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    matches = true;
                if (!story.nickname.NullOrEmpty() && story.nickname.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    matches = true;
            }

            if (!matches) return false;
        }

        if (filterFaction != null && charDef.faction != filterFaction)
            return false;

        if (statusFilter != StatusFilter.All)
        {
            var tracker = UniqueCharactersTracker.Instance;
            if (tracker == null) return statusFilter == StatusFilter.NotSpawned;

            bool exists = tracker.CharacterPawnExists(charDef);
            bool alive = tracker.CharacterPawnExistsAlive(charDef);

            switch (statusFilter)
            {
                case StatusFilter.Alive when !alive:
                case StatusFilter.Dead when (!exists || alive):
                case StatusFilter.NotSpawned when exists:
                    return false;
            }
        }

        return true;
    }

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Medium;
        GUI.color = TerminalColors.PrimaryColor;
        Widgets.Label(new Rect(0f, 0f, inRect.width, 30f), "Character Browser");
        Text.Font = GameFont.Small;
        Widgets.DrawLineHorizontal(0f, 34f, inRect.width);

        var contentRect = new Rect(0f, 44f, inRect.width, inRect.height - 44f);

        var leftPanel = new Rect(contentRect.x, contentRect.y, contentRect.width * 0.4f - PanelMargin / 2f, contentRect.height);
        DrawLeftPanel(leftPanel);

        Widgets.DrawLineVertical(leftPanel.xMax + PanelMargin / 2f, contentRect.y, contentRect.height);

        var rightPanel = new Rect(leftPanel.xMax + PanelMargin, contentRect.y, contentRect.width * 0.6f - PanelMargin / 2f, contentRect.height);
        DrawRightPanel(rightPanel);

        GUI.color = Color.white;
    }

    private void DrawLeftPanel(Rect rect)
    {
        float curY = rect.y;

        var searchRect = new Rect(rect.x, curY, rect.width, FilterHeight);
        var newSearch = Widgets.TextField(searchRect, searchText);
        if (newSearch != searchText)
        {
            searchText = newSearch;
            ApplyFilters();
        }
        curY += FilterHeight + 6f;

        var factionRowRect = new Rect(rect.x, curY, rect.width, FilterHeight);
        var factionLabel = new Rect(factionRowRect.x, factionRowRect.y, 60f, factionRowRect.height);
        var factionButton = new Rect(factionLabel.xMax + 4f, factionRowRect.y, factionRowRect.width - factionLabel.width - 4f, factionRowRect.height);

        Text.Anchor = TextAnchor.MiddleLeft;
        Widgets.Label(factionLabel, "Faction:");
        Text.Anchor = TextAnchor.UpperLeft;

        string factionButtonLabel = filterFaction?.LabelCap ?? "All Factions";
        if (Widgets.ButtonText(factionButton, factionButtonLabel))
        {
            var options = new List<FloatMenuOption>
            {
                new FloatMenuOption("All Factions", () =>
                {
                    filterFaction = null;
                    ApplyFilters();
                })
            };

            foreach (FactionDef faction in allFactions)
            {
                options.Add(new FloatMenuOption(faction.LabelCap, () =>
                {
                    filterFaction = faction;
                    ApplyFilters();
                }));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }
        curY += FilterHeight + 6f;

        var statusRowRect = new Rect(rect.x, curY, rect.width, FilterHeight);
        var statusLabel = new Rect(statusRowRect.x, statusRowRect.y, 60f, statusRowRect.height);
        var statusButton = new Rect(statusLabel.xMax + 4f, statusRowRect.y, statusRowRect.width - statusLabel.width - 4f, statusRowRect.height);

        Text.Anchor = TextAnchor.MiddleLeft;
        Widgets.Label(statusLabel, "Status:");
        Text.Anchor = TextAnchor.UpperLeft;

        if (Widgets.ButtonText(statusButton, statusFilter.ToString()))
        {
            var options = new List<FloatMenuOption>();
            foreach (StatusFilter filter in Enum.GetValues(typeof(StatusFilter)))
            {
                options.Add(new FloatMenuOption(filter.ToString(), () =>
                {
                    statusFilter = filter;
                    ApplyFilters();
                }));
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }
        curY += FilterHeight + 6f;

        GUI.color = DimPrimary;
        Widgets.DrawLineHorizontal(rect.x, curY, rect.width);
        GUI.color = TerminalColors.PrimaryColor;
        curY += 6f;

        var countRect = new Rect(rect.x, curY, rect.width, 20f);
        GUI.color = DimPrimary;
        Text.Font = GameFont.Tiny;
        Widgets.Label(countRect, $"Characters ({filteredCharacters.Count}/{allCharacters.Count})");
        Text.Font = GameFont.Small;
        GUI.color = TerminalColors.PrimaryColor;
        curY += 20f + 2f;

        float listWidth = rect.width - 16f;
        var headerRect = new Rect(rect.x, curY, listWidth, HeaderHeight);
        DrawListHeader(headerRect);
        curY += HeaderHeight;

        float listHeight = rect.yMax - curY;
        var listOutRect = new Rect(rect.x, curY, rect.width, listHeight);
        var listViewRect = new Rect(0f, 0f, listWidth, filteredCharacters.Count * RowHeight);

        Widgets.BeginScrollView(listOutRect, ref listScrollPos, listViewRect);

        float y = 0f;
        for (int i = 0; i < filteredCharacters.Count; i++)
        {
            var charDef = filteredCharacters[i];
            var rowRect = new Rect(0f, y, listViewRect.width, RowHeight);

            if (y + RowHeight >= listScrollPos.y && y <= listScrollPos.y + listOutRect.height)
            {
                DrawCharacterRow(rowRect, charDef, i);
            }

            y += RowHeight;
        }

        Widgets.EndScrollView();
    }

    private void DrawListHeader(Rect rect)
    {
        var innerRect = rect.ContractedBy(4f, 0f);

        float nameWidth = innerRect.width * 0.35f;
        float factionWidth = innerRect.width * 0.40f;
        float statusWidth = innerRect.width * 0.25f;

        var nameRect = new Rect(innerRect.x, innerRect.y, nameWidth, innerRect.height);
        var factionRect = new Rect(nameRect.xMax, innerRect.y, factionWidth, innerRect.height);
        var statusRect = new Rect(factionRect.xMax, innerRect.y, statusWidth, innerRect.height);

        Widgets.DrawLineHorizontal(rect.x, rect.yMax - 1f, rect.width);

        Text.Anchor = TextAnchor.MiddleLeft;
        Text.Font = GameFont.Tiny;
        GUI.color = DimPrimary;

        Widgets.Label(nameRect, "Name");
        Widgets.Label(factionRect, "Faction");
        Widgets.Label(statusRect, "Status");

        GUI.color = TerminalColors.PrimaryColor;
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.UpperLeft;
    }

    private void DrawCharacterRow(Rect rect, CharacterDef charDef, int index)
    {
        if (index % 2 == 1)
            Widgets.DrawLightHighlight(rect);

        if (selectedCharacter == charDef)
            Widgets.DrawHighlightSelected(rect);
        else if (Mouse.IsOver(rect))
            Widgets.DrawHighlight(rect);

        if (Widgets.ButtonInvisible(rect))
        {
            selectedCharacter = charDef;
        }

        var innerRect = rect.ContractedBy(4f, 0f);

        float nameWidth = innerRect.width * 0.35f;
        float factionWidth = innerRect.width * 0.40f;
        float statusWidth = innerRect.width * 0.25f;

        var nameRect = new Rect(innerRect.x, innerRect.y, nameWidth, innerRect.height);
        var factionRect = new Rect(nameRect.xMax, innerRect.y, factionWidth, innerRect.height);
        var statusRect = new Rect(factionRect.xMax, innerRect.y, statusWidth, innerRect.height);

        bool hasRole = charDef.roles.Count > 0;
        string rolePrefix = hasRole ? "* " : "";

        string displayName = rolePrefix + GetDisplayName(charDef);

        string statusStr = GetStatusString(charDef);

        Text.Anchor = TextAnchor.MiddleLeft;
        GUI.color = selectedCharacter == charDef ? TerminalColors.HighlightColor : TerminalColors.PrimaryColor;

        Widgets.Label(nameRect, displayName);

        if (charDef.faction != null)
        {
            float iconY = factionRect.y + (factionRect.height - IconSize) / 2f;
            var iconRect = new Rect(factionRect.x, iconY, IconSize, IconSize);

            Texture2D factionIcon = charDef.faction.FactionIcon;
            if (factionIcon != null)
            {
                GUI.color = charDef.faction.DefaultColor;
                GUI.DrawTexture(iconRect, factionIcon);
            }

            var factionLabelRect = new Rect(iconRect.xMax + 4f, factionRect.y, factionRect.width - IconSize - 4f, factionRect.height);
            string factionStr = charDef.faction.LabelCap;
            GUI.color = DimPrimary;
            Widgets.Label(factionLabelRect, factionStr);
        }
        else
        {
            GUI.color = DimPrimary;
            Widgets.Label(factionRect, "No Faction");
        }

        GUI.color = statusStr switch
        {
            "Alive" or "Alive, Spawned" => new Color(0.4f, 1f, 0.4f),
            "Dead" => new Color(1f, 0.4f, 0.4f),
            _ => DimPrimary
        };
        Widgets.Label(statusRect, statusStr);
        GUI.color = TerminalColors.PrimaryColor;

        Text.Anchor = TextAnchor.UpperLeft;
    }

    private void DrawRightPanel(Rect rect)
    {
        if (selectedCharacter == null)
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = DimPrimary;
            Widgets.Label(rect, "Select a character to view details");
            GUI.color = TerminalColors.PrimaryColor;
            Text.Anchor = TextAnchor.UpperLeft;
            return;
        }

        var innerRect = rect.ContractedBy(PanelMargin);

        float buttonHeight = 35f;
        var buttonRect = new Rect(innerRect.x, innerRect.yMax - buttonHeight, innerRect.width, buttonHeight);
        var detailsRect = new Rect(innerRect.x, innerRect.y, innerRect.width, innerRect.height - buttonHeight - PanelMargin);

        DrawCharacterDetails(detailsRect);
        DrawActionButtons(buttonRect);
    }

    private void DrawCharacterDetails(Rect rect)
    {
        var viewRect = new Rect(0f, 0f, rect.width - 16f, CalculateDetailsHeight());
        Widgets.BeginScrollView(rect, ref detailsScrollPos, viewRect);

        var listing = new Listing_Standard();
        listing.Begin(viewRect);

        Text.Font = GameFont.Medium;
        GUI.color = TerminalColors.HighlightColor;
        listing.Label(GetDisplayName(selectedCharacter));
        GUI.color = TerminalColors.PrimaryColor;
        Text.Font = GameFont.Small;
        listing.Gap(4f);

        DrawSection(listing, "Basic Info", () =>
        {
            listing.Label($"Faction: {selectedCharacter.faction?.LabelCap ?? "Factionless"}");
            listing.Label($"Kind: {selectedCharacter.pawnKind?.LabelCap ?? "None Set"}");
            listing.Label($"Xenotype: {selectedCharacter.xenotype?.LabelCap ?? "None Set"}");
            listing.Label($"Status: {GetStatusString(selectedCharacter)}");
        });

        var story = selectedCharacter.definitions.OfType<CharacterStoryDefinition>().FirstOrDefault();
        if (story != null)
        {
            DrawSection(listing, "Story", () =>
            {
                if (!story.firstName.NullOrEmpty() || !story.lastName.NullOrEmpty())
                {
                    string fullName = $"{story.firstName ?? ""} {story.lastName ?? ""}".Trim();
                    if (!story.nickname.NullOrEmpty())
                        fullName = $"{story.firstName ?? ""} \"{story.nickname}\" {story.lastName ?? ""}".Trim();
                    listing.Label($"Name: {fullName}");
                }
                if (story.gender != null)
                    listing.Label($"Gender: {story.gender}");
                if (story.age != null)
                    listing.Label($"Age: {story.age}");
                if (story.chronologicalAge != null && story.chronologicalAge != story.age)
                    listing.Label($"Chronological Age: {story.chronologicalAge}");
            });
        }

        UniqueCharactersTracker.Instance.TryGetExistingPawn(selectedCharacter, out Pawn existingPawn);
        if (existingPawn?.story != null && (existingPawn.story.Childhood != null || existingPawn.story.Adulthood != null))
        {
            DrawSection(listing, "Backstory", () =>
            {
                if (existingPawn.story.Childhood != null)
                    listing.Label($"Childhood: {existingPawn.story.Childhood.TitleCapFor(existingPawn.gender)}");
                if (existingPawn.story.Adulthood != null)
                    listing.Label($"Adulthood: {existingPawn.story.Adulthood.TitleCapFor(existingPawn.gender)}");
            });
        }

        var appearance = selectedCharacter.definitions.OfType<CharacterAppearanceDefinition>().FirstOrDefault();
        if (appearance != null)
        {
            DrawSection(listing, "Appearance", () =>
            {
                if (appearance.hairDef != null)
                    listing.Label($"Hair: {appearance.hairDef.LabelCap}");
                if (appearance.beardDef != null)
                    listing.Label($"Beard: {appearance.beardDef.LabelCap}");
                if (appearance.bodyTypeDef != null)
                    listing.Label($"Body Type: {appearance.bodyTypeDef.LabelCap}");
                if (appearance.headTypeDef != null)
                    listing.Label($"Head Type: {appearance.headTypeDef.LabelCap}");
                if (appearance.faceTattooDef != null)
                    listing.Label($"Face Tattoo: {appearance.faceTattooDef.LabelCap}");
                if (appearance.bodyTattooDef != null)
                    listing.Label($"Body Tattoo: {appearance.bodyTattooDef.LabelCap}");
                if (appearance.hairColor != null)
                    DrawColorSwatchLine(listing, "Hair Color", appearance.hairColor.Value);
                if (appearance.skinColorOverride != null)
                    DrawColorSwatchLine(listing, "Skin Color", appearance.skinColorOverride.Value);
            });
        }

        var title = selectedCharacter.definitions.OfType<CharacterTitleDefinition>().FirstOrDefault();
        if (title != null)
        {
            DrawSection(listing, "Title", () =>
            {
                listing.Label($"Title: {title.title?.LabelCap ?? "None"}");
            });
        }

        if (selectedCharacter.roles.Count > 0)
        {
            DrawSection(listing, "Roles", () =>
            {
                foreach (var role in selectedCharacter.roles)
                {
                    string roleName = PrettifyRoleName(role.GetType().Name);
                    if (role is CharacterRole_FactionLeader leader)
                    {
                        listing.Label($"{roleName} (seniority: {leader.seniority})");
                    }
                    else
                    {
                        listing.Label(roleName);
                    }
                }
            });
        }

        var uniqueItems = selectedCharacter.definitions.OfType<CharacterUniqueItemDefinition>()
            .Where(item => item.uniqueItem != null).ToList();
        if (uniqueItems.Count > 0)
        {
            DrawSection(listing, "Equipment", () =>
            {
                foreach (var item in uniqueItems)
                {
                    DrawEquipmentRow(listing, item.uniqueItem);
                }
            });
        }

        listing.Gap(12f);
        Text.Font = GameFont.Tiny;
        GUI.color = DimPrimary;
        listing.Label(selectedCharacter.defName);
        GUI.color = TerminalColors.PrimaryColor;
        Text.Font = GameFont.Small;

        listing.End();
        Widgets.EndScrollView();
    }

    private static void DrawColorSwatchLine(Listing_Standard listing, string label, Color color)
    {
        Rect rect = listing.GetRect(20f);
        Rect labelRect = new Rect(rect.x, rect.y, rect.width - 26f, rect.height);
        Rect swatchRect = new Rect(rect.xMax - 20f, rect.y + 2f, 16f, 16f);

        Text.Anchor = TextAnchor.MiddleLeft;
        Widgets.Label(labelRect, $"{label}:");
        Text.Anchor = TextAnchor.UpperLeft;

        Widgets.DrawBoxSolid(swatchRect, color);
    }

    private static void DrawEquipmentRow(Listing_Standard listing, ThingDef def)
    {
        Rect rect = listing.GetRect(28f);
        Rect iconRect = new Rect(rect.x, rect.y, 24f, 24f);
        Rect labelRect = new Rect(iconRect.xMax + 6f, rect.y, rect.width - iconRect.width - 6f, rect.height);

        if (def.uiIcon != null)
        {
            GUI.color = def.uiIconColor;
            GUI.DrawTexture(iconRect, def.uiIcon);
            GUI.color = TerminalColors.PrimaryColor;
        }

        Text.Anchor = TextAnchor.MiddleLeft;
        Widgets.Label(labelRect, def.LabelCap);
        Text.Anchor = TextAnchor.UpperLeft;
    }

    private static string PrettifyRoleName(string typeName)
    {
        string name = typeName.StartsWith("CharacterRole_") ? typeName["CharacterRole_".Length..] : typeName;
        string result = "";
        foreach (char c in name)
        {
            if (char.IsUpper(c) && result.Length > 0)
                result += " ";
            result += c;
        }
        return result;
    }

    private void DrawSection(Listing_Standard listing, string title, Action content)
    {
        listing.Gap(8f);
        Text.Font = GameFont.Tiny;
        GUI.color = DimPrimary;
        listing.Label(title);
        GUI.color = TerminalColors.PrimaryColor;
        Text.Font = GameFont.Small;
        listing.Gap(2f);
        content();
    }

    private float CalculateDetailsHeight()
    {
        if (selectedCharacter == null) return 100f;

        float height = 60f;

        height += 30f + 4 * 24f;

        var story = selectedCharacter.definitions.OfType<CharacterStoryDefinition>().FirstOrDefault();
        if (story != null)
        {
            height += 30f;
            if (!story.firstName.NullOrEmpty() || !story.lastName.NullOrEmpty()) height += 24f;
            if (story.gender != null) height += 24f;
            if (story.age != null) height += 24f;
            if (story.chronologicalAge != null && story.chronologicalAge != story.age) height += 24f;
        }

        UniqueCharactersTracker.Instance.TryGetExistingPawn(selectedCharacter, out Pawn existingPawn);
        if (existingPawn?.story != null && (existingPawn.story.Childhood != null || existingPawn.story.Adulthood != null))
        {
            height += 30f;
            if (existingPawn.story.Childhood != null) height += 24f;
            if (existingPawn.story.Adulthood != null) height += 24f;
        }

        var appearance = selectedCharacter.definitions.OfType<CharacterAppearanceDefinition>().FirstOrDefault();
        if (appearance != null)
        {
            height += 30f;
            if (appearance.hairDef != null) height += 24f;
            if (appearance.beardDef != null) height += 24f;
            if (appearance.bodyTypeDef != null) height += 24f;
            if (appearance.headTypeDef != null) height += 24f;
            if (appearance.faceTattooDef != null) height += 24f;
            if (appearance.bodyTattooDef != null) height += 24f;
            if (appearance.hairColor != null) height += 24f;
            if (appearance.skinColorOverride != null) height += 24f;
        }

        var title = selectedCharacter.definitions.OfType<CharacterTitleDefinition>().FirstOrDefault();
        if (title != null)
            height += 30f + 24f;

        if (selectedCharacter.roles.Count > 0)
            height += 30f + selectedCharacter.roles.Count * 24f;

        var uniqueItems = selectedCharacter.definitions.OfType<CharacterUniqueItemDefinition>()
            .Where(item => item.uniqueItem != null).ToList();
        if (uniqueItems.Count > 0)
            height += 30f + uniqueItems.Count * 28f;

        height += 12f + 20f;

        return height + 20f;
    }

    private void DrawActionButtons(Rect rect)
    {
        float buttonWidth = (rect.width - PanelMargin * 2f) / 3f;

        var generateButton = new Rect(rect.x, rect.y, buttonWidth, rect.height);
        var spawnButton = new Rect(generateButton.xMax + PanelMargin, rect.y, buttonWidth, rect.height);
        var gotoButton = new Rect(spawnButton.xMax + PanelMargin, rect.y, buttonWidth, rect.height);

        var tracker = UniqueCharactersTracker.Instance;

        bool alreadyExists = tracker?.CharacterPawnExists(selectedCharacter) ?? false;
        GUI.color = alreadyExists ? DimPrimary : TerminalColors.PrimaryColor;

        if (Widgets.ButtonText(generateButton, "Generate Pawn"))
        {
            if (alreadyExists)
            {
                Messages.Message($"{GetDisplayName(selectedCharacter)} already exists", MessageTypeDefOf.RejectInput, false);
            }
            else
            {
                UniqueCharactersTracker.Instance.GetOrGenPawn(selectedCharacter);
                Messages.Message($"Generated {GetDisplayName(selectedCharacter)}", MessageTypeDefOf.PositiveEvent, false);
            }
        }
        GUI.color = TerminalColors.PrimaryColor;

        if (Widgets.ButtonText(spawnButton, "Spawn at Mouse"))
        {
            if (Find.CurrentMap != null)
            {
                IntVec3 cell = UI.MouseCell();
                Pawn pawn = UniqueCharactersTracker.Instance.GetOrGenPawn(selectedCharacter);
                GenSpawn.Spawn(pawn, cell, Find.CurrentMap);
                Messages.Message($"Spawned {GetDisplayName(selectedCharacter)}", MessageTypeDefOf.PositiveEvent, false);
            }
            else
            {
                Messages.Message("No map available", MessageTypeDefOf.RejectInput, false);
            }
        }

        bool canGoTo = tracker?.CharacterPawnSpawned(selectedCharacter) ?? false;
        GUI.color = canGoTo ? TerminalColors.PrimaryColor : DimPrimary;

        if (Widgets.ButtonText(gotoButton, "Go to Pawn") && canGoTo)
        {
            Pawn pawn = tracker.GetOrGenPawn(selectedCharacter);
            if (pawn is { Spawned: true })
            {
                CameraJumper.TryJumpAndSelect(pawn);
                Close();
            }
        }

        GUI.color = TerminalColors.PrimaryColor;
    }

    private static string GetDisplayName(CharacterDef charDef)
    {
        var story = charDef.definitions.OfType<CharacterStoryDefinition>().FirstOrDefault();

        if (story == null)
            return charDef.defName;
        if (!story.nickname.NullOrEmpty())
            return story.nickname;
        if (!story.firstName.NullOrEmpty())
            return $"{story.firstName} {story.lastName}".Trim();

        return charDef.defName;
    }

    private static string GetStatusString(CharacterDef charDef)
    {
        var tracker = UniqueCharactersTracker.Instance;

        if (!tracker.CharacterPawnExists(charDef))
            return "None";
        if (tracker.CharacterPawnDead(charDef))
            return "Dead";

        return tracker.CharacterPawnSpawned(charDef) ? "Alive, Spawned" : "Alive";
    }
}
