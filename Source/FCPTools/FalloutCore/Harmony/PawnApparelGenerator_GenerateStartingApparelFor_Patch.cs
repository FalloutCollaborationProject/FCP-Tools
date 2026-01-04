using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FCP.Core
{
	[HarmonyPatchCategory(FCPCoreMod.LatePatchesCategory)]
	[HarmonyPatch(typeof(PawnApparelGenerator), "GenerateStartingApparelFor")]
	public static class PawnApparelGenerator_GenerateStartingApparelFor_Patch
	{
		public static List<ThingStuffPair> staticList = new List<ThingStuffPair>();
		private static FieldInfo allApparelPairsField;

		public static void Prefix(Pawn pawn)
		{
			var xenotype = pawn.genes?.Xenotype;
			if (xenotype != null)
			{
				if (allApparelPairsField == null)
				{
					allApparelPairsField = typeof(PawnApparelGenerator).GetField("allApparelPairs", BindingFlags.NonPublic | BindingFlags.Static);
				}
				var allApparelPairs = (List<ThingStuffPair>)allApparelPairsField.GetValue(null);
				staticList = allApparelPairs.Where(x => !x.thing.CanUseByXenotype(xenotype)).ToList();
				allApparelPairs.RemoveAll(x => staticList.Contains(x));
			}
		}

		public static void Postfix()
		{
			if (staticList.Any())
			{
				var allApparelPairs = (List<ThingStuffPair>)allApparelPairsField.GetValue(null);
				allApparelPairs.AddRange(staticList);
				staticList.Clear();
			}
		}
	}
}
