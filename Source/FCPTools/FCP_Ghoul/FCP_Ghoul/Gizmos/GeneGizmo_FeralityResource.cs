using UnityEngine;
using Verse;

namespace FCP_Ghoul
{
	[StaticConstructorOnStartup]
	public class GeneGizmo_Ferality : Gizmo
	{
		//private static readonly Texture2D FeralityCostTex = SolidColorMaterials
		//	.NewSolidColorTexture(new Color(0.78f, 0.72f, 0.66f));
		private static readonly Texture2D FullShieldBarTex = SolidColorMaterials
			.NewSolidColorTexture(new Color(0.729f, 0.2f, 0.239f));

		private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials
			.NewSolidColorTexture(Color.clear);

		private readonly Gene_Manic _gene;

		public GeneGizmo_Ferality(Gene_Manic gene)
		{
			_gene = gene;
			Order = -100f;
		}

		public override float GetWidth(float maxWidth)
		{
			return 140f;
		}

		public override GizmoResult GizmoOnGUI(Vector2 topLeft,
			float maxWidth, GizmoRenderParms parms)
		{
			Rect outerRect = new(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			Rect innerRect = outerRect.ContractedBy(8f);

			Widgets.DrawWindowBackground(outerRect);

			Rect labelRect = new(innerRect.x, innerRect.y,
				innerRect.width, innerRect.height / 2f);
			Text.Font = GameFont.Small;
			Widgets.Label(labelRect, "Ferality"); // TODO: change to something else (keyed string too)

			Rect barRect = new(innerRect.x, innerRect.y + innerRect.height / 2f,
				innerRect.width, innerRect.height / 2f);
			float fillPercent = Mathf.Clamp01(_gene.CurQuantity / 100f);
			Widgets.FillableBar(barRect, fillPercent, FullShieldBarTex,
				EmptyShieldBarTex, doBorder: true);

			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(barRect, $"{_gene.CurQuantity:F0} / 100"); // TODO: uh what? lol
			Text.Anchor = TextAnchor.UpperLeft;

			TooltipHandler.TipRegion(innerRect, _gene.def.description);

			return new GizmoResult(GizmoState.Clear);
		}
	}
}