using RimWorld;
using UnityEngine;
using Verse;

namespace FCP.Core.Ghouls
{
    [StaticConstructorOnStartup]
    public class GeneGizmo_Ferality : Gizmo
    {
        private Gene_Ferality gene;
        private static readonly Texture2D FullBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.8f, 0.2f, 0.2f));
        private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.1f, 0.1f, 0.1f));

        public GeneGizmo_Ferality(Gene_Ferality gene)
        {
            this.gene = gene;
            Order = -100f;
        }

        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Widgets.DrawWindowBackground(rect);

            // Title
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperCenter;
            Rect labelRect = new Rect(rect.x, rect.y + 2f, rect.width, 20f);
            Widgets.Label(labelRect, "Ferality");

            // Bar background
            Rect barRect = new Rect(rect.x + 10f, rect.y + 25f, rect.width - 20f, 20f);
            Widgets.DrawBoxSolid(barRect, EmptyBarTex.GetPixel(0, 0));

            float fillWidth = barRect.width * gene.FeralityPercent;
            Rect fillRect = new Rect(barRect.x, barRect.y, fillWidth, barRect.height);
            
            Color barColor;
            if (gene.FeralityPercent < 0.25f)
                barColor = Color.Lerp(Color.green, Color.yellow, gene.FeralityPercent * 4f);
            else if (gene.FeralityPercent < 0.5f)
                barColor = Color.Lerp(Color.yellow, new Color(1f, 0.5f, 0f), (gene.FeralityPercent - 0.25f) * 4f);
            else if (gene.FeralityPercent < 0.75f)
                barColor = Color.Lerp(new Color(1f, 0.5f, 0f), Color.red, (gene.FeralityPercent - 0.5f) * 4f);
            else
                barColor = Color.Lerp(Color.red, new Color(0.6f, 0f, 0f), (gene.FeralityPercent - 0.75f) * 4f);

            Widgets.DrawBoxSolid(fillRect, barColor);
            Widgets.DrawBox(barRect);

            Text.Anchor = TextAnchor.MiddleCenter;
            Rect textRect = new Rect(rect.x, rect.y + 47f, rect.width, 20f);
            Widgets.Label(textRect, gene.Ferality.ToString("F0") + "%");

            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            if (Mouse.IsOver(rect))
            {
                string tooltip = $"Ferality: {gene.Ferality:F1}%\n\nThis ghoul is slowly losing their mind. Use drugs to reduce ferality and prevent them from going feral.";
                if (gene.Ferality >= 100f)
                    tooltip += "\n\n<color=red>About to go feral!</color>";
                TooltipHandler.TipRegion(rect, tooltip);
            }

            return new GizmoResult(GizmoState.Clear);
        }
    }
}
