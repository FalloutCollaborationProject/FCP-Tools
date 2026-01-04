using RimWorld;
using Verse;

namespace FCP.Core
{
	public static class XenotypeUtility
	{
		public static bool CanUseByXenotype(this Def def, XenotypeDef xenotype)
		{
			var extension = def.GetModExtension<XenotypeExtension>();
			if (extension == null)
			{
				return true;
			}
			if (extension.whitelistedXenotypes != null && extension.whitelistedXenotypes.Any())
			{
				return extension.whitelistedXenotypes.Contains(xenotype);
			}
			if (extension.blacklistedXenotypes != null && extension.blacklistedXenotypes.Any())
			{
				return !extension.blacklistedXenotypes.Contains(xenotype);
			}
			return true;
		}
	}
}
