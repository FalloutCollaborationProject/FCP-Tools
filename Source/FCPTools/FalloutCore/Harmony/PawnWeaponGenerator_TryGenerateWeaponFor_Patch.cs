using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FCP.Core
{
	[HarmonyPatchCategory(FCPCoreMod.LatePatchesCategory)]
	[HarmonyPatch(typeof(PawnWeaponGenerator), "TryGenerateWeaponFor")]
	public static class PawnWeaponGenerator_TryGenerateWeaponFor_Patch
	{
		public static List<ThingStuffPair> staticList = new List<ThingStuffPair>();
		private static FieldInfo allWeaponPairsField;

		public static void Prefix(Pawn pawn)
		{
			var xenotype = pawn.genes?.Xenotype;
			if (xenotype != null)
			{
				if (allWeaponPairsField == null)
				{
					allWeaponPairsField = typeof(PawnWeaponGenerator).GetField("allWeaponPairs", BindingFlags.NonPublic | BindingFlags.Static);
				}
				var allWeaponPairs = (List<ThingStuffPair>)allWeaponPairsField.GetValue(null);
				staticList = allWeaponPairs.Where(x => !x.thing.CanUseByXenotype(xenotype)).ToList();
				allWeaponPairs.RemoveAll(x => staticList.Contains(x));
			}
		}

		public static void Postfix()
		{
			if (staticList.Any())
			{
				var allWeaponPairs = (List<ThingStuffPair>)allWeaponPairsField.GetValue(null);
				allWeaponPairs.AddRange(staticList);
				staticList.Clear();
			}
		}
	}
}
