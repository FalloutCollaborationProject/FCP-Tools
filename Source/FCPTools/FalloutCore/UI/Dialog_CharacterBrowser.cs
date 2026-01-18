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

    // Filters
    private string searchText = "";
    private FactionDef filterFaction;
    private StatusFilter statusFilter = StatusFilter.All;

    // Cached faction list for dropdown
    private List<FactionDef> allFactions = [];

    // Statuses
    private enum StatusFilter
    {
        All,
        Alive,
        Dead,
        NotSpawned
    }

    public override Vector2 InitialSize => new Vector2(1600f, 900f);

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
        // Text search filter
        if (!searchText.NullOrEmpty())
        {
            bool matches = charDef.defName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;

            // Also check story definition names
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

        // Faction filter
        if (filterFaction != null && charDef.faction != filterFaction)
            return false;

        // Status filter
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
        Text.Font = GameFont.Small;

        // Title
        var titleRect = new Rect(0f, 0f, inRect.width, 30f);
        Text.Font = GameFont.Medium;
        Widgets.Label(titleRect, "Character Browser");
        Text.Font = GameFont.Small;

        var contentRect = new Rect(0f, 40f, inRect.width, inRect.height - 40f);

        // Left panel (40%)
        var leftPanel = new Rect(contentRect.x, contentRect.y, contentRect.width * 0.4f - PanelMargin / 2f, contentRect.height);
        DrawLeftPanel(leftPanel);

        // Right panel (60%)
        var rightPanel = new Rect(leftPanel.xMax + PanelMargin, contentRect.y, contentRect.width * 0.6f - PanelMargin / 2f, contentRect.height);
        DrawRightPanel(rightPanel);
    }

    private void DrawLeftPanel(Rect rect)
    {
        float curY = rect.y;

        // Search box
        var searchRect = new Rect(rect.x, curY, rect.width, FilterHeight);
        var newSearch = Widgets.TextField(searchRect, searchText);
        if (newSearch != searchText)
        {
            searchText = newSearch;
            ApplyFilters();
        }
        curY += FilterHeight + 6f;

        // Faction filter
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

        // Status filter
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

        // Divider line
        Widgets.DrawLineHorizontal(rect.x, curY, rect.width);
        curY += 6f;

        // Character count header
        var countRect = new Rect(rect.x, curY, rect.width, 20f);
        GUI.color = new Color(0.7f, 0.7f, 0.7f);
        Text.Font = GameFont.Tiny;
        Widgets.Label(countRect, $"Characters ({filteredCharacters.Count}/{allCharacters.Count})");
        Text.Font = GameFont.Small;
        GUI.color = Color.white;
        curY += 20f + 2f;

        // Column headers
        float listWidth = rect.width - 16f; // Account for scrollbar
        var headerRect = new Rect(rect.x, curY, listWidth, HeaderHeight);
        DrawListHeader(headerRect);
        curY += HeaderHeight;

        // Character list
        float listHeight = rect.yMax - curY;
        var listOutRect = new Rect(rect.x, curY, rect.width, listHeight);
        var listViewRect = new Rect(0f, 0f, listWidth, filteredCharacters.Count * RowHeight);

        Widgets.BeginScrollView(listOutRect, ref listScrollPos, listViewRect);

        float y = 0f;
        for (int i = 0; i < filteredCharacters.Count; i++)
        {
            var charDef = filteredCharacters[i];
            var rowRect = new Rect(0f, y, listViewRect.width, RowHeight);

            // Only draw visible rows
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

        // Column widths - Name (35%), Faction (40%), Status (25%)
        float nameWidth = innerRect.width * 0.35f;
        float factionWidth = innerRect.width * 0.40f;
        float statusWidth = innerRect.width * 0.25f;

        var nameRect = new Rect(innerRect.x, innerRect.y, nameWidth, innerRect.height);
        var factionRect = new Rect(nameRect.xMax, innerRect.y, factionWidth, innerRect.height);
        var statusRect = new Rect(factionRect.xMax, innerRect.y, statusWidth, innerRect.height);

        // Draw background
        Widgets.DrawLightHighlight(rect);
        Widgets.DrawLineHorizontal(rect.x, rect.yMax - 1f, rect.width);

        // Draw headers
        Text.Anchor = TextAnchor.MiddleLeft;
        GUI.color = new Color(0.85f, 0.85f, 0.85f);

        Widgets.Label(nameRect, "Name");
        Widgets.Label(factionRect, "Faction");
        Widgets.Label(statusRect, "Status");

        GUI.color = Color.white;
        Text.Anchor = TextAnchor.UpperLeft;
    }

    private void DrawCharacterRow(Rect rect, CharacterDef charDef, int index)
    {
        // Alternating background
        if (index % 2 == 1)
            Widgets.DrawLightHighlight(rect);

        // Selection highlight
        if (selectedCharacter == charDef)
            Widgets.DrawHighlightSelected(rect);
        else if (Mouse.IsOver(rect))
            Widgets.DrawHighlight(rect);

        // Click handling
        if (Widgets.ButtonInvisible(rect))
        {
            selectedCharacter = charDef;
        }

        var innerRect = rect.ContractedBy(4f, 0f);

        // Column widths - Name (35%), Faction (40%), Status (25%)
        float nameWidth = innerRect.width * 0.35f;
        float factionWidth = innerRect.width * 0.40f;
        float statusWidth = innerRect.width * 0.25f;

        var nameRect = new Rect(innerRect.x, innerRect.y, nameWidth, innerRect.height);
        var factionRect = new Rect(nameRect.xMax, innerRect.y, factionWidth, innerRect.height);
        var statusRect = new Rect(factionRect.xMax, innerRect.y, statusWidth, innerRect.height);

        // Role indicator
        bool hasRole = charDef.roles.Count > 0;
        string rolePrefix = hasRole ? "* " : "";

        // Display name
        string displayName = rolePrefix + GetDisplayName(charDef);

        // Status
        string statusStr = GetStatusString(charDef);

        Text.Anchor = TextAnchor.MiddleLeft;

        // Name column
        Widgets.Label(nameRect, displayName);

        // Faction column with icon
        if (charDef.faction != null)
        {
            // Draw faction icon
            float iconY = factionRect.y + (factionRect.height - IconSize) / 2f;
            var iconRect = new Rect(factionRect.x, iconY, IconSize, IconSize);

            Texture2D factionIcon = charDef.faction.FactionIcon;
            if (factionIcon != null)
            {
                GUI.color = charDef.faction.DefaultColor;
                GUI.DrawTexture(iconRect, factionIcon);
                GUI.color = Color.white;
            }

            // Faction name after icon
            var factionLabelRect = new Rect(iconRect.xMax + 4f, factionRect.y, factionRect.width - IconSize - 4f, factionRect.height);
            string factionStr = charDef.faction.LabelCap;
            GUI.color = new Color(0.85f, 0.85f, 0.85f);
            Widgets.Label(factionLabelRect, factionStr);
        }
        else
        {
            GUI.color = new Color(0.5f, 0.5f, 0.5f);
            Widgets.Label(factionRect, "No Faction");
        }

        GUI.color = Color.white;

        // Status column with color
        GUI.color = statusStr switch
        {
            "Alive" or "Alive, Spawned" => new Color(0.5f, 1f, 0.5f),
            "Dead" => new Color(1f, 0.5f, 0.5f),
            _ => new Color(0.6f, 0.6f, 0.6f)
        };
        Widgets.Label(statusRect, statusStr);
        GUI.color = Color.white;

        Text.Anchor = TextAnchor.UpperLeft;
    }

    private void DrawRightPanel(Rect rect)
    {
        Widgets.DrawMenuSection(rect);

        if (selectedCharacter == null)
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = new Color(0.6f, 0.6f, 0.6f);
            Widgets.Label(rect, "Select a character to view details");
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            return;
        }

        var innerRect = rect.ContractedBy(PanelMargin);

        // Action buttons at bottom
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

        // Header
        Text.Font = GameFont.Medium;
        listing.Label(GetDisplayName(selectedCharacter));
        Text.Font = GameFont.Small;
        listing.Gap(4f);

        // Basic info
        DrawSection(listing, "Basic Info", () =>
        {
            listing.Label($"DefName: {selectedCharacter.defName}");
            listing.Label($"Faction: {selectedCharacter.faction?.LabelCap ?? "Factionless"}");
            listing.Label($"PawnKind: {selectedCharacter.pawnKind?.defName ?? "None Set"}");
            listing.Label($"Xenotype: {selectedCharacter.xenotype?.LabelCap ?? "None Set"}");
            listing.Label($"Status: {GetStatusString(selectedCharacter)}");
        });

        // Story definition
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

        // Appearance definition
        var appearance = selectedCharacter.definitions.OfType<CharacterAppearanceDefinition>().FirstOrDefault();
        if (appearance != null)
        {
            DrawSection(listing, "Appearance", () =>
            {
                if (appearance.hairDef != null)
                    listing.Label($"Hair: {appearance.hairDef.defName}");
                if (appearance.beardDef != null)
                    listing.Label($"Beard: {appearance.beardDef.defName}");
                if (appearance.bodyTypeDef != null)
                    listing.Label($"Body Type: {appearance.bodyTypeDef.defName}");
                if (appearance.headTypeDef != null)
                    listing.Label($"Head Type: {appearance.headTypeDef.defName}");
                if (appearance.faceTattooDef != null)
                    listing.Label($"Face Tattoo: {appearance.faceTattooDef.defName}");
                if (appearance.bodyTattooDef != null)
                    listing.Label($"Body Tattoo: {appearance.bodyTattooDef.defName}");
                if (appearance.hairColor != null)
                    listing.Label($"Hair Color: {appearance.hairColor}");
                if (appearance.skinColorOverride != null)
                    listing.Label($"Skin Color: {appearance.skinColorOverride}");
            });
        }

        // Title definition
        var title = selectedCharacter.definitions.OfType<CharacterTitleDefinition>().FirstOrDefault();
        if (title != null)
        {
            DrawSection(listing, "Title", () =>
            {
                listing.Label($"Title: {title.title?.LabelCap ?? "None"}");
            });
        }

        // Roles
        if (selectedCharacter.roles.Count > 0)
        {
            DrawSection(listing, "Roles", () =>
            {
                foreach (var role in selectedCharacter.roles)
                {
                    string roleName = role.GetType().Name;
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

        // Unique items
        var uniqueItems = selectedCharacter.definitions.OfType<CharacterUniqueItemDefinition>().ToList();
        if (uniqueItems.Count > 0)
        {
            DrawSection(listing, "Unique Items", () =>
            {
                foreach (var item in uniqueItems)
                {
                    if (item.uniqueItem != null)
                        listing.Label($"- {item.uniqueItem.LabelCap} ({item.uniqueItem.defName})");
                }
            });
        }

        listing.End();
        Widgets.EndScrollView();
    }

    private void DrawSection(Listing_Standard listing, string title, Action content)
    {
        listing.Gap(8f);
        GUI.color = new Color(0.8f, 0.8f, 0.8f);
        listing.Label($"-- {title} --");
        GUI.color = Color.white;
        listing.Gap(2f);
        content();
    }

    private float CalculateDetailsHeight()
    {
        if (selectedCharacter == null) return 100f;

        float height = 60f; // Header + basic spacing

        // Basic info: always 5 lines
        height += 30f + 5 * 24f;

        // Story
        var story = selectedCharacter.definitions.OfType<CharacterStoryDefinition>().FirstOrDefault();
        if (story != null)
        {
            height += 30f;
            if (!story.firstName.NullOrEmpty() || !story.lastName.NullOrEmpty()) height += 24f;
            if (story.gender != null) height += 24f;
            if (story.age != null) height += 24f;
            if (story.chronologicalAge != null && story.chronologicalAge != story.age) height += 24f;
        }

        // Appearance
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

        // Title
        var title = selectedCharacter.definitions.OfType<CharacterTitleDefinition>().FirstOrDefault();
        if (title != null)
            height += 30f + 24f;

        // Roles
        if (selectedCharacter.roles.Count > 0)
            height += 30f + selectedCharacter.roles.Count * 24f;

        // Unique items
        var uniqueItems = selectedCharacter.definitions.OfType<CharacterUniqueItemDefinition>().ToList();
        if (uniqueItems.Count > 0)
            height += 30f + uniqueItems.Count * 24f;

        return height + 20f; // Extra padding
    }

    private void DrawActionButtons(Rect rect)
    {
        float buttonWidth = (rect.width - PanelMargin * 2f) / 3f;

        var generateButton = new Rect(rect.x, rect.y, buttonWidth, rect.height);
        var spawnButton = new Rect(generateButton.xMax + PanelMargin, rect.y, buttonWidth, rect.height);
        var gotoButton = new Rect(spawnButton.xMax + PanelMargin, rect.y, buttonWidth, rect.height);

        var tracker = UniqueCharactersTracker.Instance;

        // Generate button - creates pawn without spawning
        bool alreadyExists = tracker?.CharacterPawnExists(selectedCharacter) ?? false;
        if (alreadyExists)
            GUI.color = new Color(0.5f, 0.5f, 0.5f);

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
        GUI.color = Color.white;

        // Spawn button
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

        // Go to pawn button (only enabled if pawn exists and is spawned)
        bool canGoTo = tracker?.CharacterPawnSpawned(selectedCharacter) ?? false;

        if (!canGoTo)
            GUI.color = new Color(0.5f, 0.5f, 0.5f);

        if (Widgets.ButtonText(gotoButton, "Go to Pawn") && canGoTo)
        {
            if (tracker != null && tracker.TryGetPawnCharacter(null, out _))
            {
                // Need to get pawn differently - the tracker stores by CharacterDef
                Pawn pawn = tracker.GetOrGenPawn(selectedCharacter);
                if (pawn is { Spawned: true })
                {
                    CameraJumper.TryJumpAndSelect(pawn);
                    Close();
                }
            }
        }

        GUI.color = Color.white;
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
