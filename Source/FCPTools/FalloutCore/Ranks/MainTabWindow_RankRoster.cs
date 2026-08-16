using FCP.Core;
using FCP.Enlist;
using UnityEngine;

namespace FCP.Ranks;

[UsedImplicitly]
public class MainTabWindow_RankRoster : MainTabWindow
{
    private enum RosterTab
    {
        Order,
        Lineage,
        Records
    }

    private const float RowHeight = 30f;
    private const float PanelMargin = 10f;
    private const float TabBarHeight = 32f;
    private const int MaxRecordsShown = 200;

    private List<Pawn> allColonists = [];
    private Pawn selectedPawn;
    private RosterTab curTab = RosterTab.Order;
    private Vector2 listScrollPos;
    private Vector2 detailScrollPos;
    private Vector2 lineageScrollPos;
    private Vector2 recordsScrollPos;
    private readonly HashSet<FactionDef> expandedFactions = [];
    private bool expandedIndependent;

    public override Vector2 RequestedTabSize => new Vector2(1000f, 680f);

    public override void PreOpen()
    {
        base.PreOpen();
        allColonists = PawnsFinder.AllMaps_FreeColonists
            .Concat(ExpeditionComp.AllAway)
            .Distinct()
            .OrderByDescending(p => GameComp.IsTracked(p))
            .ThenBy(p => p.LabelShortCap)
            .ToList();

        if (selectedPawn != null && !allColonists.Contains(selectedPawn))
            selectedPawn = null;
    }

    private static GameComponent_PawnRanks GameComp => Current.Game.GetComponent<GameComponent_PawnRanks>();
    private static GameComponent_Expeditions ExpeditionComp => Current.Game.GetComponent<GameComponent_Expeditions>();

    private static Color DimPrimary => new Color(TerminalColors.PrimaryColor.r, TerminalColors.PrimaryColor.g, TerminalColors.PrimaryColor.b, 0.5f);

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Medium;
        GUI.color = TerminalColors.PrimaryColor;
        Widgets.Label(new Rect(0f, 0f, inRect.width, 30f), "FCP_Rank_TerminalTitle".Translate());
        Text.Font = GameFont.Small;

        var tabBarRect = new Rect(0f, 40f, inRect.width, TabBarHeight);
        DrawTabBar(tabBarRect);

        var contentRect = new Rect(0f, tabBarRect.yMax + 4f, inRect.width, inRect.height - tabBarRect.yMax - 4f);

        switch (curTab)
        {
            case RosterTab.Lineage:
                DrawLineageTab(contentRect);
                break;
            case RosterTab.Records:
                DrawRecordsTab(contentRect);
                break;
            default:
                DrawOrderTab(contentRect);
                break;
        }

