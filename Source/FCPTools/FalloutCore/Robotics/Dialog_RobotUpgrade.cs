using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace FCP.Core.Robotics
{
    public class Dialog_RobotUpgrade : Window
    {
        private readonly Pawn robot;
        private readonly Func<List<RobotUpgradeOption>> optionsProvider;
        private Vector2 scrollPosition;

        public override Vector2 InitialSize => new Vector2(460f, 500f);

        public Dialog_RobotUpgrade(Pawn robot, Func<List<RobotUpgradeOption>> optionsProvider)
        {
            this.robot = robot;
            this.optionsProvider = optionsProvider;
            doCloseX = true;
            forcePause = true;
            absorbInputAroundWindow = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            var titleRect = new Rect(0f, 0f, inRect.width, 35f);
            GUI.color = TerminalColors.PrimaryColor;
            Widgets.Label(titleRect, "FCP_RobotUpgradeDialog_Title".Translate(robot.LabelShort));
            GUI.color = Color.white;

            Text.Font = GameFont.Small;
            var listRect = new Rect(0f, 45f, inRect.width, inRect.height - 90f);
            DrawOptionList(listRect);

            var closeRect = new Rect(inRect.width - 100f, inRect.height - 40f, 90f, 35f);
            if (Widgets.ButtonText(closeRect, "Close"))
            {
                Close();
            }
        }

        private void DrawOptionList(Rect rect)
        {
            List<RobotUpgradeOption> options = optionsProvider();
            if (options.Count == 0)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.color = TerminalColors.PrimaryColor;
                Widgets.Label(rect, "FCP_RobotUpgradeDialog_None".Translate());
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
                return;
            }

            const float rowHeight = 60f;
            const float headerHeight = 26f;

            float totalHeight = 0f;
            string lastCategory = null;
            foreach (RobotUpgradeOption option in options)
            {
                if (ShouldDrawHeader(option.category, lastCategory))
                {
                    totalHeight += headerHeight;
                    lastCategory = option.category;
                }
                totalHeight += rowHeight + 6f;
            }

            var viewRect = new Rect(0f, 0f, rect.width - 20f, totalHeight);
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);

            float y = 0f;
            lastCategory = null;
            foreach (RobotUpgradeOption option in options)
            {
                if (ShouldDrawHeader(option.category, lastCategory))
                {
                    DrawCategoryHeader(new Rect(0f, y, viewRect.width, headerHeight), option.category);
                    y += headerHeight;
                    lastCategory = option.category;
                }
                DrawRow(new Rect(0f, y, viewRect.width, rowHeight), option);
                y += rowHeight + 6f;
            }

            Widgets.EndScrollView();
        }

        private static bool ShouldDrawHeader(string category, string lastCategory)
        {
            return !category.NullOrEmpty() && category != lastCategory;
        }

        private void DrawCategoryHeader(Rect rect, string category)
        {
            Text.Font = GameFont.Tiny;
            GUI.color = TerminalColors.PrimaryColor;
            Widgets.Label(rect, category.ToUpperInvariant());
            Widgets.DrawLineHorizontal(rect.x, rect.yMax - 3f, rect.width);
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
        }

        private void DrawRow(Rect rect, RobotUpgradeOption option)
        {
            Widgets.DrawLightHighlight(rect);

            var labelRect = new Rect(rect.x + 8f, rect.y + 4f, rect.width - 130f, 24f);
            GUI.color = option.Enabled ? TerminalColors.PrimaryColor : Color.gray;
            Widgets.Label(labelRect, option.label);
            GUI.color = Color.white;

            var costRect = new Rect(rect.x + 8f, labelRect.yMax, rect.width - 130f, 28f);
            DrawCost(costRect, option.cost);

            var buttonRect = new Rect(rect.width - 110f, rect.y + rect.height / 2f - 14f, 100f, 28f);
            if (!option.Enabled)
            {
                GUI.color = Color.gray;
                Widgets.Label(buttonRect, option.disabledReason);
                GUI.color = Color.white;
                return;
            }

            if (Widgets.ButtonText(buttonRect, "FCP_RobotUpgradeDialog_Install".Translate()))
            {
                option.install();
            }
        }

        private void DrawCost(Rect rect, List<ThingDefCountClass> cost)
        {
            float x = rect.x;
            foreach (ThingDefCountClass entry in cost)
            {
                int have = robot.Map?.resourceCounter.GetCount(entry.thingDef) ?? 0;
                bool afford = have >= entry.count;

                var iconRect = new Rect(x, rect.y, 20f, 20f);
                Widgets.ThingIcon(iconRect, entry.thingDef);

                var countRect = new Rect(iconRect.xMax + 2f, rect.y + 2f, 70f, 20f);
                GUI.color = afford ? Color.white : Color.red;
                Widgets.Label(countRect, $"{have}/{entry.count}");
                GUI.color = Color.white;

                x = countRect.xMax + 12f;
            }
        }
    }
}
