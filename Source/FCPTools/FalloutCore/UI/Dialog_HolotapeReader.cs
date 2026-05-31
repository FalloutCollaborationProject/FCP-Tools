using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace FCP.Core.Holotapes
{
    public class Dialog_HolotapeReader : Window
    {
        private readonly CompHolotape holotapeComp;
        private readonly Buildings.CompHolotapeStorage terminalStorage;
        private readonly CompPipboyHolotapeStorage pipboyStorage;
        private readonly Pawn reader;
        private int currentEntryIndex;
        private Vector2 scrollPosition;

        public override Vector2 InitialSize => new Vector2(800f, 700f);

        public Dialog_HolotapeReader(CompHolotape holotapeComp, Buildings.CompHolotapeStorage storage, Pawn reader = null)
        {
            this.holotapeComp = holotapeComp;
            terminalStorage = storage;
            this.reader = reader;
            doCloseX = true;
            forcePause = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = false;
            MarkReadAndGiveJoy();
        }

        public Dialog_HolotapeReader(CompHolotape holotapeComp, CompPipboyHolotapeStorage storage, Pawn reader)
        {
            this.holotapeComp = holotapeComp;
            pipboyStorage = storage;
            this.reader = reader;
            doCloseX = true;
            forcePause = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = false;
            MarkReadAndGiveJoy();
        }

        private void MarkReadAndGiveJoy()
        {
            if (!holotapeComp.HasBeenRead)
            {
                holotapeComp.MarkAsRead();
                if (reader != null)
                    GiveRecreationJoy();
            }
        }

        private void GiveRecreationJoy()
        {
            Need_Joy joy = reader.needs?.joy;
            if (joy != null)
                joy.GainJoy(0.08f, JoyKindDefOf.Reading);

            ThoughtDef thoughtDef = DefDatabase<ThoughtDef>.GetNamedSilentFail("FCP_ReadHolotape");
            if (thoughtDef != null)
                reader.needs.mood?.thoughts.memories.TryGainMemory(thoughtDef);
        }

        public override void DoWindowContents(Rect inRect)
        {
            HolotapeDef def = holotapeComp.ContentDef;
            if (def == null)
            {
                Close();
                return;
            }

            float y = 0f;
            Color terminalColor = TerminalColors.PrimaryColor;

            Text.Font = GameFont.Medium;
            Rect titleRect = new Rect(0f, y, inRect.width, 35f);
            GUI.color = terminalColor;
            Widgets.Label(titleRect, def.LabelCap);
            y += 40f;

            Text.Font = GameFont.Small;
            GUI.color = terminalColor * 0.7f;
            Rect infoRect = new Rect(0f, y, inRect.width, 20f);
            TaggedString info = def.category.ToString();
            if (!def.author.NullOrEmpty())
                info += " • " + def.author;
            Widgets.Label(infoRect, info);
            GUI.color = Color.white;
            y += 25f;

            if (def.entries.Count > 1)
            {
                Rect tabRect = new Rect(0f, y, inRect.width, 30f);
                DrawEntryTabs(tabRect, def);
                y += 35f;
            }

            Rect contentRect = new Rect(0f, y, inRect.width, inRect.height - y - 50f);
            DrawEntryContent(contentRect, def);

            Rect buttonRect = new Rect(inRect.width - 200f, inRect.height - 40f, 90f, 35f);
            if (Widgets.ButtonText(buttonRect, "Close"))
                Close();

            buttonRect.x -= 100f;
            if (Widgets.ButtonText(buttonRect, "Back"))
            {
                if (terminalStorage != null)
                    Find.WindowStack.Add(new Dialog_HolotapeBrowser(terminalStorage, reader));
                else if (pipboyStorage != null)
                    Find.WindowStack.Add(new Dialog_HolotapeBrowser(pipboyStorage, reader));
                Close();
            }

            if (def.skillToTeach != null && !holotapeComp.SkillAlreadyGranted && reader != null)
            {
                buttonRect.x -= 180f;
                buttonRect.width = 170f;
                if (Widgets.ButtonText(buttonRect, "Learn " + def.skillToTeach.LabelCap + " (+" + def.xpAmount.ToString("F0") + " XP)"))
                {
                    reader.skills.Learn(def.skillToTeach, def.xpAmount);
                    holotapeComp.GrantSkill();
                    Messages.Message(reader.LabelShort + " read about " + def.skillToTeach.LabelCap, reader, MessageTypeDefOf.PositiveEvent);
                }
            }
        }

        private void DrawEntryTabs(Rect rect, HolotapeDef def)
        {
            List<HolotapeEntry> entries = def.entries;
            float tabWidth = Mathf.Min(rect.width / entries.Count, 150f);

            for (int i = 0; i < entries.Count; i++)
            {
                Rect tabRect = new Rect(rect.x + i * tabWidth, rect.y, tabWidth, rect.height);
                
                if (currentEntryIndex == i)
                {
                    Widgets.DrawMenuSection(tabRect);
                }
                else
                {
                    if (Widgets.ButtonInvisible(tabRect))
                    {
                        currentEntryIndex = i;
                        scrollPosition = Vector2.zero;
                    }
                    if (Mouse.IsOver(tabRect))
                        Widgets.DrawHighlight(tabRect);
                }

                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = GameFont.Tiny;
                GUI.color = TerminalColors.PrimaryColor;
                Widgets.Label(tabRect, entries[i].title.NullOrEmpty() ? "Entry " + (i + 1) : entries[i].title);
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Small;
            }
        }

        private void DrawEntryContent(Rect rect, HolotapeDef def)
        {
            List<HolotapeEntry> entries = def.entries;
            if (currentEntryIndex >= entries.Count)
                currentEntryIndex = 0;

            HolotapeEntry entry = entries[currentEntryIndex];
            
            Widgets.DrawMenuSection(rect);
            Rect innerRect = rect.ContractedBy(10f);
            
            Text.Font = GameFont.Small;
            float textHeight = Text.CalcHeight(entry.content, innerRect.width);
            Rect viewRect = new Rect(0f, 0f, innerRect.width - 20f, textHeight + 10f);

            Widgets.BeginScrollView(innerRect, ref scrollPosition, viewRect);
            
            GUI.color = TerminalColors.PrimaryColor;
            Widgets.Label(new Rect(0f, 0f, viewRect.width, textHeight), entry.content);
            GUI.color = Color.white;
            
            Widgets.EndScrollView();
        }

        public override void PostClose()
        {
            base.PostClose();
            
            if (reader != null && Find.Selector.SelectedObjects.Contains(reader))
            {
                Find.Selector.Deselect(reader);
                Find.Selector.Select(reader);
            }
        }
    }
}
