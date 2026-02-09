using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace FCP.Enlist;

public class BuyOutOption
{
	public float price;
	public string letterTitleContractTerminatedKey;
	public string letterLabelContractTerminatedKey;
	public string buttonLabelKey;
	public string buttonDescKey;
	public string buttonIconTexPath;
}

public class FactionEnlistOptionsDef : Def
{
	public Type workerClass = typeof(FactionEnlistWorker);
	private FactionEnlistWorker workerInt;
	public FactionEnlistWorker Worker
	{
		get
		{
			if (workerInt == null)
			{
				workerInt = Activator.CreateInstance(workerClass, new object[] { this }) as FactionEnlistWorker;
			}
			return workerInt;
		}
	}
	public bool autoAssignToAllFactions;
	public TechLevel? requiredTechLevel;
	public bool overrideModExtension;

	public ThingDef currencyDef;

	public int? minGoodwillRequrementToEnlist;
	public int enlistGoodwillGain;
	public int minGoodwillUponEnlist;

	public string enlistLetterTitleKey;
	public string enlistLetterLabelKey;

	public string enlistButtonLabelKey;
	public string enlistButtonDescKey;
	public string enlistButtonIconTexPath;
	public string enlistRequrementsNotSatisfiedKey;
	public string enlistedToHostileFactionKey;
	public SoundDef enlistedSoundDef;

	public string enlistedWithKey;
	public bool missionsAreEnabled;
	public string missionsLabelKey;
	public string missionsDescKey;
	public string missionsButtonIconTexPath;
	public string missionsBackgroundMenuTexPath;
	public string missionsMenuDescriptionKey;
	public string missionsMenuSelectQuestKey;
	public string missionsMenuStartQuestKey;
	public string missionsMenuStartQuestNoEnoughMoneyKey;
	public int missionsQuestCost;
	public SoundDef missionsMenuAmbientSoundDef;

	public bool			mechSerumIsEnabled;
	public string		mechSerumLabelKey;
	public string		mechSerumDescKey;
	public string		mechSerumButtonIconTexPath;
	public int			mechSerumCost;
	public string		mechSerumCostRequirementKey;

	public bool salaryIsEnabled;
	public ThingDef salaryDef;
	public string salaryLabelKey;
	public string salaryDescKey;
	public string salaryButtonIconTexPath;
	public FloatRange salaryRange;
	public int salaryPeriodDays;

	public List<ProvisionOption> provisionOptions;

	public bool storageIsEnabled;
	public string storageLabelKey;
	public string storageDescKey;
	public string storageButtonIconTexPath;

	public float recreationGainPerTick;
	public List<WorkOption> workOptions;

	public bool turnInIsEnabled;
	public string turnInLabelKey;
	public string turnInDescKey;
	public string turnInButtonIconTexPath;
	public TraderKindDef turnInTraderKind;
	public string turnInTraderNameKey;
	public ThoughtDef turnInThought;
	public int turnInRefreshSilverInDays;

	public bool dropPodServiceIsEnabled;
	public string dropPodServiceLabelKey;
	public string dropPodServiceDescKey;
	public string dropPodServiceButtonIconTexPath;
	public int dropPodServiceCost;
	public string dropPodServiceCostRequirementKey;

	public bool shuttleServiceIsEnabled;
	public string shuttleServiceLabelKey;
	public string shuttleServiceDescKey;
	public string shuttleServiceButtonIconTexPath;
	public int shuttleServiceCost;
	public string shuttleServiceCostRequirementKey;

	public int resignGoodwillGain;
	public string resignMenuTextKey;
	public string resignLetterTitleKey;
	public string resignLetterLabelKey;
	public string resignButtonLabelKey;
	public string resignButtonDescKey;
	public string resignButtonIconTexPath;

	public string kickOutLetterTitleKey;
	public string kickOutLetterLabelKey;

	public bool reinforcementsAreEnabled;
	public string reinforcementsButtonLabelKey;
	public string reinforcementsButtonDescKey;
	public string reinforcementsButtonIconTexPath;

	public List<ReinforcementOption> reinforcementOptions;
	public int reinforcementCallGoodwillCost;
	public int reinforcementCallCooldownTicks;

	public bool settlementTradingLockedBehindEnlist;
	public string settlementTradingLockedBehindEnlistReasonKey;

	public bool settlementGiftingLockedBehindEnlist;
	public string settlementGiftingLockedBehindEnlistReasonKey;

	public bool promoteOptionEnabled;
	public string promoteButtonLabelKey;
	public string promoteButtonDescKey;
	public string promoteButtonIconTexPath;
	public string promoteRequrementsNotSatisfiedKey;
	public List<SkillRequirement> promoteSkillRequirements;
	public List<ProvisionOption> promoteProvisionOptions;

	public string protocolButtonLabelKey;
	public string protocolButtonDescKey;
	public string protocolButtonIconTexPath;
	public string protocolEnterText;
	public string protocolInvalidWarning;
	public List<ProtocolOption> protocolOptions;
	public BuyOutOption buyOutOption;


}