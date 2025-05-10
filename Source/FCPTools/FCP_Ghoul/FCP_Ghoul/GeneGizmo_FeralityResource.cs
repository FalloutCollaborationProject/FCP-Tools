using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
namespace FCP_Ghoul
{
    [StaticConstructorOnStartup]
    public class GeneGizmo_Ferality : Gizmo
    {
		//private static readonly Texture2D FeralityCostTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.78f, 0.72f, 0.66f));

		private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.729f, 0.2f, 0.239f));

		private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

		Gene_Ferality gene;
        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }
        public GeneGizmo_Ferality(Gene_Ferality gene)
        {
            this.gene = gene;
            Order = -100f;
        }
		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			Rect rect2 = rect.ContractedBy(8f);//was 6f
			Widgets.DrawWindowBackground(rect);
			Rect rect3 = rect2;
			rect3.height = rect.height / 2f;
			Text.Font = GameFont.Small;
			Widgets.Label(rect3, "Ferality");
			Rect rect4 = rect2;
			rect4.yMin = rect2.y + rect2.height / 2f;
			float fillPercent = gene.quantity / Mathf.Max(1f, 100);
			Widgets.FillableBar(rect4, fillPercent, FullShieldBarTex, EmptyShieldBarTex, doBorder: true);
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(rect4, (gene.quantity).ToString("F0") + " / " + ( 100f).ToString("F0"));
			Text.Anchor = TextAnchor.UpperLeft;
			TooltipHandler.TipRegion(rect2, gene.def.description);
			return new GizmoResult(GizmoState.Clear);
		}



	}
}
