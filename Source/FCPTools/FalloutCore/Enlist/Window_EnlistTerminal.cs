using FCP.Core;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace FCP.Enlist;

public class Window_EnlistTerminal : Window
{
    private enum EnlistTab
    {
        Overview,
        Missions,
        Trade,
        Work,
        Command
    }

    private const float TabBarHeight = 32f;
    private const float PanelMargin = 10f;
    private const float ContentIndent = 12f;
    private const float ButtonHeight = 30f;
    private const float ButtonPadding = 24f;
    private const float MinButtonWidth = 200f;
    private const float TitleIconSize = 28f;

    private readonly Caravan caravan;
    private readonly WorldObjectCompEnlist comp;
    private readonly FactionEnlistOptionsDef options;
    private readonly Faction faction;

    private EnlistTab curTab = EnlistTab.Overview;
    private Vector2 overviewScrollPos;
    private Vector2 missionsScrollPos;
    private Vector2 tradeScrollPos;
    private Vector2 workScrollPos;
    private Vector2 commandScrollPos;
    private bool confirmingResign;

    public override Vector2 InitialSize => new Vector2(900f, 620f);
    protected override float Margin => 0f;

    private static Color DimPrimary => new Color(TerminalColors.PrimaryColor.r, TerminalColors.PrimaryColor.g, TerminalColors.PrimaryColor.b, 0.5f);

    public Window_EnlistTerminal(Caravan caravan, WorldObjectCompEnlist comp, FactionEnlistOptionsDef options)
    {
        this.caravan = caravan;
        this.comp = comp;
        this.options = options;
        faction = comp.parent.Faction;

        doCloseX = true;
        absorbInputAroundWindow = true;
        closeOnClickedOutside = false;
        curTab = EnlistTab.Overview;

        if (options.missionsAreEnabled)
        {
            BountyBoard board = comp.GetBountyBoard(options);
            if (board.lastRefreshTick == 0 || Find.TickManager.TicksGame > board.lastRefreshTick + (30 * GenDate.TicksPerDay))
            {
                comp.RefreshBountyBoard(options);
            }
        }
    }

    private bool Enlisted => WorldEnlistTracker.Instance.EnlistedTo(faction, options);

    // Plain-text translate helpers: strips embedded rich-text (e.g. a faction's own name color)
    // so every label reads in a single, consistent terminal color instead of clashing tones.
    private static string Tr(string key) => ((string)key.Translate()).StripTags();
    private static string Tr(string key, NamedArgument arg) => ((string)key.Translate(arg)).StripTags();

    private static float ButtonWidth(string label, float min = MinButtonWidth)
    {
        return Mathf.Max(min, Text.CalcSize(label).x + ButtonPadding);
    }

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Medium;
        GUI.color = TerminalColors.PrimaryColor;

        float titleX = ContentIndent;
        Texture2D factionIcon = faction.def.FactionIcon;
        if (factionIcon != null)
        {
            GUI.color = faction.def.DefaultColor;
            GUI.DrawTexture(new Rect(titleX, 1f, TitleIconSize, TitleIconSize), factionIcon);
            GUI.color = TerminalColors.PrimaryColor;
            titleX += TitleIconSize + 8f;
        }
        Widgets.Label(new Rect(titleX, 0f, inRect.width - 40f - titleX, 30f), Tr("FCP_Enlist_TerminalTitle", faction.Named("FACTION")));
        Text.Font = GameFont.Small;

        var tabBarRect = new Rect(0f, 40f, inRect.width, TabBarHeight);
        DrawTabBar(tabBarRect);

        var contentRect = new Rect(0f, tabBarRect.yMax + 4f, inRect.width, inRect.height - tabBarRect.yMax - 4f);

        GUI.BeginGroup(contentRect);
        var localRect = new Rect(0f, 0f, contentRect.width, contentRect.height);
        switch (curTab)
        {
            case EnlistTab.Missions:
                DrawMissionsTab(localRect);
                break;
            case EnlistTab.Trade:
                DrawTradeTab(localRect);
                break;
            case EnlistTab.Work:
                DrawWorkTab(localRect);
                break;
            case EnlistTab.Command:
                DrawCommandTab(localRect);
                break;
            default:
                DrawOverviewTab(localRect);
                break;
        }
        GUI.EndGroup();