        GUI.color = Color.white;
    }

    private void DrawTabBar(Rect rect)
    {
        float tabWidth = rect.width / 3f;
        DrawTabButton(new Rect(rect.x, rect.y, tabWidth, rect.height), "FCP_Rank_Tab_Order".Translate(), RosterTab.Order);
        DrawTabButton(new Rect(rect.x + tabWidth, rect.y, tabWidth, rect.height), "FCP_Rank_Tab_Lineage".Translate(), RosterTab.Lineage);
        DrawTabButton(new Rect(rect.x + tabWidth * 2f, rect.y, tabWidth, rect.height), "FCP_Rank_Tab_Records".Translate(), RosterTab.Records);

        GUI.color = TerminalColors.PrimaryColor;
        Widgets.DrawLineHorizontal(rect.x, rect.yMax, rect.width);
    }

    private void DrawTabButton(Rect rect, string label, RosterTab tab)
    {
        bool selected = curTab == tab;

        GUI.color = selected ? TerminalColors.HighlightColor : DimPrimary;
        Widgets.DrawLineHorizontal(rect.x, rect.y, rect.width);
        if (selected)
            Widgets.DrawLineHorizontal(rect.x, rect.yMax - 2f, rect.width);
        if (!selected && Mouse.IsOver(rect))
            Widgets.DrawHighlight(rect);

        Text.Anchor = TextAnchor.MiddleCenter;
        GUI.color = selected ? TerminalColors.HighlightColor : TerminalColors.PrimaryColor;
        Widgets.Label(rect, label);
        Text.Anchor = TextAnchor.UpperLeft;
        GUI.color = TerminalColors.PrimaryColor;

        if (Widgets.ButtonInvisible(rect))
            curTab = tab;
    }

    private void DrawOrderTab(Rect rect)
    {
        var leftPanel = new Rect(rect.x, rect.y, rect.width * 0.35f - PanelMargin / 2f, rect.height);
        DrawRoster(leftPanel);

        var rightPanel = new Rect(leftPanel.xMax + PanelMargin, rect.y, rect.width * 0.65f - PanelMargin / 2f, rect.height);
        DrawDetails(rightPanel);
    }

    private void DrawRoster(Rect rect)
    {
        GUI.color = TerminalColors.PrimaryColor;

        float listWidth = rect.width - 16f;
        var listOutRect = rect;
        var listViewRect = new Rect(0f, 0f, listWidth, allColonists.Count * RowHeight);

        Widgets.BeginScrollView(listOutRect, ref listScrollPos, listViewRect);

        float y = 0f;
        for (int i = 0; i < allColonists.Count; i++)
        {
            Pawn pawn = allColonists[i];
            var rowRect = new Rect(0f, y, listViewRect.width, RowHeight);

            if (y + RowHeight >= listScrollPos.y && y <= listScrollPos.y + listOutRect.height)
                DrawRosterRow(rowRect, pawn, i);

            y += RowHeight;
        }

        Widgets.EndScrollView();
    }

    private void DrawRosterRow(Rect rect, Pawn pawn, int index)
    {
        if (index % 2 == 1)
            Widgets.DrawLightHighlight(rect);
        if (selectedPawn == pawn)
            Widgets.DrawHighlightSelected(rect);
        else if (Mouse.IsOver(rect))
            Widgets.DrawHighlight(rect);

        if (Widgets.ButtonInvisible(rect))
            selectedPawn = pawn;

        var innerRect = rect.ContractedBy(4f, 0f);
        var nameRect = new Rect(innerRect.x, innerRect.y, innerRect.width * 0.55f, innerRect.height);
        var tierRect = new Rect(nameRect.xMax, innerRect.y, innerRect.width - nameRect.width, innerRect.height);

        Text.Anchor = TextAnchor.MiddleLeft;
        GUI.color = TerminalColors.PrimaryColor;
        Widgets.Label(nameRect, pawn.LabelShortCap);

        PawnRankData data = GameComp.GetRankData(pawn);
        GUI.color = data != null ? TerminalColors.HighlightColor : DimPrimary;
        Widgets.Label(tierRect, data != null ? data.currentTier.LabelCap : "FCP_Rank_Unranked".Translate());
        GUI.color = TerminalColors.PrimaryColor;
        Text.Anchor = TextAnchor.UpperLeft;
    }

    private void DrawDetails(Rect rect)
    {
        GUI.color = TerminalColors.PrimaryColor;

        if (selectedPawn == null)
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = DimPrimary;
            Widgets.Label(rect, "FCP_Rank_NoPawnSelected".Translate());
            GUI.color = TerminalColors.PrimaryColor;
            Text.Anchor = TextAnchor.UpperLeft;
            return;
        }

        var viewRect = new Rect(0f, 0f, rect.width - 16f, 1200f);
        Widgets.BeginScrollView(rect, ref detailScrollPos, viewRect);

        float curY = 0f;
        Text.Font = GameFont.Medium;
        Widgets.Label(new Rect(0f, curY, viewRect.width, 30f), selectedPawn.LabelShortCap);
        Text.Font = GameFont.Small;
        curY += 34f;

        PawnRankData data = GameComp.GetRankData(selectedPawn);
        ExpeditionData expeditionData = ExpeditionComp.GetExpeditionData(selectedPawn);
        if (data == null)
        {
            curY = DrawEnrollmentOptions(viewRect, curY);
        }
        else if (expeditionData != null)
        {
            curY = DrawCurrentRank(viewRect, curY, data);
            curY += 10f;
            curY = DrawAwayStatus(viewRect, curY, expeditionData);
        }
        else if (!RankTrackUtility.IsFactionActive(data.track))
        {
            curY = DrawCurrentRank(viewRect, curY, data);
            curY += 10f;
            curY = DrawSuspendedStatus(viewRect, curY, data);
        }
        else
        {
            curY = DrawCurrentRank(viewRect, curY, data);
            curY += 10f;
            curY = DrawMentorSection(viewRect, curY, data);
            curY += 10f;

            if (data.currentTier.expedition != null)
            {
                curY = DrawExpeditionSection(viewRect, curY, data.currentTier.expedition);
                curY += 10f;
            }

            List<RankTierDef> options = data.currentTier.nextTierOptions;
            if (!options.NullOrEmpty())
                curY = DrawPromotionOptions(viewRect, curY, data, options);
        }

        Widgets.EndScrollView();
    }

    private float DrawEnrollmentOptions(Rect viewRect, float curY)
    {
        GUI.color = TerminalColors.PrimaryColor;
        Widgets.Label(new Rect(0f, curY, viewRect.width, 22f), "FCP_Rank_NotEnrolled".Translate());
        curY += 26f;

        foreach (RankTrackDef track in GetAllTracks())
        {
            if (RankTrackUtility.IsFactionActive(track))
            {
                var buttonRect = new Rect(0f, curY, 260f, 28f);
                if (Widgets.ButtonText(buttonRect, "FCP_Rank_EnrollIn".Translate(track.label)))
                    GameComp.Enroll(selectedPawn, track);
                curY += 32f;
            }
            else if (Prefs.DevMode)
            {
                var buttonRect = new Rect(0f, curY, 260f, 28f);
                GUI.color = Color.red;
                if (Widgets.ButtonText(buttonRect, "FCP_Rank_DevForceEnroll".Translate(track.label)))
                    GameComp.Enroll(selectedPawn, track, devForce: true);
                GUI.color = TerminalColors.PrimaryColor;
                curY += 32f;
            }
            else
            {
                string label = "FCP_Rank_RequiresEnlistment".Translate(track.label);
                float height = Text.CalcHeight(label, viewRect.width);
                GUI.color = DimPrimary;
                Widgets.Label(new Rect(0f, curY, viewRect.width, height), label);
                GUI.color = TerminalColors.PrimaryColor;
                curY += height + 6f;
            }
        }

        return curY;
    }

    private float DrawCurrentRank(Rect viewRect, float curY, PawnRankData data)
    {
        Texture2D trackIcon = !data.track.iconPath.NullOrEmpty() ? ContentFinder<Texture2D>.Get(data.track.iconPath, false) : null;

        GUI.color = TerminalColors.HighlightColor;
        if (trackIcon != null)
        {
            GUI.DrawTexture(new Rect(0f, curY, 24f, 24f), trackIcon);
            Widgets.Label(new Rect(30f, curY, viewRect.width - 30f, 24f), data.currentTier.LabelCap);
        }
        else
        {
            Widgets.Label(new Rect(0f, curY, viewRect.width, 24f), data.currentTier.LabelCap);
        }
        GUI.color = TerminalColors.PrimaryColor;
        curY += 24f;

        string pathName = RankTrackUtility.GetPathName(data.track, data.currentTier);
        if (!pathName.NullOrEmpty())
        {
            GUI.color = DimPrimary;
            Widgets.Label(new Rect(0f, curY, viewRect.width, 20f), pathName);
            GUI.color = TerminalColors.PrimaryColor;
            curY += 22f;
        }

        if (!data.currentTier.description.NullOrEmpty())
        {
            float height = Text.CalcHeight(data.currentTier.description, viewRect.width);
            Widgets.Label(new Rect(0f, curY, viewRect.width, height), data.currentTier.description);
            curY += height + 4f;
        }

        int daysInRank = (Find.TickManager.TicksGame - data.rankStartTick) / GenDate.TicksPerDay;
        Widgets.Label(new Rect(0f, curY, viewRect.width, 20f), "FCP_Rank_DaysInRank".Translate(daysInRank));
        curY += 24f;

        return curY;
    }

    private void DrawLineageTab(Rect rect)
    {
        GUI.color = TerminalColors.PrimaryColor;

        List<FactionDef> factions = GetChainOfCommandFactions();
        List<CharacterDef> independents = GetIndependentCharacters();

        if (factions.Count == 0 && independents.Count == 0)
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = DimPrimary;
            Widgets.Label(rect, "FCP_Rank_Lineage_Empty".Translate());
            GUI.color = TerminalColors.PrimaryColor;
            Text.Anchor = TextAnchor.UpperLeft;
            return;
        }

        int folderCount = factions.Count + (independents.Count > 0 ? 1 : 0);
        int expandedCount = expandedFactions.Count + (expandedIndependent ? 1 : 0);
        float estimatedHeight = folderCount * 32f + expandedCount * 400f + 20f;
        var viewRect = new Rect(0f, 0f, rect.width - 16f, Mathf.Max(estimatedHeight, rect.height));
        Widgets.BeginScrollView(rect, ref lineageScrollPos, viewRect);

        float curY = 0f;
        foreach (FactionDef faction in factions)
            curY = DrawFactionFolder(viewRect, curY, faction);

        if (independents.Count > 0)
            curY = DrawIndependentFolder(viewRect, curY, independents);

        Widgets.EndScrollView();
    }

    private static List<CharacterDef> GetIndependentCharacters()
    {
        return DefDatabase<CharacterDef>.AllDefsListForReading
            .Where(c => c.faction == null)
            .ToList();
    }

    private float DrawIndependentFolder(Rect viewRect, float curY, List<CharacterDef> independents)
    {
        var headerRect = new Rect(0f, curY, viewRect.width, 28f);
        if (Mouse.IsOver(headerRect))
            Widgets.DrawHighlight(headerRect);
        if (Widgets.ButtonInvisible(headerRect))
            expandedIndependent = !expandedIndependent;

        var countRect = new Rect(headerRect.width - 180f, curY, 180f, 28f);
        var nameRect = new Rect(0f, curY, countRect.x, 28f);

        GUI.color = TerminalColors.HighlightColor;
        Text.Anchor = TextAnchor.MiddleLeft;
        string arrow = expandedIndependent ? "▼ " : "▶ ";
        Widgets.Label(nameRect, arrow + "FCP_Rank_Lineage_Independent".Translate());
        GUI.color = DimPrimary;
        Text.Anchor = TextAnchor.MiddleRight;
        Widgets.Label(countRect, "FCP_Rank_Lineage_FolderCount".Translate(independents.Count, 0));
        Text.Anchor = TextAnchor.UpperLeft;
        GUI.color = TerminalColors.PrimaryColor;
        curY += 30f;

        if (!expandedIndependent)
            return curY;

        foreach (CharacterDef characterDef in independents.OrderBy(GetCharacterDisplayName))
            curY = DrawNotableMember(viewRect, curY, characterDef);

        curY += 14f;
        return curY;
    }

    private List<FactionDef> GetChainOfCommandFactions()
    {
        var factions = new HashSet<FactionDef>();

        foreach (FactionDef factionDef in DefDatabase<FactionDef>.AllDefsListForReading)
        {
            FactionExtension_RankTracks extension = factionDef.GetModExtension<FactionExtension_RankTracks>();
            if (extension != null && !extension.rankTracks.NullOrEmpty())
                factions.Add(factionDef);
        }

        foreach (CharacterDef characterDef in DefDatabase<CharacterDef>.AllDefsListForReading)
        {
            if (characterDef.faction != null)
                factions.Add(characterDef.faction);
        }

        return factions.OrderBy(f => f.LabelCap.ToString()).ToList();
    }

    private float DrawFactionFolder(Rect viewRect, float curY, FactionDef faction)
    {
        bool expanded = expandedFactions.Contains(faction);
        List<CharacterDef> notableMembers = DefDatabase<CharacterDef>.AllDefsListForReading
            .Where(c => c.faction == faction)
            .ToList();
        List<RankTrackDef> tracks = faction.GetModExtension<FactionExtension_RankTracks>()?.rankTracks ?? [];

        var headerRect = new Rect(0f, curY, viewRect.width, 28f);
        if (Mouse.IsOver(headerRect))
            Widgets.DrawHighlight(headerRect);
        if (Widgets.ButtonInvisible(headerRect))
        {
            if (!expandedFactions.Add(faction))
                expandedFactions.Remove(faction);
        }

        var countRect = new Rect(headerRect.width - 180f, curY, 180f, 28f);
        var nameRect = new Rect(0f, curY, countRect.x, 28f);

        GUI.color = TerminalColors.HighlightColor;
        Text.Anchor = TextAnchor.MiddleLeft;
        string arrow = expanded ? "▼ " : "▶ ";
        Widgets.Label(nameRect, arrow + faction.LabelCap);
        GUI.color = DimPrimary;
        Text.Anchor = TextAnchor.MiddleRight;
        Widgets.Label(countRect, "FCP_Rank_Lineage_FolderCount".Translate(notableMembers.Count, CountEnrolled(tracks)));
        Text.Anchor = TextAnchor.UpperLeft;
        GUI.color = TerminalColors.PrimaryColor;
        curY += 30f;

        if (!expanded)
            return curY;

        foreach (CharacterDef characterDef in notableMembers.OrderBy(GetCharacterDisplayName))
            curY = DrawNotableMember(viewRect, curY, characterDef);

        if (notableMembers.Count > 0 && tracks.Count > 0)
            curY += 6f;

        foreach (RankTrackDef track in tracks)
        {
            List<Pawn> founders = GameComp.AllTracked
                .Where(kv => kv.Value.track == track && kv.Value.mentor == null)
                .Select(kv => kv.Key)
                .OrderBy(p => p.LabelShortCap)
                .ToList();

            if (founders.Count == 0)
                continue;

            var visited = new HashSet<Pawn>();
            foreach (Pawn founder in founders)
                curY = DrawLineageNode(viewRect, curY, founder, visited, 1);
        }

        curY += 14f;
        return curY;
    }

    private int CountEnrolled(List<RankTrackDef> tracks)
    {
        return GameComp.AllTracked.Count(kv => tracks.Contains(kv.Value.track));
    }

    private float DrawNotableMember(Rect viewRect, float curY, CharacterDef characterDef)
    {
        var lineRect = new Rect(24f, curY, viewRect.width - 24f, 22f);

        bool isLeader = IsCurrentFactionLeader(characterDef);
        RankTierDef tier = characterDef.definitions.OfType<CharacterRankDefinition>().FirstOrDefault()?.tier;
        bool exists = UniqueCharactersTracker.Instance != null && UniqueCharactersTracker.Instance.CharacterPawnExists(characterDef);

        string name = GetCharacterDisplayName(characterDef);
        string tag = isLeader ? GetLeaderTag(characterDef) : tier?.LabelCap.ToString();
        string line = tag.NullOrEmpty() ? name : $"{name} - {tag}";

        if (exists && Mouse.IsOver(lineRect))
            Widgets.DrawHighlight(lineRect);

        GUI.color = exists ? TerminalColors.PrimaryColor : DimPrimary;
        Widgets.Label(lineRect, line);
        GUI.color = TerminalColors.PrimaryColor;

        if (exists && Widgets.ButtonInvisible(lineRect))
        {
            Pawn pawn = UniqueCharactersTracker.Instance.GetOrGenPawn(characterDef);
            if (allColonists.Contains(pawn))
            {
                selectedPawn = pawn;
                curTab = RosterTab.Order;
            }
        }

        return curY + 24f;
    }

    private static bool IsCurrentFactionLeader(CharacterDef characterDef)
    {
        if (characterDef.faction == null || UniqueCharactersTracker.Instance == null)
            return false;

        Faction faction = Find.FactionManager.FirstFactionOfDef(characterDef.faction);
        if (faction?.leader == null)
            return false;

        return UniqueCharactersTracker.Instance.TryGetPawnCharacter(faction.leader, out UniqueCharacter character)
            && character.def == characterDef;
    }

    private static string GetLeaderTag(CharacterDef characterDef)
    {
        string title = characterDef.faction?.leaderTitle;
        return !title.NullOrEmpty() ? title.CapitalizeFirst() : "FCP_Rank_Lineage_Leader".Translate().ToString();
    }

    private static string GetCharacterDisplayName(CharacterDef characterDef)
    {
        CharacterStoryDefinition story = characterDef.definitions.OfType<CharacterStoryDefinition>().FirstOrDefault();
        if (story == null)
            return characterDef.LabelCap.ToString();

        if (!story.nickname.NullOrEmpty())
            return story.nickname;

        string full = string.Join(" ", new[] { story.firstName, story.lastName }.Where(s => !s.NullOrEmpty()));
        return full.NullOrEmpty() ? characterDef.LabelCap.ToString() : full;
    }

    private float DrawLineageNode(Rect viewRect, float curY, Pawn pawn, HashSet<Pawn> visited, int depth)
    {
        if (!visited.Add(pawn))
            return curY;

        PawnRankData data = GameComp.GetRankData(pawn);
        string rankLabel = data?.currentTier?.LabelCap ?? "";
        string line = depth <= 1 || data?.mentor == null
            ? "FCP_Rank_Lineage_Founder".Translate(pawn.LabelShortCap, rankLabel)
            : "FCP_Rank_Lineage_Trained".Translate(pawn.LabelShortCap, rankLabel, data.mentor.LabelShortCap);

        var lineRect = new Rect(depth * 24f, curY, viewRect.width - depth * 24f, 22f);
        Widgets.Label(lineRect, line);
        curY += 24f;

        List<Pawn> trainees = GameComp.AllTracked
            .Where(kv => kv.Value.mentor == pawn)
            .Select(kv => kv.Key)
            .OrderBy(p => p.LabelShortCap)
            .ToList();

        foreach (Pawn trainee in trainees)
            curY = DrawLineageNode(viewRect, curY, trainee, visited, depth + 1);

        return curY;
    }

    private void DrawRecordsTab(Rect rect)
    {
        GUI.color = TerminalColors.PrimaryColor;

        var entries = GameComp.AllTracked
            .SelectMany(kv => kv.Value.history.Select(h => (pawn: kv.Key, entry: h)))
            .OrderByDescending(x => x.entry.tick)
            .Take(MaxRecordsShown)
            .ToList();

        if (entries.Count == 0)
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = DimPrimary;
            Widgets.Label(rect, "FCP_Rank_Records_Empty".Translate());
            GUI.color = TerminalColors.PrimaryColor;
            Text.Anchor = TextAnchor.UpperLeft;
            return;
        }

        var viewRect = new Rect(0f, 0f, rect.width - 16f, entries.Count * 26f + 20f);
        Widgets.BeginScrollView(rect, ref recordsScrollPos, viewRect);

        float curY = 0f;
        foreach (var (pawn, entry) in entries)
        {
            int daysAgo = (Find.TickManager.TicksGame - entry.tick) / GenDate.TicksPerDay;
            string line = "FCP_Rank_Records_Entry".Translate(daysAgo, pawn.LabelShortCap, entry.text);
            float height = Text.CalcHeight(line, viewRect.width);

            GUI.color = DimPrimary;
            Widgets.Label(new Rect(0f, curY, viewRect.width, height), line);
            GUI.color = TerminalColors.PrimaryColor;
            curY += height + 4f;
        }

        Widgets.EndScrollView();
    }

    private float DrawSuspendedStatus(Rect viewRect, float curY, PawnRankData data)
    {
        GUI.color = TerminalColors.HighlightColor;
        Widgets.Label(new Rect(0f, curY, viewRect.width, 22f), "FCP_Rank_Suspended_Title".Translate());
        GUI.color = DimPrimary;
        curY += 22f;

        float height = Text.CalcHeight("FCP_Rank_Suspended_Text".Translate(data.track.label), viewRect.width);
        Widgets.Label(new Rect(0f, curY, viewRect.width, height), "FCP_Rank_Suspended_Text".Translate(data.track.label));
        curY += height + 4f;

        GUI.color = TerminalColors.PrimaryColor;
        return curY;
    }

    private float DrawMentorSection(Rect viewRect, float curY, PawnRankData data)
    {
        GUI.color = TerminalColors.PrimaryColor;

        var rowRect = new Rect(0f, curY, viewRect.width, 24f);
        var labelRect = new Rect(rowRect.x, rowRect.y, 100f, rowRect.height);
        var buttonRect = new Rect(labelRect.xMax, rowRect.y, 220f, rowRect.height);

        Text.Anchor = TextAnchor.MiddleLeft;
        Widgets.Label(labelRect, "FCP_Rank_Mentor".Translate());
        Text.Anchor = TextAnchor.UpperLeft;

        string mentorLabel = data.mentor?.LabelShortCap ?? "FCP_Rank_NoMentor".Translate().ToString();
        if (Widgets.ButtonText(buttonRect, mentorLabel))
            OpenMentorMenu(data);

        return curY + 28f;
    }

    private float DrawAwayStatus(Rect viewRect, float curY, ExpeditionData expeditionData)
    {
        int daysLeft = Mathf.Max(0, (expeditionData.returnTick - Find.TickManager.TicksGame) / GenDate.TicksPerDay);
        GUI.color = TerminalColors.HighlightColor;
        Widgets.Label(new Rect(0f, curY, viewRect.width, 22f), "FCP_Rank_OnExpedition".Translate(expeditionData.expedition.LabelCap));
        GUI.color = TerminalColors.PrimaryColor;
        curY += 22f;
        Widgets.Label(new Rect(0f, curY, viewRect.width, 20f), "FCP_Rank_ExpeditionReturnsIn".Translate(daysLeft));
        curY += 24f;

        if (Prefs.DevMode)
        {
            var buttonRect = new Rect(0f, curY, 260f, 28f);
            GUI.color = Color.red;
            if (Widgets.ButtonText(buttonRect, "FCP_Rank_DevForceReturn".Translate()))
                ExpeditionComp.ForceReturn(selectedPawn);
            GUI.color = TerminalColors.PrimaryColor;
            curY += 32f;
        }

        return curY;
    }

    private float DrawExpeditionSection(Rect viewRect, float curY, RankExpeditionDef expedition)
    {
        GUI.color = TerminalColors.PrimaryColor;

        var buttonRect = new Rect(0f, curY, 260f, 28f);
        if (Widgets.ButtonText(buttonRect, "FCP_Rank_SendOnExpedition".Translate(expedition.LabelCap)))
            ExpeditionComp.Send(selectedPawn, expedition);

        return curY + 32f;
    }

    private void OpenMentorMenu(PawnRankData data)
    {
        var options = new List<FloatMenuOption>
        {
            new FloatMenuOption("FCP_Rank_NoMentor".Translate(), () => GameComp.SetMentor(selectedPawn, null))
        };

        foreach (Pawn candidate in allColonists)
        {
            if (candidate == selectedPawn)
                continue;

            PawnRankData candidateData = GameComp.GetRankData(candidate);
            if (candidateData == null || candidateData.track != data.track)
                continue;

            options.Add(new FloatMenuOption(candidate.LabelShortCap, () => GameComp.SetMentor(selectedPawn, candidate)));
        }

        Find.WindowStack.Add(new FloatMenu(options));
    }

    private float DrawPromotionOptions(Rect viewRect, float curY, PawnRankData data, List<RankTierDef> options)
    {
        Text.Font = GameFont.Tiny;
        GUI.color = DimPrimary;
        Widgets.Label(new Rect(0f, curY, viewRect.width, 18f),
            options.Count > 1 ? "FCP_Rank_ChoosePath".Translate() : "FCP_Rank_NextRank".Translate());
        GUI.color = TerminalColors.PrimaryColor;
        Text.Font = GameFont.Small;
        curY += 20f;

        foreach (RankTierDef option in options)
            curY = DrawPromotionOption(viewRect, curY, data, option);

        return curY;
    }

    private float DrawPromotionOption(Rect viewRect, float curY, PawnRankData data, RankTierDef option)
    {
        GUI.color = TerminalColors.PrimaryColor;

        if (!option.pathName.NullOrEmpty())
        {
            Widgets.Label(new Rect(0f, curY, viewRect.width, 20f), option.pathName);
            curY += 22f;
        }

        Widgets.Label(new Rect(0f, curY, viewRect.width, 20f), option.LabelCap);
        curY += 22f;

        bool allMet = true;

        int daysInRank = (Find.TickManager.TicksGame - data.rankStartTick) / GenDate.TicksPerDay;
        if (option.minDaysInRank > 0)
        {
            allMet &= DrawRequirementLine(viewRect, ref curY, "FCP_Rank_Req_Days".Translate(daysInRank, option.minDaysInRank),
                daysInRank >= option.minDaysInRank);
        }

        if (!option.skillRequirements.NullOrEmpty())
        {
            foreach (SkillRequirement requirement in option.skillRequirements)
            {
                allMet &= DrawRequirementLine(viewRect, ref curY, requirement.Summary, requirement.PawnSatisfies(selectedPawn));
            }
        }

        if (option.minSquiresTrained > 0)
        {
            allMet &= DrawRequirementLine(viewRect, ref curY,
                "FCP_Rank_Req_SquiresTrained".Translate(data.squiresTrained, option.minSquiresTrained),
                data.squiresTrained >= option.minSquiresTrained);
        }

        if (option.minMissionsCompleted > 0)
        {
            allMet &= DrawRequirementLine(viewRect, ref curY,
                "FCP_Rank_Req_MissionsCompleted".Translate(data.missionsCompleted, option.minMissionsCompleted),
                data.missionsCompleted >= option.minMissionsCompleted);
        }

        bool devForce = !allMet && Prefs.DevMode;

        GUI.color = allMet ? TerminalColors.PrimaryColor : (devForce ? Color.red : DimPrimary);
        var buttonRect = new Rect(0f, curY, 200f, 28f);
        string buttonLabel = devForce ? "FCP_Rank_DevForcePromote".Translate() : "FCP_Rank_Promote".Translate();
        if (Widgets.ButtonText(buttonRect, buttonLabel) && (allMet || devForce))
            GameComp.Promote(selectedPawn, option);
        GUI.color = TerminalColors.PrimaryColor;

        return curY + 36f;
    }

    private static bool DrawRequirementLine(Rect viewRect, ref float curY, string label, bool met)
    {
        GUI.color = met ? new Color(0.4f, 1f, 0.4f) : new Color(1f, 0.4f, 0.4f);
        Widgets.Label(new Rect(12f, curY, viewRect.width - 12f, 18f), (met ? "✓ " : "✗ ") + label);
        GUI.color = TerminalColors.PrimaryColor;
        curY += 20f;
        return met;
    }

    private static IEnumerable<RankTrackDef> GetAllTracks()
    {
        var seen = new HashSet<RankTrackDef>();
        foreach (FactionDef factionDef in DefDatabase<FactionDef>.AllDefsListForReading)
        {
            FactionExtension_RankTracks extension = factionDef.GetModExtension<FactionExtension_RankTracks>();
            if (extension?.rankTracks == null)
                continue;

            foreach (RankTrackDef track in extension.rankTracks)
            {
                if (seen.Add(track))
                    yield return track;
            }
        }
    }
}
