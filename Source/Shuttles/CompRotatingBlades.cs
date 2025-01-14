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

	[HotSwappable]
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
			Vector3 bladePos = drawLoc + graphicData.positionOffset;
			bladePos.y += 1;
			graphicData.bladeGraphic.Graphic.Draw(bladePos, Rot4.South, this.parent, rotationRate);
		}
	}
}