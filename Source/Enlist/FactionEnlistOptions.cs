using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace  FCP.Enlist
{
	public class ProvisionOption
    {
		public string provisionsLabelKey;
		public string provisionsDescKey;
		public string provisionsButtonIconTexPath;
		public List<ProvisionRecord> provisions;
		public float provisionsRestockInDays;
	}
	public class ProvisionRecord
	{
		public ThingDef thing;

		public ThingDef stuff;

		public IntRange amountRange;
	}
	public class WorkOption
	{
		public string workLabelKey;
		public string workDescKey;
		public string workButtonIconTexPath;
		public List<SkillGain> experienceGainsPerHour;
		public int? silverGainPerHour;
		public float? additionalRestFall;
		public int? tendCaravanMembersEveryTicks;
		public List<ThingDef> medicinesToTend;
		public float? immunityGainSpeedMultiplier;
	}

	public class ReinforcementOption
	{
		public FloatRange relationsRange;

		public IntRange pointsRange;
	}

	public class FactionEnlistOptions : DefModExtension
    {
		public List<FactionEnlistOptionsDef> enlistOptionsDefs;
		public bool dontGiveEnlistmentOptions;
		public bool ignoreAutoAssignedDefs;
	}
}
