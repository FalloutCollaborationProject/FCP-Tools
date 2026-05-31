using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace FCP.Core.Holotapes
{
    public class Dialog_HolotapeBrowser : Window
    {
        private readonly Buildings.CompHolotapeStorage terminalStorage;
        private readonly CompPipboyHolotapeStorage pipboyStorage;
        private readonly Pawn pawn;
        private Vector2 scrollPosition;
        private Thing selectedHolotape;

        public override Vector2 InitialSize => new Vector2(700f, 600f);

        public Dialog_HolotapeBrowser(Buildings.CompHolotapeStorage storage, Pawn pawn = null)
        {
            terminalStorage = storage;
            this.pawn = pawn;
            doCloseX = true;
            forcePause = true;
            absorbInputAroundWindow = true;
        }

        public Dialog_HolotapeBrowser(CompPipboyHolotapeStorage storage, Pawn pawn)
        {
            pipboyStorage = storage;
            this.pawn = pawn;
            doCloseX = true;
            forcePause = true;
            absorbInputAroundWindow = true;
        }

        private List<Thing> StoredHolotapes => terminalStorage != null ? terminalStorage.StoredHolotapes : pipboyStorage.StoredHolotapes;
        private int Count => terminalStorage != null ? terminalStorage.Count : pipboyStorage.Count;
        private string Title => pipboyStorage != null ? "Pip-Boy Holotape Archive" : "Terminal Holotape Archive";

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            var titleRect = new Rect(0f, 0f, inRect.width, 35f);
            GUI.color = TerminalColors.PrimaryColor;
            Widgets.Label(titleRect, Title);
            GUI.color = Color.white;
            
            Text.Font = GameFont.Small;
            var listRect = new Rect(0f, 45f, inRect.width, inRect.height - 100f);
            DrawHolotapeList(listRect);

            var buttonRect = new Rect(inRect.width - 200f, inRect.height - 40f, 90f, 35f);
            if (Widgets.ButtonText(buttonRect, "Close"))
            {
                Close();
            }

            if (selectedHolotape != null)
            {
                buttonRect.x -= 100f;
                if (Widgets.ButtonText(buttonRect, "Read"))
                {
                    var comp = selectedHolotape.TryGetComp<CompHolotape>();
                    if (comp != null && comp.ContentDef != null)
                    {
                        if (terminalStorage != null)
                            Find.WindowStack.Add(new Dialog_HolotapeReader(comp, terminalStorage, pawn));
                        else if (pipboyStorage != null)
                            Find.WindowStack.Add(new Dialog_HolotapeReader(comp, pipboyStorage, pawn));
                        Close();
                    }
                }
            }
        }

        private void DrawHolotapeList(Rect rect)
        {
            var holotapes = StoredHolotapes;
            if (holotapes.NullOrEmpty())
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.color = TerminalColors.PrimaryColor;
                Widgets.Label(rect, "No holotapes stored.");
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
                return;
            }

            var viewRect = new Rect(0f, 0f, rect.width - 20f, Count * 60f);
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);

            float y = 0f;
            foreach (var holotape in holotapes)
            {
                if (holotape == null) 
                    continue;

                var comp = holotape.TryGetComp<CompHolotape>();
                if (comp?.ContentDef == null) 
                    continue;

                var rowRect = new Rect(0f, y, viewRect.width, 55f);
                DrawHolotapeRow(rowRect, holotape, comp);
                y += 60f;
            }

            Widgets.EndScrollView();
        }

        private void DrawHolotapeRow(Rect rect, Thing holotape, CompHolotape comp)
        {
            bool isSelected = selectedHolotape == holotape;
            
            if (isSelected)
            {
                Widgets.DrawHighlight(rect);
            }

            if (Mouse.IsOver(rect))
            {
                Widgets.DrawLightHighlight(rect);
            }

            if (Widgets.ButtonInvisible(rect))
            {
                selectedHolotape = holotape;
            }

            var iconRect = new Rect(rect.x + 5f, rect.y + 5f, 45f, 45f);
            Widgets.ThingIcon(iconRect, holotape);

            var labelRect = new Rect(iconRect.xMax + 10f, rect.y, rect.width - 65f, 25f);
            Text.Font = GameFont.Small;
            GUI.color = TerminalColors.PrimaryColor;
            Widgets.Label(labelRect, comp.ContentDef.LabelCap);
            GUI.color = Color.white;

            var infoRect = new Rect(labelRect.x, labelRect.yMax, labelRect.width, 15f);
            Text.Font = GameFont.Tiny;
            GUI.color = TerminalColors.PrimaryColor * 0.7f;
            string info = $"{comp.ContentDef.category}";
            if (!comp.ContentDef.author.NullOrEmpty())
                info += $" • {comp.ContentDef.author}";
            if (comp.HasBeenRead)
                info += " • Read";
            Widgets.Label(infoRect, info);
            GUI.color = Color.white;

            var entryRect = new Rect(infoRect.x, infoRect.yMax, infoRect.width, 15f);
            GUI.color = TerminalColors.PrimaryColor * 0.7f;
            if (comp.ContentDef.entries != null)
            {
                Widgets.Label(entryRect, $"{comp.ContentDef.entries.Count} {(comp.ContentDef.entries.Count == 1 ? "entry" : "entries")}");
            }
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
        }
    }
}
