using HarmonyLib;
using RimWorld;
using Verse;

namespace FCP.Core
{
	[HarmonyPatch(typeof(JobGiver_OptimizeApparel), "ApparelScoreGain")]
	public static class JobGiver_OptimizeApparel_ApparelScoreGain_Patch
	{
		public static void Postfix(Pawn pawn, Apparel ap, ref float __result)
		{
			if (!ap.def.CanUseByXenotype(pawn.genes?.Xenotype))
			{
				__result = -1000f;
			}
		}
	}
}
