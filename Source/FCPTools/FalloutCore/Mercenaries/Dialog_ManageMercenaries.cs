using Verse;
using RimWorld;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace FCP.Core
{
    public class Dialog_ManageMercenaries : Window
    {
        private Vector2 scrollPositionFaction = Vector2.zero;
        private Vector2 scrollPositionGroup = Vector2.zero;
        private Vector2 scrollPositionMember = Vector2.zero;
        private Vector2 scrollPositionAvailable = Vector2.zero;

        private Faction selectedFaction = null;
        private MercenaryGroup selectedGroup = null;
        private CaravanObjectiveDef selectedObjective = null;
        private string newGroupName = "";
        private int caravanDurationDays = 7;
        private string caravanDurationDaysBuffer = "7";

        private const float RowHeight = 30f;
        private const float ButtonWidth = 120f;
        private new const float Margin = 10f;
        private const float ColumnWidthRatio = 0.3f;

        public override Vector2 InitialSize => new Vector2(900f, 700f);

        public Dialog_ManageMercenaries()
        {
            forcePause = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
            closeOnAccept = false;
            doCloseButton = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            inRect.height -= CloseButSize.y;

            float columnWidth = inRect.width * ColumnWidthRatio - Margin * 2;
            float remainingWidth = inRect.width - columnWidth - Margin * 2;

            Rect factionRect = new Rect(inRect.x, inRect.y, columnWidth, inRect.height);
            Rect groupRect = new Rect(factionRect.xMax + Margin, inRect.y, columnWidth, inRect.height);
            Rect memberRect = new Rect(groupRect.xMax + Margin, inRect.y, remainingWidth - columnWidth - Margin, inRect.height);
            Rect availableRect = new Rect(memberRect.xMax + Margin, inRect.y, columnWidth, inRect.height);
            DrawFactionColumn(factionRect);
            if (selectedFaction != null)
            {
                DrawGroupColumn(groupRect);
            }
            else
            {
                Widgets.Label(groupRect.ContractedBy(Margin), "SelectFactionFirst".Translate());
            }
            if (selectedGroup != null)
            {
                DrawMemberColumn(memberRect);
            }
            else
            {
                Widgets.Label(memberRect.ContractedBy(Margin), "SelectGroupFirst".Translate());
            }
            if (selectedFaction != null && selectedGroup != null)
            {
                DrawAvailablePawnsColumn(availableRect);
            }
            else
            {
                Widgets.Label(availableRect.ContractedBy(Margin), "SelectFactionAndGroupFirst".Translate());
            }
        }

        private void DrawFactionColumn(Rect rect)
        {
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 30f), "MercenaryFactions".Translate());
            Rect scrollViewRect = new Rect(rect.x, rect.y + 40f, rect.width, rect.height - 40f);
            var mercenaryFactions = Find.FactionManager.AllFactionsVisible
                .Where(f => f.def.HasModExtension<MercenaryExtension>() && !f.IsPlayer)
                .ToList();

            if (!mercenaryFactions.Any())
            {
                Widgets.Label(scrollViewRect.ContractedBy(Margin), "NoMercenaryFactions".Translate());
                return;
            }

            float viewHeightFaction = mercenaryFactions.Count * (RowHeight + Margin);
            Rect scrollContentRect = new Rect(0f, 0f, scrollViewRect.width - 16f, viewHeightFaction);

            Widgets.BeginScrollView(scrollViewRect, ref scrollPositionFaction, scrollContentRect);
            float listY = 0f;
            foreach (var faction in mercenaryFactions)
            {
                Rect rowRect = new Rect(0f, listY, scrollContentRect.width, RowHeight);
                if (selectedFaction == faction) Widgets.DrawHighlightSelected(rowRect);
                Widgets.DrawHighlightIfMouseover(rowRect);

                if (Widgets.ButtonInvisible(rowRect))
                {
                    selectedFaction = faction;
                    selectedGroup = null;
                }
                Widgets.Label(rowRect.ContractedBy(Margin / 2), faction.Name);
                listY += RowHeight + Margin;
            }
            Widgets.EndScrollView();
        }

        private void DrawGroupColumn(Rect rect)
        {
            var factionData = selectedFaction.GetMercenaryData();
            if (factionData == null) return;

            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 30f), "Groups".Translate());
            float curY = rect.y + 40f;
            Rect newGroupRect = new Rect(rect.x, curY, rect.width, 30f);
            newGroupName = Widgets.TextField(new Rect(newGroupRect.x, newGroupRect.y, newGroupRect.width - ButtonWidth - Margin, 30f), newGroupName);
            if (Widgets.ButtonText(new Rect(newGroupRect.xMax - ButtonWidth, newGroupRect.y, ButtonWidth, 30f), "CreateGroup".Translate()))
            {
                if (!string.IsNullOrWhiteSpace(newGroupName) && !factionData.groups.Any(g => g.name == newGroupName))
                {
                    factionData.groups.Add(new MercenaryGroup { name = newGroupName });
                    newGroupName = "";
                }
            }
            curY += 40f;
            Rect scrollViewRect = new Rect(rect.x, curY, rect.width, rect.height - (curY - rect.y));
            float viewHeightGroup = factionData.groups.Count * (RowHeight + Margin);
            Rect scrollContentRect = new Rect(0f, 0f, scrollViewRect.width - 16f, viewHeightGroup);

            Widgets.BeginScrollView(scrollViewRect, ref scrollPositionGroup, scrollContentRect);
            float listY = 0f;
            foreach (var group in factionData.groups.ToList())
            {
                Rect rowRect = new Rect(0f, listY, scrollContentRect.width, RowHeight);
                if (selectedGroup == group) Widgets.DrawHighlightSelected(rowRect);
                Widgets.DrawHighlightIfMouseover(rowRect);
                if (Widgets.ButtonInvisible(new Rect(rowRect.x, rowRect.y, rowRect.width - ButtonWidth - Margin, RowHeight)))
                {
                    selectedGroup = group;
                }

                Widgets.Label(new Rect(rowRect.x + Margin / 2, rowRect.y, rowRect.width - ButtonWidth - Margin * 2, RowHeight),
                    $"{group.name} ({group.members.Count})");
                if (Widgets.ButtonText(new Rect(rowRect.xMax - ButtonWidth, rowRect.y, ButtonWidth, RowHeight), "Delete".Translate()))
                {
                    factionData.groups.Remove(group);
                    if (selectedGroup == group) selectedGroup = null;
                }
                listY += RowHeight + Margin;
            }
            Widgets.EndScrollView();
        }

        private void DrawMemberColumn(Rect rect)
        {
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 30f), "GroupMembers".Translate(selectedGroup.name));
            Rect scrollViewRect = new Rect(rect.x, rect.y + 40f, rect.width, rect.height - 40f);
            selectedGroup.members.RemoveAll(p => p == null || p.Destroyed);

            float viewHeightMember = selectedGroup.members.Count * (RowHeight + Margin);
            Rect scrollContentRect = new Rect(0f, 0f, scrollViewRect.width - 16f, viewHeightMember);

            Widgets.BeginScrollView(scrollViewRect, ref scrollPositionMember, scrollContentRect);
            float listY = 0f;
            foreach (var pawn in selectedGroup.members.ToList())
            {
                Rect rowRect = new Rect(0f, listY, scrollContentRect.width, RowHeight);
                Widgets.DrawHighlightIfMouseover(rowRect);
                Widgets.Label(new Rect(rowRect.x + Margin / 2, rowRect.y, rowRect.width - ButtonWidth - Margin * 2, RowHeight), pawn.LabelCap);
                if (Widgets.ButtonText(new Rect(rowRect.xMax - ButtonWidth, rowRect.y, ButtonWidth, RowHeight), "Remove".Translate()))
                {
                    selectedGroup.members.Remove(pawn);
                }
                listY += RowHeight + Margin;
            }
            Widgets.EndScrollView();
            float caravanSectionY = viewHeightMember + 50f;
            float curY = caravanSectionY;

            Widgets.Label(new Rect(rect.x, curY, rect.width, 30f), "CaravanObjective".Translate());
            Rect objectiveButtonRect = new Rect(rect.x, curY + 30f, rect.width, 30f);
            string objectiveLabel = selectedObjective?.label ?? "SelectObjective".Translate();
            if (Widgets.ButtonText(objectiveButtonRect, objectiveLabel))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                var objectives = selectedFaction.def.GetModExtension<MercenaryExtension>()?.caravanObjectives;
                if (objectives != null)
                {
                    foreach (var objective in objectives)
                    {
                        CaravanObjectiveDef localDef = objective;
                        options.Add(new FloatMenuOption(localDef.label, () =>
                        {
                            selectedObjective = localDef;
                        }));
                    }
                }
                if (!options.Any())
                {
                    options.Add(new FloatMenuOption("NoObjectivesAvailable".Translate(), null));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }
            curY = objectiveButtonRect.yMax + Margin;
            Widgets.Label(new Rect(rect.x, curY, rect.width, 30f), "CaravanDurationDays".Translate());
            Rect durationRect = new Rect(rect.x, curY + 30f, rect.width / 2f - Margin, 30f);
            Widgets.TextFieldNumeric<int>(durationRect, ref caravanDurationDays, ref caravanDurationDaysBuffer);
            curY = durationRect.yMax + Margin;
            Rect sendButtonRect = new Rect(rect.x, curY, ButtonWidth, 35f);
            if (Widgets.ButtonText(sendButtonRect, "SendCaravan".Translate()))
            {
                TrySendCaravan();
            }
        }

        private void TrySendCaravan()
        {
            if (selectedFaction == null || selectedGroup == null || selectedObjective == null) return;
            List<Pawn> caravanPawns = selectedGroup.members.ToList();
            if (!caravanPawns.Any())
            {
                Messages.Message("MessageNoMercenariesSelectedForCaravan".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }

            if (MercenaryUtility.TrySendMercenaryCaravan(selectedFaction, caravanPawns, selectedObjective, caravanDurationDays))
            {
                Messages.Message("MessageMercenaryCaravanSent".Translate(selectedFaction.Name, selectedObjective.label, caravanDurationDays), MessageTypeDefOf.PositiveEvent);
                Close();
            }
            else
            {
            }
        }

        private void DrawAvailablePawnsColumn(Rect rect)
        {
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 30f), "AvailableToAssign".Translate());
            Rect scrollViewRect = new Rect(rect.x, rect.y + 40f, rect.width, rect.height - 40f);

            var data = selectedFaction.GetMercenaryData();
            if (data == null) return;
            var allHired = data.AllHiredPawns;
            var availablePawns = allHired.Except(selectedGroup.members).ToList();
            availablePawns.RemoveAll(p => p == null || p.Destroyed);

            float viewHeightAvailable = availablePawns.Count * (RowHeight + Margin);
            Rect scrollContentRect = new Rect(0f, 0f, scrollViewRect.width - 16f, viewHeightAvailable);

            Widgets.BeginScrollView(scrollViewRect, ref scrollPositionAvailable, scrollContentRect);
            float listY = 0f;
            foreach (var pawn in availablePawns)
            {
                Rect rowRect = new Rect(0f, listY, scrollContentRect.width, RowHeight);
                Widgets.DrawHighlightIfMouseover(rowRect);
                Widgets.Label(new Rect(rowRect.x + Margin / 2, rowRect.y, rowRect.width - ButtonWidth - Margin * 2, RowHeight), pawn.LabelCap);
                if (Widgets.ButtonText(new Rect(rowRect.xMax - ButtonWidth, rowRect.y, ButtonWidth, RowHeight), "Add".Translate()))
                {
                    foreach (var otherGroup in data.groups.Where(g => g != selectedGroup))
                    {
                        otherGroup.members.Remove(pawn);
                    }
                    selectedGroup.members.Add(pawn);
                }
                listY += RowHeight + Margin;
            }
            Widgets.EndScrollView();
        }

    }
}
