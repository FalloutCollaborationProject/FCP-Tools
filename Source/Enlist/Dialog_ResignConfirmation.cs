using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Grammar;
using Verse.Sound;

namespace  FCP.Enlist
{
    public class Dialog_ResignConfirmation : Dialog_NodeTree
    {
        public override float Margin => 0f;
        public override Vector2 InitialSize => new Vector2(620f, Mathf.Min(480, UI.screenHeight));

        private DiaOption xOption;
        private DiaOption yesOption;
        private DiaOption noOption;
        private FactionEnlistOptionsDef options;
        private WorldObjectCompEnlist settlementComp;
        public Dialog_ResignConfirmation(DiaNode startNode, bool radioMode, WorldObjectCompEnlist settlementComp, FactionEnlistOptionsDef options)
            : base(startNode, radioMode)
        {
            this.settlementComp = settlementComp;

            xOption = new DiaOption("X");
            xOption.resolveTree = true;
            xOption.dialog = this;

            yesOption = new DiaOption("Yes".Translate());
            yesOption.resolveTree = true;
            yesOption.dialog = this;
            yesOption.action = delegate ()
            {
                WorldEnlistTracker.Instance.Delist(this.settlementComp.parent.Faction, options);
            };
            noOption = new DiaOption("No".Translate());
            noOption.resolveTree = true;
            noOption.dialog = this;

            this.options = options;
            this.absorbInputAroundWindow = false;
        }

        public override void PreClose()
        {
            base.PreClose();
            curNode.PreClose();
        }

        public override void PostClose()
        {
            base.PostClose();
            if (closeAction != null)
            {
                closeAction();
            }
        }

        public override void WindowOnGUI()
        {
            if (screenFillColor != Color.clear)
            {
                GUI.color = screenFillColor;
                GUI.DrawTexture(new Rect(0f, 0f, UI.screenWidth, UI.screenHeight), BaseContent.WhiteTex);
                GUI.color = Color.white;
            }
            base.WindowOnGUI();
        }

        [TweakValue("0Enlist", 0, 500)] public static float menuDescriptionXOffset = 70f;
        [TweakValue("0Enlist", 0, 500)] public static float menuDescriptionYOffset = 200f;
        [TweakValue("0Enlist", 0, 500)] public static float menuDescriptionWidth = 460f;
        [TweakValue("0Enlist", 0, 500)] public static float menuDescriptionHeight = 60f;

        [TweakValue("0Enlist", 0, 500)] public static float noOptionXOffset = 20f;
        [TweakValue("0Enlist", 0, 500)] public static float noOptionYOffset = 440f;

        [TweakValue("0Enlist", 0, 500)] public static float yesOptionXOffset = 510f;
        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;
            Rect xRect = new Rect(inRect.xMax - 20, inRect.y + 5, 20, 20);
            GUIHelper.OptOnGUI(xOption, xRect);

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect menuDescription = new Rect(inRect.x + menuDescriptionXOffset, inRect.y + menuDescriptionYOffset, menuDescriptionWidth, menuDescriptionHeight);
            Widgets.Label(menuDescription, options.resignMenuTextKey.Translate());

            Rect noRect = new Rect(inRect.x + noOptionXOffset, inRect.y + noOptionYOffset, 30, 30);
            GUIHelper.OptOnGUI(noOption, noRect);

            Rect yesRect = new Rect(noRect.xMax + yesOptionXOffset, noRect.y, 30, 30);
            GUIHelper.OptOnGUI(yesOption, yesRect);

            Text.Anchor = TextAnchor.UpperLeft;

        }
    }
}
