using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FCP_Shuttles
{
	public class BladeGraphicData
	{
		public GraphicData bladeGraphic;
		public Vector3 positionOffset;
		public float spinRate;
	}

	public class CompProperties_RotatingBlades : CompProperties
	{
		public List<BladeGraphicData> bladeGraphicsData;
		public CompProperties_RotatingBlades()
		{
			this.compClass = typeof(CompRotatingBlades);
		}
	}

	public class CompRotatingBlades : ThingComp
	{
		private List<float> rotationRates = new List<float>();

		public CompProperties_RotatingBlades Props => base.props as CompProperties_RotatingBlades;

		public override void PostPostMake()
		{
			base.PostPostMake();
			rotationRates = new List<float>(Props.bladeGraphicsData.Count);
			for (int i = 0; i < Props.bladeGraphicsData.Count; i++)
			{
				rotationRates.Add(0f);
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Collections.Look(ref rotationRates, "rotationRates", LookMode.Value);
		}

		public override void CompTick()
		{
			base.CompTick();
			for (int i = 0; i < rotationRates.Count; i++)
			{
				rotationRates[i] = (rotationRates[i] + Props.bladeGraphicsData[i].spinRate) % 360;
			}
		}

		[HarmonyPatch(typeof(Skyfaller), "DrawAt")]
		public static class Skyfaller_DrawAt_Patch
		{
			public static void Postfix(Skyfaller __instance, Vector3 drawLoc)
			{
				var comp = __instance.TryGetComp<CompRotatingBlades>();
				if (comp != null)
				{
					comp.DrawRotatingBlades(drawLoc);
				}
			}
		}

		public override void PostDraw()
		{
			base.PostDraw();
			DrawRotatingBlades(parent.DrawPos);
		}

		public void DrawRotatingBlades(Vector3 drawLoc)
		{
			for (int i = 0; i < Props.bladeGraphicsData.Count; i++)
			{
				DrawRotatingBlade(drawLoc, rotationRates[i], Props.bladeGraphicsData[i]);
			}
		}

		private void DrawRotatingBlade(Vector3 drawLoc, float rotationRate, BladeGraphicData graphicData)
		{
			float angle = rotationRate;
			Vector3 bladePos = drawLoc + graphicData.positionOffset.RotatedBy(angle);
			bladePos.y += 10;
			Log.Message("bladePos: " + bladePos + " - " + graphicData.bladeGraphic.Graphic);
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(bladePos, Quaternion.identity, graphicData.bladeGraphic.drawSize);
			Graphics.DrawMesh(MeshPool.plane10, matrix, graphicData.bladeGraphic.Graphic.MatSingle, 0);
		}
	}
}