using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace FCP.Core.Robotics
{
    public class Dialog_RobotControl : Window
    {
        private readonly Map map;
        private Vector2 scrollPosition;

        public override Vector2 InitialSize => new Vector2(500f, 500f);

        public Dialog_RobotControl(Map map)
        {
            this.map = map;
            doCloseX = true;
            forcePause = true;
            absorbInputAroundWindow = true;
        }

        private List<Pawn> Robots => map.mapPawns.AllPawnsSpawned
            .Where(p => p.Faction == Faction.OfPlayer && RobotUtility.IsAnyRobot(p))
            .ToList();

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            var titleRect = new Rect(0f, 0f, inRect.width, 35f);
            GUI.color = TerminalColors.PrimaryColor;
            Widgets.Label(titleRect, "FCP_ControlRobots_Gizmo".Translate());
            GUI.color = Color.white;

            Text.Font = GameFont.Small;
            var listRect = new Rect(0f, 45f, inRect.width, inRect.height - 90f);
            DrawRobotList(listRect);

            var closeRect = new Rect(inRect.width - 100f, inRect.height - 40f, 90f, 35f);
            if (Widgets.ButtonText(closeRect, "Close"))
            {
                Close();
            }
        }

        private void DrawRobotList(Rect rect)
        {
            List<Pawn> robots = Robots;
            if (robots.Count == 0)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.color = TerminalColors.PrimaryColor;
                Widgets.Label(rect, "FCP_ControlRobots_None".Translate());
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
                return;
            }

            var viewRect = new Rect(0f, 0f, rect.width - 20f, robots.Count * 120f);
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);

            float y = 0f;
            foreach (Pawn robot in robots)
            {
                var rowRect = new Rect(0f, y, viewRect.width, 115f);
                DrawRobotRow(rowRect, robot);
                y += 120f;
            }

            Widgets.EndScrollView();
        }

        private void DrawRobotRow(Rect rect, Pawn robot)
        {
            if (Mouse.IsOver(rect))
            {
                Widgets.DrawLightHighlight(rect);
            }

            var iconRect = new Rect(rect.x + 5f, rect.y + 5f, 45f, 45f);
            RenderTexture portrait = PortraitsCache.Get(robot, iconRect.size, Rot4.South);
            GUI.DrawTexture(iconRect, portrait);

            var labelRect = new Rect(iconRect.xMax + 10f, rect.y + 2f, rect.width - 65f, 25f);
            GUI.color = TerminalColors.PrimaryColor;
            Text.Font = GameFont.Small;
            Widgets.Label(labelRect, robot.LabelShort);
            GUI.color = Color.white;

            var modeLabelRect = new Rect(labelRect.x, labelRect.yMax, labelRect.width, 20f);
            var buttonsRect = new Rect(rect.x, rect.y + 55f, rect.width - 10f, 28f);
            var sharedButtonsRect = new Rect(rect.x, rect.y + 87f, rect.width - 10f, 28f);

            CompEyebotMode eyebotMode = robot.GetComp<CompEyebotMode>();
            if (eyebotMode != null)
            {
                DrawModeLabel(modeLabelRect, eyebotMode.Mode.ToString());
                DrawEyebotButtons(buttonsRect, eyebotMode);
                DrawSharedButtons(sharedButtonsRect, robot);
                return;
            }

            CompSecuritronMode securitronMode = robot.GetComp<CompSecuritronMode>();
            if (securitronMode != null)
            {
                string modeLabel = securitronMode.Mode == SecuritronMode.GuardHome
                    ? "FCP_SecuritronMode_GuardHome".Translate()
                    : "FCP_SecuritronMode_GuardPawn".Translate(securitronMode.GuardedPawn?.LabelShort ?? "?");
                DrawModeLabel(modeLabelRect, modeLabel);
                DrawSecuritronButtons(buttonsRect, securitronMode);
                DrawSharedButtons(sharedButtonsRect, robot);
                return;
            }

            CompProtectronMode protectronMode = robot.GetComp<CompProtectronMode>();
            if (protectronMode != null)
            {
                DrawModeLabel(modeLabelRect, protectronMode.Mode.ToString());
                DrawProtectronButtons(buttonsRect, protectronMode);
                DrawSharedButtons(sharedButtonsRect, robot);
                return;
            }

            CompMrHandyMode mrHandyMode = robot.GetComp<CompMrHandyMode>();
            if (mrHandyMode != null)
            {
                string modeLabel = mrHandyMode.Mode == MrHandyMode.GuardPawn
                    ? "FCP_SecuritronMode_GuardPawn".Translate(mrHandyMode.GuardedPawn?.LabelShort ?? "?")
                    : mrHandyMode.Mode.ToString();
                DrawModeLabel(modeLabelRect, modeLabel);
                DrawMrHandyButtons(buttonsRect, robot, mrHandyMode);
                DrawSharedButtons(sharedButtonsRect, robot);
            }
        }

        private void DrawSharedButtons(Rect rect, Pawn robot)
        {
            CompRobotManualPower power = robot.GetComp<CompRobotManualPower>();
            if (power == null)
            {
                return;
            }

            bool canPaint = robot.GetComp<CompColorable>() != null;
            float buttonWidth = canPaint ? (rect.width - 5f) / 2f : rect.width;
            var powerRect = new Rect(rect.x, rect.y, buttonWidth, rect.height);

            string powerLabel = power.PoweredOn
                ? "FCP_RobotPower_TurnOff".Translate()
                : "FCP_RobotPower_TurnOn".Translate();
            if (Widgets.ButtonText(powerRect, powerLabel))
            {
                power.TogglePower();
            }

            if (!canPaint)
            {
                return;
            }

            var paintRect = new Rect(powerRect.xMax + 5f, rect.y, buttonWidth, rect.height);
            if (Widgets.ButtonText(paintRect, "FCP_RobotPaint_Pick".Translate()))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (RobotPaintOption paint in RobotPaintOptions.General)
                {
                    options.Add(new FloatMenuOption(paint.label.Translate(), () => robot.SetColor(paint.color)));
                }
                foreach (RobotPaintOption paint in RobotPaintOptions.Factional)
                {
                    options.Add(new FloatMenuOption(paint.label.Translate(), () => robot.SetColor(paint.color)));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }
        }

        private static void DrawModeLabel(Rect rect, string mode)
        {
            GUI.color = TerminalColors.PrimaryColor * 0.7f;
            Text.Font = GameFont.Tiny;
            Widgets.Label(rect, "FCP_EyebotMode_Inspect".Translate(mode));
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
        }

        private static void DrawEyebotButtons(Rect rect, CompEyebotMode modeComp)
        {
            float buttonWidth = rect.width / 3f;
            var defendRect = new Rect(rect.x, rect.y, buttonWidth, rect.height);
            var musicRect = new Rect(defendRect.xMax + 5f, rect.y, buttonWidth, rect.height);
            var exploreRect = new Rect(musicRect.xMax + 5f, rect.y, buttonWidth, rect.height);

            if (Widgets.ButtonText(defendRect, "FCP_EyebotMode_Defend".Translate()))
            {
                modeComp.SetMode(EyebotMode.Defend);
            }
            if (Widgets.ButtonText(musicRect, "FCP_EyebotMode_Music".Translate()))
            {
                modeComp.SetMode(EyebotMode.Music);
            }
            if (Widgets.ButtonText(exploreRect, "FCP_EyebotMode_Explore".Translate()))
            {
                modeComp.SetMode(EyebotMode.Explore);
            }
        }

        private void DrawSecuritronButtons(Rect rect, CompSecuritronMode modeComp)
        {
            Pawn robot = (Pawn)modeComp.parent;
            CompSecuritronFace faceComp = robot.GetComp<CompSecuritronFace>();
            int columns = faceComp != null ? 4 : 3;
            float buttonWidth = (rect.width - (columns - 1) * 5f) / columns;
            var guardHomeRect = new Rect(rect.x, rect.y, buttonWidth, rect.height);
            var guardPawnRect = new Rect(guardHomeRect.xMax + 5f, rect.y, buttonWidth, rect.height);
            var guardPointRect = new Rect(guardPawnRect.xMax + 5f, rect.y, buttonWidth, rect.height);

            if (Widgets.ButtonText(guardHomeRect, "FCP_SecuritronMode_GuardHome".Translate()))
            {
                modeComp.SetGuardHome();
            }
            if (Widgets.ButtonText(guardPawnRect, "FCP_SecuritronMode_GuardPawn_Pick".Translate()))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (Pawn colonist in map.mapPawns.FreeColonists)
                {
                    options.Add(new FloatMenuOption(colonist.LabelShort, () => modeComp.SetGuardPawn(colonist)));
                }
                if (options.Count == 0)
                {
                    options.Add(new FloatMenuOption("FCP_ControlRobots_None".Translate(), null));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }
            if (Widgets.ButtonText(guardPointRect, "FCP_SecuritronMode_GuardPoint_Pick".Translate()))
            {
                BeginGuardPointTargeting(modeComp);
            }

            if (faceComp == null)
            {
                return;
            }

            var faceRect = new Rect(guardPointRect.xMax + 5f, rect.y, buttonWidth, rect.height);
            if (Widgets.ButtonText(faceRect, "FCP_SecuritronFace_Pick".Translate()))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (HediffDef faceDef in CompSecuritronFace.PlayerSelectableFaces)
                {
                    options.Add(new FloatMenuOption(faceDef.LabelCap, () => faceComp.SetFace(faceDef)));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }
        }

        private void BeginGuardPointTargeting(CompSecuritronMode modeComp)
        {
            Close();
            TargetingParameters targetParams = new TargetingParameters
            {
                canTargetLocations = true,
                canTargetPawns = false,
                canTargetBuildings = false,
                canTargetItems = false,
                mustBeSelectable = false,
            };
            Find.Targeter.BeginTargeting(targetParams, target => modeComp.SetGuardPoint(target.Cell), requiresCastedSelected: false);
        }

        private static void DrawProtectronButtons(Rect rect, CompProtectronMode modeComp)
        {
            float buttonWidth = (rect.width - 10f) / 3f;
            var guardRect = new Rect(rect.x, rect.y, buttonWidth, rect.height);
            var constructRect = new Rect(guardRect.xMax + 5f, rect.y, buttonWidth, rect.height);
            var haulRect = new Rect(constructRect.xMax + 5f, rect.y, buttonWidth, rect.height);

            if (Widgets.ButtonText(guardRect, "FCP_ProtectronMode_Guard".Translate()))
            {
                modeComp.SetMode(ProtectronMode.Guard);
            }
            if (Widgets.ButtonText(constructRect, "FCP_ProtectronMode_Construct".Translate()))
            {
                modeComp.SetMode(ProtectronMode.Construct);
            }
            if (Widgets.ButtonText(haulRect, "FCP_ProtectronMode_Haul".Translate()))
            {
                modeComp.SetMode(ProtectronMode.Haul);
            }
        }

        private void DrawMrHandyButtons(Rect rect, Pawn robot, CompMrHandyMode modeComp)
        {
            CompMrHandyLoadout loadout = robot.GetComp<CompMrHandyLoadout>();
            if (loadout == null)
            {
                return;
            }

            var candidates = new List<(MrHandyMode mode, string labelKey)>
            {
                (MrHandyMode.Guard, "FCP_MrHandyMode_Guard"),
                (MrHandyMode.GuardPawn, "FCP_MrHandyMode_GuardPawn_Pick"),
                (MrHandyMode.Cook, "FCP_MrHandyMode_Cook"),
                (MrHandyMode.Clean, "FCP_MrHandyMode_Clean"),
                (MrHandyMode.Garden, "FCP_MrHandyMode_Garden"),
                (MrHandyMode.BasicCare, "FCP_MrHandyMode_BasicCare"),
                (MrHandyMode.Tend, "FCP_MrHandyMode_Tend"),
                (MrHandyMode.Childcare, "FCP_MrHandyMode_Childcare"),
            };

            List<(MrHandyMode mode, string labelKey)> visible = candidates.Where(c => loadout.SupportsMode(c.mode)).ToList();
            if (visible.Count == 0)
            {
                return;
            }

            float buttonWidth = (rect.width - (visible.Count - 1) * 5f) / visible.Count;
            float x = rect.x;
            foreach ((MrHandyMode mode, string labelKey) in visible)
            {
                var buttonRect = new Rect(x, rect.y, buttonWidth, rect.height);
                if (Widgets.ButtonText(buttonRect, labelKey.Translate()))
                {
                    if (mode == MrHandyMode.GuardPawn)
                    {
                        List<FloatMenuOption> options = new List<FloatMenuOption>();
                        foreach (Pawn colonist in map.mapPawns.FreeColonists)
                        {
                            options.Add(new FloatMenuOption(colonist.LabelShort, () => modeComp.SetGuardPawn(colonist)));
                        }
                        if (options.Count == 0)
                        {
                            options.Add(new FloatMenuOption("FCP_ControlRobots_None".Translate(), null));
                        }
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                    else
                    {
                        modeComp.SetMode(mode);
                    }
                }
                x = buttonRect.xMax + 5f;
            }
        }
    }
}
