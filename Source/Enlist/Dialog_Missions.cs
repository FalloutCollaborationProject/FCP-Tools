using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace FCP.Enlist
{
    public class Dialog_Missions : Dialog_NodeTree
    {
        public override float Margin => 0f;
        public override Vector2 InitialSize => new Vector2(620f, Mathf.Min(480, UI.screenHeight));

        private DiaOption cancelOption;
        private DiaOption xOption;
        private DiaOption startQuest;
        private Texture2D backgroundMenu;
        private FactionEnlistOptionsDef options;
        private Quest chosenQuest;
        private WorldObjectCompEnlist settlementComp;
        private Caravan caravan;
        public Dialog_Missions(DiaNode startNode, bool radioMode, Caravan caravan, WorldObjectCompEnlist settlementComp, Texture2D backgroundMenu, FactionEnlistOptionsDef options)
            : base(startNode, radioMode)
        {
            this.caravan = caravan;
            if (settlementComp.generatedQuests is null ||  settlementComp.generatedQuestsLastTick == 0 ||
                Find.TickManager.TicksGame > settlementComp.generatedQuestsLastTick + (30 * GenDate.TicksPerDay))
            {
                settlementComp.GenerateQuests();
            }
            if (settlementComp.generatedQuests.Any())
            {
                this.chosenQuest = settlementComp.generatedQuests.First();
            }
            this.settlementComp = settlementComp;
            cancelOption = new DiaOption("RH.Cancel".Translate());
            cancelOption.resolveTree = true;
            cancelOption.dialog = this;

            xOption = new DiaOption("X");
            xOption.resolveTree = true;
            xOption.dialog = this;
            startQuest = new DiaOption(options.missionsMenuStartQuestKey.Translate(options.missionsQuestCost.ToString("c0")));
            startQuest.action = delegate ()
            {
                settlementComp.AddQuest(chosenQuest, options);
                if (settlementComp.generatedQuests.Any())
                {
                    this.chosenQuest = settlementComp.generatedQuests.First();
                }
                else
                {
                    this.chosenQuest = null;
                }
                settlementComp.ExtractMoneyFromCaravan(caravan, options.missionsQuestCost);
                this.Close();
            };
            if (!CaravanHasEnoughMoney(options.missionsQuestCost))
            {
                startQuest.Disable(options.missionsMenuStartQuestNoEnoughMoneyKey.Translate());
            }
            this.backgroundMenu = backgroundMenu;
            this.options = options;
            this.absorbInputAroundWindow = true;
            if (this.options.missionsMenuAmbientSoundDef != null)
            {
                this.soundAmbient = this.options.missionsMenuAmbientSoundDef;
            }
        }

        private bool CaravanHasEnoughMoney(int fee)
        {
            return this.caravan.AllThings.Where(x => x.def == ThingDefOf.Silver).Sum((Thing t) => t.stackCount) >= fee;
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
        [TweakValue("0Enlist", 0, 500)] public static float menuDescriptionYOffset = 330f;
        [TweakValue("0Enlist", 0, 500)] public static float menuDescriptionWidth = 460f;
        [TweakValue("0Enlist", 0, 500)] public static float menuDescriptionHeight = 60f;

        [TweakValue("0Enlist", 0, 500)] public static float selectQuestXOffset = 60f;
        [TweakValue("0Enlist", 0, 500)] public static float selectQuestYOffset = 10f;
        [TweakValue("0Enlist", 0, 500)] public static float selectQuestWidth = 120f;
        [TweakValue("0Enlist", 0, 500)] public static float selectQuestHeight = 30f;

        [TweakValue("0Enlist", 0, 500)] public static float questDropDownWidth = 280f;
        [TweakValue("0Enlist", 0, 500)] public static float questDropDownHeight = 30f;

        [TweakValue("0Enlist", 0, 500)] public static float startQuestWidth = 130;
        [TweakValue("0Enlist", 0, 500)] public static float startQuestHeight = 35f;

        [TweakValue("0Enlist", 0, 600)] public static float cancelXOffset = 550f;
        [TweakValue("0Enlist", 0, 500)] public static float cancelYOffset = 60f;
        public override void DoWindowContents(Rect inRect)
        {

            Text.Font = GameFont.Small;
            Rect xRect = new Rect(inRect.xMax - 20, inRect.y + 5, 20, 20);
            GUIHelper.OptOnGUI(xOption, xRect);
            var backGroundMenuRect = new Rect(inRect.x, inRect.y, inRect.width, backgroundMenu.height).ContractedBy(10);
            GUI.DrawTexture(backGroundMenuRect, backgroundMenu, ScaleMode.ScaleToFit);

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect menuDescription = new Rect(inRect.x + menuDescriptionXOffset, inRect.y + menuDescriptionYOffset, menuDescriptionWidth, menuDescriptionHeight);
            Widgets.Label(menuDescription, options.missionsMenuDescriptionKey.Translate());

            if (this.chosenQuest != null)
            {
                Rect selectQuest = new Rect(inRect.x + selectQuestXOffset, menuDescription.yMax + selectQuestYOffset, selectQuestWidth, selectQuestHeight);
                Widgets.Label(selectQuest, options.missionsMenuSelectQuestKey.Translate());

                Rect questDropDown = new Rect(selectQuest.xMax + 10, selectQuest.y, questDropDownWidth, selectQuest.height);
                GUIHelper.Dropdown(questDropDown, this.chosenQuest, Color.white, (Quest quest) => quest, new Func<Quest, IEnumerable<Widgets.DropdownMenuElement<Quest>>>(Button_GenerateMenu), this.chosenQuest.name);
                Widgets.DrawBox(questDropDown);

                Text.Anchor = TextAnchor.MiddleLeft;
                Rect startQuestBox = new Rect(questDropDown.xMax + 10, questDropDown.y + 5, startQuestWidth, startQuestHeight);
                GUIHelper.OptOnGUI(startQuest, startQuestBox);
            }
            else if (Find.AnyPlayerHomeMap == null)
            {
                Rect playerSettlementRequired = new Rect(inRect.x + selectQuestXOffset, menuDescription.yMax + selectQuestYOffset, 300, selectQuestHeight);
                GUI.color = Color.red;
                Widgets.Label(playerSettlementRequired, "RH.PlayerSettlementRequired".Translate());
                GUI.color = Color.white;
            }

            Text.Anchor = TextAnchor.MiddleLeft;
            Rect cancelBox = new Rect(inRect.x + cancelXOffset, menuDescription.yMax + cancelYOffset, startQuestWidth, startQuestHeight);
            GUIHelper.OptOnGUI(cancelOption, cancelBox);
            Text.Anchor = TextAnchor.UpperLeft;

            if (KeyBindingDefOf.Cancel.KeyDownEvent)
            {
                this.Close();
            }
        }

        private IEnumerable<Widgets.DropdownMenuElement<Quest>> Button_GenerateMenu(Quest q)
        {
            foreach (Quest quest in this.settlementComp.generatedQuests)
            {
                yield return new Widgets.DropdownMenuElement<Quest>
                {
                    option = new FloatMenuOption(quest.name, delegate
                    {
                        chosenQuest = quest;
                    }),
                    payload = quest
                };
            }
        }
    }
}
