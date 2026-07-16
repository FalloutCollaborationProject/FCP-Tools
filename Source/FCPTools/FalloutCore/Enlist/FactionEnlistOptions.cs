using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace FCP.Enlist;

public class ProvisionOption
{
	public string provisionsLabelKey;
	public string provisionsDescKey;
	public string provisionsButtonIconTexPath;
	public List<ProvisionRecord> provisions;
	public List<RecruitRecord> recruits;
	public float provisionsRestockInDays;
}

public class ProvisionRecord
{
	public ThingDef thing;
	public ThingDef stuff;
	public IntRange amountRange;
}

public class RecruitRecord
{
	public PawnKindDef pawnKind;
	public IntRange count = new IntRange(1, 1);
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
	public PawnGroupKindDef groupKind;
	public List<PawnKindDef> specificPawnKinds;
	public IntRange specificPawnCount = new IntRange(1, 3);
}

public class TitleWageBonus
{
	public RoyalTitleDef title;
	public float multiplier = 1f;
}

public class AbilityTrainingOption
{
	public string labelKey;
	public string descKey;
	public string iconTexPath;
	public int trainingDurationDays = 5;
	public int cost;
	public AbilityDef abilityDef;
	public HediffDef hediffDef;
	public TraitDef traitDef;
	public int traitDegree;
}

public class DeliveryQuestTemplate
{
	public ThingDef thingToDeliver;
	public ThingDef stuff;
	public IntRange countRange = new IntRange(50, 100);
	public IntRange rewardRange = new IntRange(200, 500);
	public ThingDef rewardDef;
	public int durationDays = 30;
}

public class FactionEnlistOptions : DefModExtension
{
	public List<FactionEnlistOptionsDef> enlistOptionsDefs;
	public bool dontGiveEnlistmentOptions;
	public bool ignoreAutoAssignedDefs;
}