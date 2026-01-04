using HarmonyLib;
using RimWorld;
using Verse;

namespace FCP.Core
{
	[HarmonyPatch(typeof(EquipmentUtility), "CanEquip", [typeof(Thing), typeof(Pawn), typeof(string), typeof(bool)],
	[ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal])]
	public static class EquipmentUtility_CanEquip_Patch
	{
		public static void Postfix(ref bool __result, Thing thing, Pawn pawn, ref string cantReason)
		{
			if (__result && !thing.def.CanUseByXenotype(pawn.genes?.Xenotype))
			{
				cantReason = "FCP_WrongXenotype".Translate();
				__result = false;
			}
		}
	}
}
