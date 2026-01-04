using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FCP.Core
{
	[HarmonyPatch(typeof(ApparelProperties), "PawnCanWear", new Type[] { typeof(Pawn), typeof(bool) })]
	public static class ApparelProperties_PawnCanWear_Patch
	{
		public static Dictionary<ApparelProperties, ThingDef> mappedApparels = new Dictionary<ApparelProperties, ThingDef>();

		public static void Postfix(ApparelProperties __instance, Pawn pawn, ref bool __result)
		{
			if (__result)
			{
				if (!mappedApparels.TryGetValue(__instance, out var def))
				{
					mappedApparels[__instance] = def = DefDatabase<ThingDef>.AllDefs.FirstOrDefault(x => x.apparel == __instance);
				}
				if (def != null && !def.CanUseByXenotype(pawn.genes?.Xenotype))
				{
					__result = false;
				}
			}
		}
	}
}
