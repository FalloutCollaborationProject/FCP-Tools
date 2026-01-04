using HarmonyLib;
using RimWorld;
using Verse;

namespace FCP.Core
{
	[HarmonyPatch(typeof(WorkGiver_Researcher), "HasJobOnThing")]
	public static class WorkGiver_Researcher_HasJobOnThing_Patch
	{
		public static void Postfix(Pawn pawn, ref bool __result)
		{
			if (!__result)
			{
				return;
			}
			var project = Find.ResearchManager.GetProject();
			if (project == null)
			{
				return;
			}
			var xenotype = pawn.genes?.Xenotype;
			if (xenotype == null)
			{
				return;
			}
			if (!project.CanUseByXenotype(xenotype))
			{
				__result = false;
			}
		}
	}
}