        GUI.color = Color.white;
    }

    private void DrawTabBar(Rect rect)
    {
        var tabs = new List<(EnlistTab tab, string label)>
        {
            (EnlistTab.Overview, Tr("FCP_Enlist_Tab_Overview")),
        };

        if (Enlisted)
        {
            if (options.missionsAreEnabled)
                tabs.Add((EnlistTab.Missions, Tr("FCP_Enlist_Tab_Missions")));
            tabs.Add((EnlistTab.Trade, Tr("FCP_Enlist_Tab_Trade")));
            if (!options.workOptions.NullOrEmpty() || options.autoFeedIsEnabled || options.abilityTrainingIsEnabled)
                tabs.Add((EnlistTab.Work, Tr("FCP_Enlist_Tab_Work")));
            if (options.dropPodServiceIsEnabled || options.shuttleServiceIsEnabled || !options.protocolOptions.NullOrEmpty())
                tabs.Add((EnlistTab.Command, Tr("FCP_Enlist_Tab_Command")));
        }

        if (!tabs.Any(t => t.tab == curTab))
            curTab = EnlistTab.Overview;

        float tabWidth = rect.width / tabs.Count;
        for (int i = 0; i < tabs.Count; i++)
        {
            DrawTabButton(new Rect(rect.x + tabWidth * i, rect.y, tabWidth, rect.height), tabs[i].label, tabs[i].tab);
        }

        GUI.color = TerminalColors.PrimaryColor;
        Widgets.DrawLineHorizontal(rect.x, rect.yMax, rect.width);
    }

    private void DrawTabButton(Rect rect, string label, EnlistTab tab)
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
        {
            curTab = tab;
            confirmingResign = false;
        }
    }

    // ============================== Overview ==============================

    private void DrawOverviewTab(Rect rect)
    {
        var viewRect = new Rect(0f, 0f, rect.width - 16f, 1000f);
        Widgets.BeginScrollView(rect, ref overviewScrollPos, viewRect);

        float curY = 0f;
        GUI.color = TerminalColors.PrimaryColor;

        if (!Enlisted)
        {
            curY = DrawNotEnlistedPanel(viewRect, curY);
        }
        else
        {
            curY = DrawEnlistedStatus(viewRect, curY);
            curY += 10f;

            if (options.promoteOptionEnabled)
            {
                curY = DrawPromotionPanel(viewRect, curY);
                curY += 10f;
            }

            curY = DrawResignPanel(viewRect, curY);
        }

        Widgets.EndScrollView();
    }

    private float DrawNotEnlistedPanel(Rect viewRect, float curY)
    {
        WorldEnlistTracker tracker = WorldEnlistTracker.Instance;

        Widgets.Label(new Rect(ContentIndent, curY, viewRect.width - ContentIndent, 22f), Tr("FCP_Enlist_NotEnlisted", faction.Named("FACTION")));
        curY += 26f;

        bool canEnlist = tracker.CanEnlist(faction, options, out string cannotEnlistReason);
        string enlistLabel = Tr(options.enlistButtonLabelKey, faction.Named("FACTION"));
        var enlistRect = new Rect(ContentIndent, curY, ButtonWidth(enlistLabel), ButtonHeight);
        GUI.color = canEnlist ? TerminalColors.PrimaryColor : DimPrimary;
        if (Widgets.ButtonText(enlistRect, enlistLabel) && canEnlist)
        {
            options.Worker.EnlistTo(faction);
        }
        GUI.color = TerminalColors.PrimaryColor;
        curY += ButtonHeight + 4f;

        if (!canEnlist && !cannotEnlistReason.NullOrEmpty())
        {
            float reasonHeight = Text.CalcHeight(cannotEnlistReason, viewRect.width - ContentIndent);
            GUI.color = new Color(1f, 0.4f, 0.4f);
            Widgets.Label(new Rect(ContentIndent, curY, viewRect.width - ContentIndent, reasonHeight), cannotEnlistReason);
            GUI.color = TerminalColors.PrimaryColor;
            curY += reasonHeight + 8f;
        }

        if (options.buyOutOption != null && !tracker.Bought(faction, options))
        {
            string buyOutLabel = Tr(options.buyOutOption.buttonLabelKey, faction.Named("FACTION"));
            var buyOutRect = new Rect(ContentIndent, curY, ButtonWidth(buyOutLabel), ButtonHeight);
            if (Widgets.ButtonText(buyOutRect, buyOutLabel))
            {
                options.Worker.Buy(faction, caravan);
            }
            curY += ButtonHeight + 4f;
        }

        if (options.bountyHunterIsEnabled && options.bountyHunterTraderKind != null)
        {
            string bountyLabel = Tr(options.bountyHunterLabelKey, faction.Named("FACTION"));
            var bountyRect = new Rect(ContentIndent, curY, ButtonWidth(bountyLabel), ButtonHeight);
            if (Widgets.ButtonText(bountyRect, bountyLabel))
            {
                OpenBountyHunterTrade();
            }
            curY += ButtonHeight + 4f;
        }

        return curY;
    }

    private float DrawEnlistedStatus(Rect viewRect, float curY)
    {
        Text.Font = GameFont.Medium;
        Widgets.Label(new Rect(ContentIndent, curY, viewRect.width - ContentIndent, 26f), Tr(options.enlistedWithKey, faction.Named("FACTION")));
        Text.Font = GameFont.Small;
        curY += 30f;

        GUI.color = DimPrimary;
        Widgets.Label(new Rect(ContentIndent, curY, viewRect.width - ContentIndent, 20f), "FCP_Enlist_Goodwill".Translate(faction.GoodwillWith(Faction.OfPlayer)));
        GUI.color = TerminalColors.PrimaryColor;
        curY += 24f;

        return curY;
    }

    private float DrawPromotionPanel(Rect viewRect, float curY)
    {
        bool promoted = comp.IsPromoted(options);
        if (promoted)
            return curY;

        Text.Font = GameFont.Tiny;
        GUI.color = DimPrimary;
        Widgets.Label(new Rect(ContentIndent, curY, viewRect.width - ContentIndent, 18f), Tr("FCP_Enlist_Promotion"));
        GUI.color = TerminalColors.PrimaryColor;
        Text.Font = GameFont.Small;
        curY += 20f;

        bool allMet = true;
        if (!options.promoteSkillRequirements.NullOrEmpty())
        {
            Pawn bestPawn = caravan.PawnsListForReading.Where(p => p.IsColonist).OrderByDescending(
                p => options.promoteSkillRequirements.Count(r => r.PawnSatisfies(p))).FirstOrDefault();

            foreach (SkillRequirement requirement in options.promoteSkillRequirements)
            {
                bool met = bestPawn != null && requirement.PawnSatisfies(bestPawn);
                allMet &= DrawRequirementLine(viewRect, ref curY, requirement.Summary, met);
            }
        }

        GUI.color = allMet ? TerminalColors.PrimaryColor : DimPrimary;
        string promoteLabel = Tr(options.promoteButtonLabelKey, faction.Named("FACTION"));
        var promoteRect = new Rect(ContentIndent, curY, ButtonWidth(promoteLabel), ButtonHeight);
        if (Widgets.ButtonText(promoteRect, promoteLabel) && allMet)
        {
            comp.SetPromoted(options, true);
        }
        GUI.color = TerminalColors.PrimaryColor;
        curY += ButtonHeight + 8f;

        return curY;
    }

    private float DrawResignPanel(Rect viewRect, float curY)
    {
        curY += 10f;
        GUI.color = DimPrimary;
        Widgets.DrawLineHorizontal(ContentIndent, curY, viewRect.width - ContentIndent);
        GUI.color = TerminalColors.PrimaryColor;
        curY += 10f;

        if (!confirmingResign)
        {
            string resignLabel = Tr(options.resignButtonLabelKey, faction.Named("FACTION"));
            var resignRect = new Rect(ContentIndent, curY, ButtonWidth(resignLabel), ButtonHeight);
            GUI.color = new Color(1f, 0.4f, 0.4f);
            if (Widgets.ButtonText(resignRect, resignLabel))
            {
                confirmingResign = true;
            }
            GUI.color = TerminalColors.PrimaryColor;
            curY += ButtonHeight + 4f;
        }
        else
        {
            string resignText = Tr(options.resignMenuTextKey, faction.Named("FACTION"));
            float height = Text.CalcHeight(resignText, viewRect.width - ContentIndent);
            Widgets.Label(new Rect(ContentIndent, curY, viewRect.width - ContentIndent, height), resignText);
            curY += height + 4f;

            var confirmRect = new Rect(ContentIndent, curY, ButtonWidth("Yes".Translate(), 100f), ButtonHeight);
            GUI.color = new Color(1f, 0.4f, 0.4f);
            if (Widgets.ButtonText(confirmRect, "Yes".Translate()))
            {
                WorldEnlistTracker.Instance.Delist(faction, options);
                confirmingResign = false;
                Close();
            }
            GUI.color = TerminalColors.PrimaryColor;

            var cancelRect = new Rect(confirmRect.xMax + 10f, curY, ButtonWidth("No".Translate(), 100f), ButtonHeight);
            if (Widgets.ButtonText(cancelRect, "No".Translate()))
            {
                confirmingResign = false;
            }
            curY += ButtonHeight + 4f;
        }

        return curY;
    }

    // ============================== Missions ==============================

    private void DrawMissionsTab(Rect rect)
    {
        if (!options.missionsAreEnabled)
        {
            DrawCenteredMessage(rect, Tr("FCP_Enlist_NotAvailable"));
            return;
        }

        GUI.color = DimPrimary;
        Widgets.Label(new Rect(ContentIndent, rect.y, rect.width - ContentIndent, 20f), Tr(options.missionsMenuDescriptionKey));
        GUI.color = TerminalColors.PrimaryColor;

        var listRect = new Rect(rect.x, rect.y + 26f, rect.width, rect.height - 26f);

        BountyBoard board = comp.GetBountyBoard(options);
        if (board.available.NullOrEmpty() && board.accepted.NullOrEmpty())
        {
            DrawCenteredMessage(listRect, Tr("FCP_Enlist_NoMissions"));
            return;
        }

        ThingDef currencyDef = options.currencyDef ?? ThingDefOf.Silver;
        string acceptLabel = Tr(options.missionsMenuStartQuestKey);
        string collectLabel = Tr(options.missionsMenuCollectBountyKey);
        float actionWidth = Mathf.Max(ButtonWidth(acceptLabel, 160f), ButtonWidth(collectLabel, 160f));

        var viewRect = new Rect(0f, 0f, listRect.width - 16f - ContentIndent, (board.accepted.Count + board.available.Count) * 46f);
        Widgets.BeginScrollView(listRect, ref missionsScrollPos, viewRect);

        float curY = 0f;
        foreach (GeneratedBounty bounty in board.accepted.ToList())
        {
            bool readyToCollect = bounty.ReadyToCollect;
            GeneratedBounty localBounty = bounty;
            curY = DrawBountyRow(viewRect, curY, localBounty, currencyDef, actionWidth,
                readyToCollect ? collectLabel : null, readyToCollect,
                () => comp.CollectBounty(localBounty, options, caravan));
        }

        foreach (GeneratedBounty bounty in board.available.ToList())
        {
            GeneratedBounty localBounty = bounty;
            curY = DrawBountyRow(viewRect, curY, localBounty, currencyDef, actionWidth,
                acceptLabel, true,
                () => comp.AcceptBounty(localBounty, options));
        }

        Widgets.EndScrollView();
    }

    private float DrawBountyRow(Rect viewRect, float curY, GeneratedBounty bounty, ThingDef currencyDef, float actionWidth, string actionLabel, bool actionEnabled, Action onAction)
    {
        var rowRect = new Rect(ContentIndent, curY, viewRect.width - ContentIndent, 40f);
        if (Mouse.IsOver(rowRect))
            Widgets.DrawHighlight(rowRect);

        var nameRect = new Rect(ContentIndent, curY, viewRect.width - ContentIndent - actionWidth - 10f, 40f);
        Text.Anchor = TextAnchor.MiddleLeft;
        string rewardPart = $"{bounty.quest.name} ({bounty.reward} {currencyDef.label})";
        string line = bounty.quest.State == QuestState.Ongoing ? $"{rewardPart} - {Tr("FCP_Enlist_BountyInProgress")}" : rewardPart;
        Widgets.Label(nameRect, line);
        Text.Anchor = TextAnchor.UpperLeft;

        if (!actionLabel.NullOrEmpty())
        {
            var actionRect = new Rect(viewRect.width - actionWidth, curY + 4f, actionWidth, 32f);
            GUI.color = actionEnabled ? TerminalColors.PrimaryColor : DimPrimary;
            if (Widgets.ButtonText(actionRect, actionLabel) && actionEnabled)
            {
                onAction();
            }
            GUI.color = TerminalColors.PrimaryColor;
        }

        return curY + 46f;
    }

    // ============================== Trade ==============================

    private void DrawTradeTab(Rect rect)
    {
        var viewRect = new Rect(0f, 0f, rect.width - 16f, 900f);
        Widgets.BeginScrollView(rect, ref tradeScrollPos, viewRect);

        float curY = 0f;
        WorldEnlistTracker tracker = WorldEnlistTracker.Instance;
        FactionOptions factionOptions = tracker.factionOptionsContainer[faction];

        if (options.salaryIsEnabled)
        {
            curY = DrawSectionHeader(viewRect, curY, Tr("FCP_Enlist_Salary"));
            SalaryInfo salaryInfo = factionOptions.factionsSalaries[options];
            bool canPay = salaryInfo.CanPayMoney(options);
            string salaryLabel = Tr(options.salaryLabelKey, faction.Named("FACTION"));
            GUI.color = canPay ? TerminalColors.PrimaryColor : DimPrimary;
            var salaryRect = new Rect(ContentIndent, curY, ButtonWidth(salaryLabel), ButtonHeight);
            if (Widgets.ButtonText(salaryRect, salaryLabel) && canPay)
            {
                salaryInfo.GiveMoney(options, caravan, faction);
            }
            GUI.color = TerminalColors.PrimaryColor;
            curY += ButtonHeight + 8f;
        }

        if (!options.provisionOptions.NullOrEmpty())
        {
            curY = DrawSectionHeader(viewRect, curY, Tr("FCP_Enlist_Provisions"));
            comp.provisionInfos ??= new Dictionary<int, ProvisionsInfo>();
            curY = DrawProvisionButtons(viewRect, curY, comp.provisionInfos, options.provisionOptions);
        }

        if (comp.IsPromoted(options) && !options.promoteProvisionOptions.NullOrEmpty())
        {
            curY = DrawSectionHeader(viewRect, curY, Tr("FCP_Enlist_PromotedProvisions"));
            comp.promotedProvisionInfos ??= new Dictionary<int, ProvisionsInfo>();
            curY = DrawProvisionButtons(viewRect, curY, comp.promotedProvisionInfos, options.promoteProvisionOptions);
        }

        if (options.storageIsEnabled)
        {
            curY = DrawSectionHeader(viewRect, curY, Tr("FCP_Enlist_Storage"));
            string storageLabel = Tr(options.storageLabelKey, faction.Named("FACTION"));
            var storageRect = new Rect(ContentIndent, curY, ButtonWidth(storageLabel), ButtonHeight);
            if (Widgets.ButtonText(storageRect, storageLabel))
            {
                DiaNode dianode = new DiaNode("Storage");
                Find.WindowStack.Add(new Dialog_FactionStorage(dianode, false, caravan, factionOptions.factionsStorages[options]));
            }
            curY += ButtonHeight + 8f;
        }

        if (options.exclusiveTraderIsEnabled || options.taxCollectorIsEnabled || options.turnInIsEnabled)
        {
            curY = DrawSectionHeader(viewRect, curY, Tr("FCP_Enlist_Traders"));

            if (options.exclusiveTraderIsEnabled && options.exclusiveTraderKind != null)
            {
                bool meetsGoodwill = faction.GoodwillWith(Faction.OfPlayer) >= options.exclusiveTraderRequiredGoodwill;
                bool meetsTitle = options.exclusiveTraderRequiredTitle == null ||
                    PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists.Any(
                        p => p.royalty != null && p.royalty.GetCurrentTitleInFaction(faction)?.def.seniority >= options.exclusiveTraderRequiredTitle.seniority);
                bool canTrade = meetsGoodwill && meetsTitle;

                string exclusiveLabel = Tr(options.exclusiveTraderLabelKey, faction.Named("FACTION"));
                GUI.color = canTrade ? TerminalColors.PrimaryColor : DimPrimary;
                var rowRect = new Rect(ContentIndent, curY, ButtonWidth(exclusiveLabel), ButtonHeight);
                if (Widgets.ButtonText(rowRect, exclusiveLabel) && canTrade)
                {
                    OpenExclusiveTrade();
                }
                GUI.color = TerminalColors.PrimaryColor;
                curY += ButtonHeight + 4f;
            }

            if (options.taxCollectorIsEnabled && options.taxCollectorTraderKind != null)
            {
                string taxLabel = Tr(options.taxCollectorLabelKey, faction.Named("FACTION"));
                var rowRect = new Rect(ContentIndent, curY, ButtonWidth(taxLabel), ButtonHeight);
                if (Widgets.ButtonText(rowRect, taxLabel))
                {
                    OpenTaxCollectorTrade();
                }
                curY += ButtonHeight + 4f;
            }

            if (options.turnInIsEnabled)
            {
                string turnInLabel = Tr(options.turnInLabelKey, faction.Named("FACTION"));
                var rowRect = new Rect(ContentIndent, curY, ButtonWidth(turnInLabel), ButtonHeight);
                if (Widgets.ButtonText(rowRect, turnInLabel))
                {
                    OpenTurnInTrade();
                }
                curY += ButtonHeight + 4f;
            }
            curY += 4f;
        }

        if (options.mechSerumIsEnabled)
        {
            curY = DrawSectionHeader(viewRect, curY, Tr("FCP_Enlist_MechSerum"));
            ThingDef mechSerumCurrency = options.currencyDef ?? ThingDefOf.Silver;
            bool canAfford = caravan.AllThings.Where(x => x.def == mechSerumCurrency).Sum(x => x.stackCount) >= options.mechSerumCost;
            string mechSerumLabel = Tr(options.mechSerumLabelKey, faction.Named("FACTION"));
            GUI.color = canAfford ? TerminalColors.PrimaryColor : DimPrimary;
            var rowRect = new Rect(ContentIndent, curY, ButtonWidth(mechSerumLabel), ButtonHeight);
            if (Widgets.ButtonText(rowRect, mechSerumLabel) && canAfford)
            {
                comp.UseMechSerum(caravan, options);
            }
            GUI.color = TerminalColors.PrimaryColor;
            curY += ButtonHeight + 8f;
        }

        if (options.deliveryQuestsIsEnabled && !options.deliveryQuestTemplates.NullOrEmpty())
        {
            curY = DrawSectionHeader(viewRect, curY, Tr("FCP_Enlist_DeliveryQuests"));
            string boardLabel = Tr(options.deliveryQuestsBoardLabelKey, faction.Named("FACTION"));
            var boardRect = new Rect(ContentIndent, curY, ButtonWidth(boardLabel), ButtonHeight);
            if (Widgets.ButtonText(boardRect, boardLabel))
            {
                OpenDeliveryBoard();
            }
            curY += ButtonHeight + 4f;

            DeliveryQuest turnIn = GetTurnInDeliveryQuest(factionOptions);
            if (turnIn != null)
            {
                string deliveryTurnInLabel = Tr(options.deliveryQuestsTurnInLabelKey, faction.Named("FACTION"));
                var turnInRect = new Rect(ContentIndent, curY, ButtonWidth(deliveryTurnInLabel), ButtonHeight);
                if (Widgets.ButtonText(turnInRect, deliveryTurnInLabel) && turnIn.CaravanCanTurnIn(caravan))
                {
                    turnIn.TurnIn(caravan, options);
                    factionOptions.activeDeliveries[options].quests.Remove(turnIn);
                }
                curY += ButtonHeight + 4f;
            }
        }

        Widgets.EndScrollView();
    }

    private float DrawProvisionButtons(Rect viewRect, float curY, Dictionary<int, ProvisionsInfo> provisionInfos, List<ProvisionOption> provisionOptions)
    {
        for (int i = 0; i < provisionOptions.Count; i++)
        {
            ProvisionOption provisionOption = provisionOptions[i];
            if (!provisionInfos.TryGetValue(i, out ProvisionsInfo info))
                provisionInfos[i] = info = new ProvisionsInfo();

            bool canGive = info.CanGiveProvisions(provisionOption);
            string provisionLabel = Tr(provisionOption.provisionsLabelKey, faction.Named("FACTION"));
            GUI.color = canGive ? TerminalColors.PrimaryColor : DimPrimary;
            var rowRect = new Rect(ContentIndent, curY, ButtonWidth(provisionLabel), ButtonHeight);
            if (Widgets.ButtonText(rowRect, provisionLabel) && canGive)
            {
                info.GiveProvisions(provisionOption, caravan);
            }
            GUI.color = TerminalColors.PrimaryColor;
            curY += ButtonHeight + 4f;
        }
        return curY + 4f;
    }

    private DeliveryQuest GetTurnInDeliveryQuest(FactionOptions factionOptions)
    {
        if (factionOptions.activeDeliveries == null || !factionOptions.activeDeliveries.TryGetValue(options, out DeliveryQuestList list))
            return null;
        return list.quests.FirstOrDefault(q => q.destinationTile == comp.parent.Tile && !q.IsExpired);
    }

    private void OpenDeliveryBoard()
    {
        comp.deliveryBoards ??= new Dictionary<FactionEnlistOptionsDef, DeliveryQuestList>();
        if (!comp.deliveryBoards.ContainsKey(options))
            comp.deliveryBoards[options] = new DeliveryQuestList();

        DeliveryQuestList board = comp.deliveryBoards[options];
        bool needsRefresh = board.lastRefreshTick == 0 || Find.TickManager.TicksGame > board.lastRefreshTick + (options.deliveryQuestsRerollDays * GenDate.TicksPerDay);
        if (needsRefresh)
            comp.RefreshDeliveryBoard(options);
        board.quests.RemoveAll(q => q.IsExpired || q.accepted);
        if (!board.quests.Any())
        {
            Messages.Message("FCP_NoDeliveryQuestsAvailable".Translate(), MessageTypeDefOf.RejectInput, historical: false);
            return;
        }

        WorldEnlistTracker tracker = WorldEnlistTracker.Instance;
        List<FloatMenuOption> questOptions = new List<FloatMenuOption>();
        foreach (DeliveryQuest quest in board.quests)
        {
            DeliveryQuest localQuest = quest;
            Settlement dest = Find.WorldObjects.AllWorldObjects.OfType<Settlement>().FirstOrDefault(s => s.Tile == localQuest.destinationTile);
            string destName = dest?.LabelShort ?? localQuest.destinationTile.ToString();
            string stuffPart = localQuest.stuff != null ? " (" + localQuest.stuff.label + ")" : "";
            ThingDef rewardItem = localQuest.rewardDef ?? options.salaryDef ?? ThingDefOf.Silver;
            string label = $"{localQuest.count}x {localQuest.thingToDeliver.label}{stuffPart} -> {destName}: {localQuest.reward} {rewardItem.label}";
            questOptions.Add(new FloatMenuOption(label, delegate
            {
                localQuest.accepted = true;
                FactionOptions factionOpts = tracker.factionOptionsContainer[faction];
                factionOpts.activeDeliveries ??= new Dictionary<FactionEnlistOptionsDef, DeliveryQuestList>();
                if (!factionOpts.activeDeliveries.ContainsKey(options))
                    factionOpts.activeDeliveries[options] = new DeliveryQuestList();
                factionOpts.activeDeliveries[options].quests.Add(localQuest);
            }));
        }
        Find.WindowStack.Add(new FloatMenu(questOptions));
    }

    private void OpenBountyHunterTrade()
    {
        PawnTrader trader = comp.GetOrMakeBountyHunterTrader(options);
        trader.caravan = caravan;
        Pawn negotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan, faction, options.bountyHunterTraderKind);
        Find.WindowStack.Add(new Dialog_Trade(negotiator, trader));
    }

    private void OpenExclusiveTrade()
    {
        ExclusiveTrader trader = comp.GetOrMakeExclusiveTrader(options);
        trader.caravan = caravan;
        Pawn negotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan, faction, options.exclusiveTraderKind);
        Find.WindowStack.Add(new Dialog_Trade(negotiator, trader));
    }

    private void OpenTaxCollectorTrade()
    {
        ExclusiveTrader trader = comp.GetOrMakeTaxCollectorTrader(options);
        trader.caravan = caravan;
        Pawn negotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan, faction, options.taxCollectorTraderKind);
        Find.WindowStack.Add(new Dialog_Trade(negotiator, trader));
    }

    private void OpenTurnInTrade()
    {
        PawnTrader trader = comp.GetOrMakeTurnInTrader(options);
        trader.caravan = caravan;
        Pawn negotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan, faction, options.turnInTraderKind);
        Find.WindowStack.Add(new Dialog_Trade(negotiator, trader));
    }

    // ============================== Work ==============================

    private void DrawWorkTab(Rect rect)
    {
        var viewRect = new Rect(0f, 0f, rect.width - 16f, 700f);
        Widgets.BeginScrollView(rect, ref workScrollPos, viewRect);

        float curY = 0f;
        CaravanOptions caravanOptions = comp.GetCaravanOptions(caravan);

        if (!options.workOptions.NullOrEmpty())
        {
            curY = DrawSectionHeader(viewRect, curY, Tr("FCP_Enlist_WorkDetails"));
            foreach (WorkOption workOption in options.workOptions)
            {
                bool active = caravanOptions.curWorkOption == workOption;
                var rowRect = new Rect(ContentIndent, curY, viewRect.width - ContentIndent, ButtonHeight);

                GUI.color = active ? TerminalColors.HighlightColor : DimPrimary;
                if (!active && Mouse.IsOver(rowRect))
                    Widgets.DrawHighlight(rowRect);
                if (active)
                    Widgets.DrawBox(rowRect);

                Text.Anchor = TextAnchor.MiddleLeft;
                GUI.color = active ? TerminalColors.HighlightColor : TerminalColors.PrimaryColor;
                Widgets.Label(rowRect.ContractedBy(4f, 0f), Tr(workOption.workLabelKey, faction.Named("FACTION")));
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = TerminalColors.PrimaryColor;

                if (Widgets.ButtonInvisible(rowRect))
                {
                    if (active)
                    {
                        caravanOptions.Reset();
                    }
                    else
                    {
                        caravanOptions.curWorkOption = workOption;
                        caravanOptions.curEnlistOptionInd = comp.OptionsDefs.IndexOf(options);
                        caravanOptions.curWorkOptionInd = options.workOptions.IndexOf(workOption);
                        if (caravan.pather.Moving)
                            caravan.pather.Paused = true;
                    }
                }

                curY += ButtonHeight + 4f;
            }
            curY += 6f;
        }

        if (options.autoFeedIsEnabled)
        {
            string autoFeedLabel = Tr(options.autoFeedLabelKey, faction.Named("FACTION")) +
                (caravanOptions.autoFeedEnabled ? " [" + "On".Translate() + "]" : " [" + "Off".Translate() + "]");
            var toggleRect = new Rect(ContentIndent, curY, ButtonWidth(autoFeedLabel), ButtonHeight);
            GUI.color = caravanOptions.autoFeedEnabled ? TerminalColors.HighlightColor : TerminalColors.PrimaryColor;
            if (Widgets.ButtonText(toggleRect, autoFeedLabel))
            {
                caravanOptions.autoFeedEnabled = !caravanOptions.autoFeedEnabled;
            }
            GUI.color = TerminalColors.PrimaryColor;
            curY += ButtonHeight + 8f;
        }

        if (options.abilityTrainingIsEnabled && !options.abilityTrainingOptions.NullOrEmpty())
        {
            curY = DrawSectionHeader(viewRect, curY, Tr("FCP_Enlist_AbilityTraining"));

            comp.activeTrainingSessions ??= new Dictionary<FactionEnlistOptionsDef, AbilityTrainingSession>();
            bool running = comp.activeTrainingSessions.TryGetValue(options, out AbilityTrainingSession session);

            if (running)
            {
                GUI.color = DimPrimary;
                Widgets.Label(new Rect(ContentIndent, curY, viewRect.width - ContentIndent, 20f), ((string)"FCP_TrainingAlreadyActive".Translate(session.trainee.Named("PAWN"))).StripTags());
                GUI.color = TerminalColors.PrimaryColor;
                curY += 24f;
            }
            else
            {
                string trainLabel = Tr(options.abilityTrainingLabelKey, faction.Named("FACTION"));
                var trainRect = new Rect(ContentIndent, curY, ButtonWidth(trainLabel), ButtonHeight);
                if (Widgets.ButtonText(trainRect, trainLabel))
                {
                    OpenAbilityTrainingMenu();
                }
                curY += ButtonHeight + 4f;
            }
        }

        Widgets.EndScrollView();
    }

    private void OpenAbilityTrainingMenu()
    {
        List<FloatMenuOption> trainingOptions = new List<FloatMenuOption>();
        for (int idx = 0; idx < options.abilityTrainingOptions.Count; idx++)
        {
            AbilityTrainingOption trainingOpt = options.abilityTrainingOptions[idx];
            int capturedIdx = idx;
            trainingOptions.Add(new FloatMenuOption(trainingOpt.labelKey.Translate(), delegate
            {
                ThingDef currency = options.currencyDef ?? ThingDefOf.Silver;
                int available = caravan.AllThings.Where(t => t.def == currency).Sum(t => t.stackCount);
                if (available < trainingOpt.cost)
                {
                    Messages.Message("FCP_TrainingNotEnoughFunds".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                    return;
                }
                List<FloatMenuOption> pawnOptions = new List<FloatMenuOption>();
                foreach (Pawn candidate in caravan.PawnsListForReading.Where(p => p.IsColonist && !p.IsPrisoner))
                {
                    Pawn localPawn = candidate;
                    pawnOptions.Add(new FloatMenuOption(localPawn.LabelShort, delegate
                    {
                        comp.ExtractMoneyFromCaravan(caravan, trainingOpt.cost, options);
                        comp.activeTrainingSessions[options] = new AbilityTrainingSession
                        {
                            trainee = localPawn,
                            startTick = Find.TickManager.TicksGame,
                            durationTicks = trainingOpt.trainingDurationDays * GenDate.TicksPerDay,
                            enlistOptionDef = options,
                            trainingOptionIndex = capturedIdx
                        };
                    }, MenuOptionPriority.Default, null, localPawn));
                }
                Find.WindowStack.Add(new FloatMenu(pawnOptions));
            }));
        }
        Find.WindowStack.Add(new FloatMenu(trainingOptions));
    }

    // ============================== Command ==============================

    private void DrawCommandTab(Rect rect)
    {
        var viewRect = new Rect(0f, 0f, rect.width - 16f, 500f);
        Widgets.BeginScrollView(rect, ref commandScrollPos, viewRect);

        float curY = 0f;

        if (options.dropPodServiceIsEnabled)
        {
            ThingDef dropPodCurrency = options.currencyDef ?? ThingDefOf.Silver;
            bool canAfford = caravan.AllThings.Where(x => x.def == dropPodCurrency).Sum(x => x.stackCount) >= options.dropPodServiceCost;
            string dropPodLabel = Tr(options.dropPodServiceLabelKey, faction.Named("FACTION"));
            GUI.color = canAfford ? TerminalColors.PrimaryColor : DimPrimary;
            var rowRect = new Rect(ContentIndent, curY, ButtonWidth(dropPodLabel), ButtonHeight);
            if (Widgets.ButtonText(rowRect, dropPodLabel) && canAfford)
            {
                Close();
                comp.StartChoosingDestination(caravan, options);
            }
            GUI.color = TerminalColors.PrimaryColor;
            curY += ButtonHeight + 8f;
        }

        if (options.shuttleServiceIsEnabled)
        {
            ThingDef shuttleCurrency = options.currencyDef ?? ThingDefOf.Silver;
            bool canAfford = caravan.AllThings.Where(x => x.def == shuttleCurrency).Sum(x => x.stackCount) >= options.shuttleServiceCost;
            string shuttleLabel = Tr(options.shuttleServiceLabelKey, faction.Named("FACTION"));
            GUI.color = canAfford ? TerminalColors.PrimaryColor : DimPrimary;
            var rowRect = new Rect(ContentIndent, curY, ButtonWidth(shuttleLabel), ButtonHeight);
            if (Widgets.ButtonText(rowRect, shuttleLabel) && canAfford)
            {
                Close();
                comp.StartChoosingShuttleDestination(caravan, options);
            }
            GUI.color = TerminalColors.PrimaryColor;
            curY += ButtonHeight + 8f;
        }

        if (!options.protocolOptions.NullOrEmpty())
        {
            string protocolLabel = Tr(options.protocolButtonLabelKey);
            var rowRect = new Rect(ContentIndent, curY, ButtonWidth(protocolLabel), ButtonHeight);
            if (Widgets.ButtonText(rowRect, protocolLabel))
            {
                var dict = options.protocolOptions.ToDictionary(x => x.protocolHashKey, x => x.action);
                Find.WindowStack.Add(new Window_Password(dict, options.protocolEnterText, options.protocolInvalidWarning));
            }
            curY += ButtonHeight + 8f;
        }

        Widgets.EndScrollView();
    }

    // ============================== Shared helpers ==============================

    private static float DrawSectionHeader(Rect viewRect, float curY, string label)
    {
        Text.Font = GameFont.Tiny;
        GUI.color = DimPrimary;
        Widgets.Label(new Rect(ContentIndent, curY, viewRect.width - ContentIndent, 18f), label);
        GUI.color = TerminalColors.PrimaryColor;
        Text.Font = GameFont.Small;
        return curY + 20f;
    }

    private static bool DrawRequirementLine(Rect viewRect, ref float curY, string label, bool met)
    {
        GUI.color = met ? new Color(0.4f, 1f, 0.4f) : new Color(1f, 0.4f, 0.4f);
        Widgets.Label(new Rect(ContentIndent + 12f, curY, viewRect.width - ContentIndent - 12f, 18f), (met ? "✓ " : "✗ ") + label);
        GUI.color = TerminalColors.PrimaryColor;
        curY += 20f;
        return met;
    }

    private static void DrawCenteredMessage(Rect rect, string label)
    {
        Text.Anchor = TextAnchor.MiddleCenter;
        GUI.color = DimPrimary;
        Widgets.Label(rect, label);
        GUI.color = TerminalColors.PrimaryColor;
        Text.Anchor = TextAnchor.UpperLeft;
    }
}
