using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using FCP.Core.Shuttles;

namespace FCP.Enlist;

public class WorldObjectCompProperties_Enlist : WorldObjectCompProperties
{
	public WorldObjectCompProperties_Enlist()
	{
		compClass = typeof(WorldObjectCompEnlist);
	}
}
public class WorldObjectCompEnlist : WorldObjectComp
{
	public WorldObjectCompEnlist()
	{
		generatedQuests = new List<Quest>();
		caravanOptions = new Dictionary<Caravan, CaravanOptions>();
		pawnTraders = new Dictionary<FactionEnlistOptionsDef, PawnTrader>();
	}

	public List<Quest> generatedQuests;
	public int generatedQuestsLastTick;
	public Dictionary<int, ProvisionsInfo> provisionInfos;
	public Dictionary<int, ProvisionsInfo> promotedProvisionInfos;
	private Dictionary<Caravan, CaravanOptions> caravanOptions;
	private Dictionary<FactionEnlistOptionsDef, PawnTrader> pawnTraders;
	private Dictionary<FactionEnlistOptionsDef, PawnTrader> bountyHunterTraders;
	private Dictionary<FactionEnlistOptionsDef, ExclusiveTrader> exclusiveTraders;
	private Dictionary<FactionEnlistOptionsDef, ExclusiveTrader> taxCollectorTraders;
	private Dictionary<FactionEnlistOptionsDef, bool> promotedByOptions;
	public Dictionary<FactionEnlistOptionsDef, AbilityTrainingSession> activeTrainingSessions;
	public Dictionary<FactionEnlistOptionsDef, DeliveryQuestList> deliveryBoards;

	private List<FactionEnlistOptionsDef> optionDefs;
	public List<FactionEnlistOptionsDef> OptionsDefs => optionDefs ??= parent.Faction.GetEnlistOptions();
	public CaravanOptions GetCaravanOptions(Caravan caravan)
	{
		caravanOptions ??= new Dictionary<Caravan, CaravanOptions>();
		if (!caravanOptions.TryGetValue(caravan, out CaravanOptions options))
		{
			options = caravanOptions[caravan] = new CaravanOptions(parent);
		}
		return options;
	}
	public override void CompTick()
	{
		if (activeTrainingSessions != null)
		{
			foreach (FactionEnlistOptionsDef def in activeTrainingSessions.Keys.ToList())
			{
				AbilityTrainingSession session = activeTrainingSessions[def];
				if (session.IsDone)
				{
					session.Complete();
					if (!def.abilityTrainingCompleteLetterTitleKey.NullOrEmpty())
					{
						Find.LetterStack.ReceiveLetter(
							def.abilityTrainingCompleteLetterTitleKey.Translate(parent.Faction.Named("FACTION")),
							def.abilityTrainingCompleteLetterLabelKey.Translate(session.trainee.Named("PAWN"), parent.Faction.Named("FACTION")),
							LetterDefOf.PositiveEvent,
							session.trainee);
					}
					activeTrainingSessions.Remove(def);
				}
			}
		}
		List<Caravan> caravans = new List<Caravan>();
		Find.World.worldObjects.GetPlayerControlledCaravansAt(parent.Tile, caravans);
		if (caravans.Any())
		{
			List<FactionEnlistOptionsDef> optionsDefs = OptionsDefs;
			foreach (Caravan caravan in caravans)
			{
				if (caravan != null)
				{
					CaravanOptions caravanOptions = GetCaravanOptions(caravan);
					if (caravanOptions.curWorkOption != null)
					{
						if (caravan.pather.moving || caravan.Destroyed || caravan.Tile != parent.Tile)
						{
							caravanOptions.Reset();
						}
						else
						{
							foreach (Pawn pawn in caravan.PawnsListForReading.Where(x => x.Faction == Faction.OfPlayer && !x.IsPrisoner && x.RaceProps.Humanlike).ToList())
							{
								if (!caravan.NightResting)
								{
									if (caravanOptions.curWorkOption.experienceGainsPerHour != null)
									{
										foreach (SkillGain skillGain in caravanOptions.curWorkOption.experienceGainsPerHour)
										{
											float xpGain = skillGain.amount / ((float)GenDate.TicksPerHour);
											pawn.skills.Learn(skillGain.skill, xpGain);
										}
									}

									if (caravanOptions.curWorkOption.additionalRestFall.HasValue && pawn.needs?.rest != null)
									{
										pawn.needs.rest.CurLevel -= pawn.needs.rest.RestFallPerTick / caravanOptions.curWorkOption.additionalRestFall.Value;
									}

									if (caravanOptions.curWorkOption.silverGainPerHour.HasValue && Find.TickManager.TicksGame % GenDate.TicksPerHour == 0)
									{
										FactionEnlistOptionsDef matchingDef = optionsDefs.FirstOrDefault();
										Thing newSilver = ThingMaker.MakeThing(matchingDef.salaryDef);
										newSilver.stackCount = caravanOptions.curWorkOption.silverGainPerHour.Value;
										CaravanInventoryUtility.GiveThing(caravan, newSilver);
									}
								}

								if (caravanOptions.curWorkOption.tendCaravanMembersEveryTicks.HasValue && Find.TickManager.TicksGame
								    % caravanOptions.curWorkOption.tendCaravanMembersEveryTicks.Value == 0)
								{
									if (pawn.health.HasHediffsNeedingTend())
									{
										Medicine medicine = caravanOptions.curWorkOption.medicinesToTend != null ?
											ThingMaker.MakeThing(caravanOptions.curWorkOption.medicinesToTend.RandomElement()) as Medicine : null;
										TendUtility.DoTend(null, pawn, medicine);
									}
									List<Hediff> hediffsToRemove = pawn.health.hediffSet.hediffs.Where(x => x.def == HediffDefOf.Heatstroke || x.def == HediffDefOf.Hypothermia).ToList();
									foreach (Hediff hediff in hediffsToRemove)
									{
										pawn.health.RemoveHediff(hediff);
									}
								}
							}
						}
					}

					if (parent.Faction != null && parent.Faction != Faction.OfPlayer && optionsDefs != null)
					{
						foreach (FactionEnlistOptionsDef optionDef in optionsDefs)
						{
							if (WorldEnlistTracker.Instance.EnlistedTo(parent.Faction, optionDef))
							{
								CaravanOptions caravanOpts = GetCaravanOptions(caravan);
																foreach (Pawn pawn in caravan.PawnsListForReading.Where(x => x.Faction == Faction.OfPlayer && !x.IsPrisoner && x.RaceProps.Humanlike))
								{
									if (pawn.needs?.joy != null)
									{
										pawn.needs.joy.CurLevel += optionDef.recreationGainPerTick;
										pawn.needs.joy.lastGainTick = Find.TickManager.TicksGame;
									}
								}
								if (optionDef.autoFeedIsEnabled && caravanOpts.autoFeedEnabled && Find.TickManager.TicksGame % GenDate.TicksPerHour == 0)
								{
									foreach (Pawn pawn in caravan.PawnsListForReading)
									{
										if (pawn.IsColonist && !pawn.IsPrisoner && pawn.needs?.food != null)
											pawn.needs.food.CurLevel = Mathf.Max(pawn.needs.food.CurLevel, 0.7f);
									}
								}
							}
						}
					}
				}
			}

			if (pawnTraders != null && optionsDefs != null)
			{
				foreach (FactionEnlistOptionsDef optionDefs in optionsDefs)
				{

					PawnTrader pawnTrader = pawnTraders.TryGetValue(optionDefs, out PawnTrader value) ? value : null;
					if (pawnTrader != null && pawnTrader.refreshDays > 0 && Find.TickManager.TicksGame % (pawnTrader.refreshDays * GenDate.TicksPerDay) == 0)
					{
						pawnTrader.GenerateThings();
					}
				}
			}
			if (bountyHunterTraders != null && optionsDefs != null)
			{
				foreach (FactionEnlistOptionsDef def in optionsDefs)
				{
					if (bountyHunterTraders.TryGetValue(def, out PawnTrader bountyTrader) && bountyTrader != null && bountyTrader.refreshDays > 0)
					{
						if (Find.TickManager.TicksGame % (bountyTrader.refreshDays * GenDate.TicksPerDay) == 0)
							bountyTrader.GenerateThings();
					}
				}
			}
			if (exclusiveTraders != null && optionsDefs != null)
			{
				foreach (FactionEnlistOptionsDef def in optionsDefs)
				{
					if (exclusiveTraders.TryGetValue(def, out ExclusiveTrader exTrader) && exTrader != null && def.exclusiveTraderRefreshInDays > 0)
					{
						if (Find.TickManager.TicksGame % (def.exclusiveTraderRefreshInDays * GenDate.TicksPerDay) == 0)
							exTrader.GenerateThings();
					}
				}
			}
			if (taxCollectorTraders != null && optionsDefs != null)
			{
				foreach (FactionEnlistOptionsDef def in optionsDefs)
				{
					if (taxCollectorTraders.TryGetValue(def, out ExclusiveTrader tcTrader) && tcTrader != null && def.taxCollectorRefreshInDays > 0)
					{
						if (Find.TickManager.TicksGame % (def.taxCollectorRefreshInDays * GenDate.TicksPerDay) == 0)
							tcTrader.GenerateThings();
					}
				}
			}
		}
	}
	public void AddQuest(Quest quest, FactionEnlistOptionsDef optionsDef)
	{
		Find.QuestManager.Add(quest);
		generatedQuests.Remove(quest);
		WorldEnlistTracker worldTracker = WorldEnlistTracker.Instance;
		if (!worldTracker.factionOptionsContainer[parent.Faction].factionsWithQuests.ContainsKey(optionsDef))
		{
			worldTracker.factionOptionsContainer[parent.Faction].factionsWithQuests[optionsDef] = new QuestContainer();
		}
		if (worldTracker.factionOptionsContainer[parent.Faction].factionsWithQuests[optionsDef].availableQuests.ContainsKey(parent))
		{
			worldTracker.factionOptionsContainer[parent.Faction].factionsWithQuests[optionsDef].availableQuests[parent].quests.Add(quest);
		}
		else
		{
			worldTracker.factionOptionsContainer[parent.Faction].factionsWithQuests[optionsDef].availableQuests[parent] = new QuestsContainer(quest);
		}
	}
	public void GenerateQuests()
	{
		Patch_TryFindTile.worldObject = parent;
		int questCountToGenerate = Rand.RangeInclusive(15, 20);
		float points = StorytellerUtility.DefaultThreatPointsNow(Find.World);
		List<QuestScriptDef> questDefsToProcess = DefDatabase<QuestScriptDef>.AllDefs.Where(x => !x.isRootSpecial && x.IsRootAny).ToList();

		while (generatedQuests.Count < questCountToGenerate)
		{

			if (!questDefsToProcess.Any())
			{
				break;
			}
			QuestScriptDef newQuestCandidate = questDefsToProcess.RandomElement();
			questDefsToProcess.Remove(newQuestCandidate);
			try
			{
				Slate slate = new Slate();
				slate.Set("points", points);
				if (newQuestCandidate == QuestScriptDefOf.LongRangeMineralScannerLump)
				{
					slate.Set("targetMineable", ThingDefOf.MineableGold);
					slate.Set("worker", PawnsFinder.AllMaps_FreeColonists.FirstOrDefault());
				}
				if (newQuestCandidate.CanRun(slate, Find.World))
				{
					Quest quest = QuestGen.Generate(newQuestCandidate, slate);
					generatedQuests.Add(quest);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex + " can't generate " + newQuestCandidate);
			}
		}

		Patch_TryFindTile.worldObject = null;
		generatedQuestsLastTick = Find.TickManager.TicksGame;
	}

	public override string CompInspectStringExtra()
	{
		if (parent.Faction == Faction.OfPlayer)
		{
			StringBuilder stringBuilder = new StringBuilder();
			WorldEnlistTracker worldTracker = WorldEnlistTracker.Instance;
			foreach (Faction faction in worldTracker.EnlistedFactions())
			{
				foreach (FactionEnlistOptionsDef optionDef in faction.GetEnlistOptions())
				{
					stringBuilder.AppendInNewLine(optionDef.enlistedWithKey.Translate(faction.Named("FACTION")));
				}
			}
			return stringBuilder.ToString().TrimEndNewlines();
		}
		return null;
	}

	private Caravan tmpCaravan;
	public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
	{
		Faction faction = parent.Faction;
		if (faction != null && faction != Faction.OfPlayer)
		{
			WorldEnlistTracker worldTracker = WorldEnlistTracker.Instance;
			int order = 1;
			if (OptionsDefs != null)
			{
				foreach (FactionEnlistOptionsDef optionDef in OptionsDefs)
				{
					if (optionDef.buyOutOption != null && !worldTracker.Bought(faction, optionDef))
					{
						Command_Action command_Enlist = new Command_Action
						{
							defaultLabel = optionDef.buyOutOption.buttonLabelKey.Translate(faction.Named("FACTION")),
							defaultDesc = optionDef.buyOutOption.buttonDescKey.Translate(faction.Named("FACTION")),
							icon = ContentFinder<Texture2D>.Get(optionDef.buyOutOption.buttonIconTexPath),
							action = delegate
							{
								optionDef.Worker.Buy(faction, caravan);
							},
							order = order
						};
						if (!optionDef.Worker.CanBuy(faction, caravan, out string cannotBuyReason))
						{
							command_Enlist.Disable(cannotBuyReason);
						}
						yield return command_Enlist;
						order++;
					}

					if (!worldTracker.EnlistedTo(faction, optionDef))
					{
						Command_Action command_Enlist = new Command_Action
						{
							defaultLabel = optionDef.enlistButtonLabelKey.Translate(faction.Named("FACTION")),
							defaultDesc = optionDef.enlistButtonDescKey.Translate(faction.Named("FACTION")),
							icon = ContentFinder<Texture2D>.Get(optionDef.enlistButtonIconTexPath),
							action = delegate
							{
								optionDef.Worker.EnlistTo(faction);
							},
							order = order
						};
						if (!worldTracker.CanEnlist(faction, optionDef, out string cannotEnlistReason))
						{
							command_Enlist.Disable(cannotEnlistReason);
						}
						yield return command_Enlist;
						order++;

						if (optionDef.bountyHunterIsEnabled && optionDef.bountyHunterTraderKind != null)
						{
							bountyHunterTraders ??= new Dictionary<FactionEnlistOptionsDef, PawnTrader>();
							Command_Action command_Bounty = new Command_Action
							{
								defaultLabel = optionDef.bountyHunterLabelKey.Translate(faction.Named("FACTION")),
								defaultDesc = optionDef.bountyHunterDescKey.Translate(faction.Named("FACTION")),
								icon = ContentFinder<Texture2D>.Get(optionDef.bountyHunterButtonIconTexPath),
								action = delegate
								{
									if (!bountyHunterTraders.TryGetValue(optionDef, out PawnTrader bountyTrader))
									{
										bountyTrader = new PawnTrader
										{
											faction = parent.Faction,
											factionOptionDef = optionDef,
											isBountyHunter = true,
											refreshDays = optionDef.bountyHunterRefreshSilverInDays
										};
										bountyTrader.GenerateThings();
										bountyHunterTraders[optionDef] = bountyTrader;
									}
									bountyTrader.caravan = caravan;
									Pawn bestNegotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan, faction, optionDef.bountyHunterTraderKind);
									Find.WindowStack.Add(new Dialog_Trade(bestNegotiator, bountyTrader));
								},
								order = order
							};
							yield return command_Bounty;
							order++;
						}
					}
					else
					{
						if (optionDef.missionsAreEnabled)
						{
							Command_Action command_Mission = new Command_Action
							{
								defaultLabel = optionDef.missionsLabelKey.Translate(faction.Named("FACTION")),
								defaultDesc = optionDef.missionsDescKey.Translate(faction.Named("FACTION")),
								icon = ContentFinder<Texture2D>.Get(optionDef.missionsButtonIconTexPath),
								action = delegate
								{
									DiaNode dianode = new DiaNode("Missions");
									Dialog_Missions missionWindow = new Dialog_Missions(dianode, false, caravan, this, ContentFinder<Texture2D>.Get(optionDef.missionsBackgroundMenuTexPath), optionDef);
									Find.WindowStack.Add(missionWindow);
								},
								order = order
							};
							yield return command_Mission;
							order++;
						}

						if (optionDef.salaryIsEnabled)
						{
							Command_Action command_Salary = new Command_Action
							{
								defaultLabel = optionDef.salaryLabelKey.Translate(faction.Named("FACTION")),
								defaultDesc = optionDef.salaryDescKey.Translate(faction.Named("FACTION")),
								icon = ContentFinder<Texture2D>.Get(optionDef.salaryButtonIconTexPath),
								action = delegate
								{
									worldTracker.factionOptionsContainer[faction].factionsSalaries[optionDef].GiveMoney(optionDef, caravan, faction);
								},
								order = order
							};
							if (!worldTracker.factionOptionsContainer[faction].factionsSalaries.TryGetValue(optionDef, out SalaryInfo salaryInfo) || !salaryInfo.CanPayMoney(optionDef))
							{
								command_Salary.Disable();
							}
							yield return command_Salary;
							order++;

						}

						if (optionDef.provisionOptions != null)
						{
							provisionInfos ??= new Dictionary<int, ProvisionsInfo>();
							foreach (var button in GetProvisionButtons(provisionInfos, optionDef.provisionOptions, faction, caravan, order))
							{
								yield return button;
								order++;
							}
						}

						if (optionDef.exclusiveTraderIsEnabled && optionDef.exclusiveTraderKind != null)
						{
							exclusiveTraders ??= new Dictionary<FactionEnlistOptionsDef, ExclusiveTrader>();
							Command_Action command_ExclusiveTrader = new Command_Action
							{
								defaultLabel = optionDef.exclusiveTraderLabelKey.Translate(faction.Named("FACTION")),
								defaultDesc = optionDef.exclusiveTraderDescKey.Translate(faction.Named("FACTION")),
								icon = ContentFinder<Texture2D>.Get(optionDef.exclusiveTraderButtonIconTexPath),
								action = delegate
								{
									if (!exclusiveTraders.TryGetValue(optionDef, out ExclusiveTrader exTrader))
									{
										exTrader = new ExclusiveTrader
										{
											faction = parent.Faction,
											factionOptionDef = optionDef
										};
										exTrader.GenerateThings();
										exclusiveTraders[optionDef] = exTrader;
									}
									exTrader.caravan = caravan;
									Pawn bestNegotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan, faction, optionDef.exclusiveTraderKind);
									Find.WindowStack.Add(new Dialog_Trade(bestNegotiator, exTrader));
								},
								order = order
							};
							bool meetsGoodwill = faction.GoodwillWith(Faction.OfPlayer) >= optionDef.exclusiveTraderRequiredGoodwill;
							bool meetsTitle = optionDef.exclusiveTraderRequiredTitle == null ||
								PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists.Any(
									p => p.royalty != null && p.royalty.GetCurrentTitleInFaction(faction)?.def.seniority >= optionDef.exclusiveTraderRequiredTitle.seniority);
							if (!meetsGoodwill || !meetsTitle)
							{
								command_ExclusiveTrader.Disable(optionDef.exclusiveTraderRequirementsNotMetKey.Translate(
									optionDef.exclusiveTraderRequiredGoodwill.Named("GOODWILL"),
									faction.Named("FACTION")));
							}
							yield return command_ExclusiveTrader;
							order++;
						}

						if (optionDef.taxCollectorIsEnabled && optionDef.taxCollectorTraderKind != null)
						{
							taxCollectorTraders ??= new Dictionary<FactionEnlistOptionsDef, ExclusiveTrader>();
							Command_Action command_TaxCollector = new Command_Action
							{
								defaultLabel = optionDef.taxCollectorLabelKey.Translate(faction.Named("FACTION")),
								defaultDesc = optionDef.taxCollectorDescKey.Translate(faction.Named("FACTION")),
								icon = ContentFinder<Texture2D>.Get(optionDef.taxCollectorButtonIconTexPath),
								action = delegate
								{
									if (!taxCollectorTraders.TryGetValue(optionDef, out ExclusiveTrader tcTrader))
									{
										tcTrader = new ExclusiveTrader
										{
											faction = parent.Faction,
											factionOptionDef = optionDef,
											traderKindDef = optionDef.taxCollectorTraderKind,
											traderNameKey = optionDef.taxCollectorTraderNameKey
										};
										tcTrader.GenerateThings();
										taxCollectorTraders[optionDef] = tcTrader;
									}
									tcTrader.caravan = caravan;
									Pawn bestNegotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan, faction, optionDef.taxCollectorTraderKind);
									Find.WindowStack.Add(new Dialog_Trade(bestNegotiator, tcTrader));
								},
								order = order
							};
							yield return command_TaxCollector;
							order++;
						}

						if (optionDef.storageIsEnabled)
						{
							Command_Action command_Storage = new Command_Action
							{
								defaultLabel = optionDef.storageLabelKey.Translate(faction.Named("FACTION")),
								defaultDesc = optionDef.storageDescKey.Translate(faction.Named("FACTION")),
								icon = ContentFinder<Texture2D>.Get(optionDef.storageButtonIconTexPath),
								action = delegate
								{
									DiaNode dianode = new DiaNode("Storage");
									Dialog_FactionStorage storageWindow = new Dialog_FactionStorage(dianode, false, caravan, worldTracker.factionOptionsContainer[faction].factionsStorages[optionDef]);
									Find.WindowStack.Add(storageWindow);
								},
								order = order
							};
							yield return command_Storage;
							order++;
						}
						if (optionDef.mechSerumIsEnabled)
						{
							Command_Action command_MechSerum = new Command_Action
							{
								defaultLabel = optionDef.mechSerumLabelKey.Translate(faction.Named("FACTION")),
								defaultDesc = optionDef.mechSerumDescKey.Translate(faction.Named("FACTION")),
								icon = ContentFinder<Texture2D>.Get(optionDef.mechSerumButtonIconTexPath),
								action = delegate
								{
									ExtractMoneyFromCaravan(caravan, optionDef.mechSerumCost, optionDef);
									foreach (Pawn pawn in caravan.PawnsListForReading)
									{
										List<BodyPartRecord> list = (from x in pawn.RaceProps.body.AllParts

											where pawn.health.hediffSet.PartIsMissing(x)

											select x).ToList<BodyPartRecord>();

										foreach (BodyPartRecord missingPart in list)
										{
											pawn.health.RestorePart(missingPart, null, true);
										}
										for (int num = pawn.health.hediffSet.hediffs.Count - 1; num >= 0; num--)
										{
											Hediff hediff = pawn.health.hediffSet.hediffs[num];
											HediffComp_GetsPermanent comp = hediff.TryGetComp<HediffComp_GetsPermanent>();
											if (comp != null && comp.IsPermanent)
											{
												pawn.health.hediffSet.hediffs.RemoveAt(num);
											}
											else if (hediff.def.isBad)
											{
												pawn.health.hediffSet.hediffs.RemoveAt(num);
											}
										}
										if (pawn.Downed)
										{
											pawn.health.MakeUndowned(null);
										}
										pawn.health.hediffSet.DirtyCache();
									}
								}
							};
							ThingDef mechSerumCurrency = optionDef.currencyDef ?? ThingDefOf.Silver;
							if (caravan.AllThings.Where(x => x.def == mechSerumCurrency).Sum(x => x.stackCount) < optionDef.mechSerumCost)
							{
								command_MechSerum.Disable(optionDef.mechSerumCostRequirementKey.Translate());
							}
							command_MechSerum.order = order;
							yield return command_MechSerum;
							order++;
						}
						if (optionDef.workOptions != null)
						{
							foreach (WorkOption workOption in optionDef.workOptions)
							{
								CaravanOptions caravanOptions = GetCaravanOptions(caravan);
								Command_Toggle command_Training = new Command_Toggle
								{
									hotKey = KeyBindingDefOf.Misc1,
									isActive = () => caravanOptions.curWorkOption == workOption,
									toggleAction = delegate
									{
										if (caravanOptions.curWorkOption == workOption)
										{
											caravanOptions.Reset();
										}
										else
										{
											caravanOptions.curWorkOption = workOption;
											caravanOptions.curEnlistOptionInd = OptionsDefs.IndexOf(optionDef);
											caravanOptions.curWorkOptionInd = optionDef.workOptions.IndexOf(workOption);
											if (caravan.pather.Moving)
											{
												caravan.pather.Paused = true;
											}
										}
									},
									defaultLabel = workOption.workLabelKey.Translate(faction.Named("FACTION")),
									defaultDesc = workOption.workDescKey.Translate(faction.Named("FACTION")),
									icon = ContentFinder<Texture2D>.Get(workOption.workButtonIconTexPath),
									order = order
								};
								yield return command_Training;
								order++;
							}
						}

						if (optionDef.autoFeedIsEnabled)
						{
							CaravanOptions caravanAutoFeedOptions = GetCaravanOptions(caravan);
							Command_Toggle command_AutoFeed = new Command_Toggle
							{
								isActive = () => caravanAutoFeedOptions.autoFeedEnabled,
								toggleAction = delegate
								{
									caravanAutoFeedOptions.autoFeedEnabled = !caravanAutoFeedOptions.autoFeedEnabled;
								},
								defaultLabel = optionDef.autoFeedLabelKey.Translate(faction.Named("FACTION")),
								defaultDesc = optionDef.autoFeedDescKey.Translate(faction.Named("FACTION")),
								icon = ContentFinder<Texture2D>.Get(optionDef.autoFeedButtonIconTexPath),
								order = order
							};
							yield return command_AutoFeed;
							order++;
						}

						if (optionDef.turnInIsEnabled)
						{
							Command_Action command_TurnIn = new Command_Action
							{
								defaultLabel = optionDef.turnInLabelKey.Translate(faction.Named("FACTION")),
								defaultDesc = optionDef.turnInDescKey.Translate(faction.Named("FACTION")),
								icon = ContentFinder<Texture2D>.Get(optionDef.turnInButtonIconTexPath),
								action = delegate
								{
									pawnTraders ??= new Dictionary<FactionEnlistOptionsDef, PawnTrader>();
									if (!pawnTraders.TryGetValue(optionDef, out PawnTrader pawnTrader))
									{
										pawnTrader = new PawnTrader
										{
											faction = parent.Faction,
											factionOptionDef = optionDef
										};
										pawnTrader.GenerateThings();
										pawnTraders[optionDef] = pawnTrader;
									}
									pawnTrader.caravan = caravan;
									Pawn bestPlayerNegotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan, faction, optionDef.turnInTraderKind);
									Find.WindowStack.Add(new Dialog_Trade(bestPlayerNegotiator, pawnTrader));
								},
								order = order
							};
							yield return command_TurnIn;
							order++;
						}
						if (optionDef.dropPodServiceIsEnabled)
						{
							Command_Action command_DropPodService = new Command_Action
							{
								defaultLabel = optionDef.dropPodServiceLabelKey.Translate(faction.Named("FACTION")),
								defaultDesc = optionDef.dropPodServiceDescKey.Translate(faction.Named("FACTION")),
								icon = ContentFinder<Texture2D>.Get(optionDef.dropPodServiceButtonIconTexPath),
								action = delegate
								{
									StartChoosingDestination(caravan, optionDef);
								},

								alsoClickIfOtherInGroupClicked = false
							};
							ThingDef dropPodCurrency = optionDef.currencyDef ?? ThingDefOf.Silver;
							if (caravan.AllThings.Where(x => x.def == dropPodCurrency).Sum(x => x.stackCount) < optionDef.dropPodServiceCost)
							{
								command_DropPodService.Disable(optionDef.dropPodServiceCostRequirementKey.Translate());
							}

							command_DropPodService.order = order;
							yield return command_DropPodService;
							order++;
						}
						if (optionDef.shuttleServiceIsEnabled)
						{
							Command_Action command_ShuttleService = new Command_Action
							{
								defaultLabel = optionDef.shuttleServiceLabelKey.Translate(faction.Named("FACTION")),
								defaultDesc = optionDef.shuttleServiceDescKey.Translate(faction.Named("FACTION")),
								icon = ContentFinder<Texture2D>.Get(optionDef.shuttleServiceButtonIconTexPath, reportFailure: false) ?? BaseContent.BadTex,
								action = delegate
								{
									StartChoosingShuttleDestination(caravan, optionDef);
								},

								alsoClickIfOtherInGroupClicked = false
							};
							ThingDef shuttleCurrency = optionDef.currencyDef ?? ThingDefOf.Silver;
							if (caravan.AllThings.Where(x => x.def == shuttleCurrency).Sum(x => x.stackCount) < optionDef.shuttleServiceCost)
							{
								command_ShuttleService.Disable(optionDef.shuttleServiceCostRequirementKey.Translate());
							}

							command_ShuttleService.order = order;
							yield return command_ShuttleService;
							order++;
						}
						if (promotedByOptions is null)
						{
							promotedByOptions ??= new Dictionary<FactionEnlistOptionsDef, bool>();
						}
						if (promotedByOptions.ContainsKey(optionDef) is false)
						{
							promotedByOptions[optionDef] = false;
						}
						var promoted = promotedByOptions[optionDef];
						if (promoted is false && optionDef.promoteOptionEnabled)
						{
							Command_Action command_Promote = new Command_Action
							{
								defaultLabel = optionDef.promoteButtonLabelKey.Translate(faction.Named("FACTION")),
								defaultDesc = optionDef.promoteButtonDescKey.Translate(faction.Named("FACTION")),
								icon = ContentFinder<Texture2D>.Get(optionDef.promoteButtonIconTexPath),
								action = delegate
								{
									promotedByOptions[optionDef] = true;
								},
								order = order
							};
							if (optionDef.promoteSkillRequirements.NullOrEmpty() is false && caravan.PawnsListForReading
								    .Where(x => x.IsColonist && EnlistUtils.PawnSatisfiesSkillRequirements(x, optionDef.promoteSkillRequirements)).Any() is false)
							{
								command_Promote.Disable(optionDef.promoteRequrementsNotSatisfiedKey
									.Translate(optionDef.promoteSkillRequirements.Select((SkillRequirement x) => x.Summary).ToCommaList()));
							}
							yield return command_Promote;
							order++;
						}
						if (promoted)
						{
							if (optionDef.promoteProvisionOptions != null)
							{
								promotedProvisionInfos ??= new Dictionary<int, ProvisionsInfo>();
								foreach (var button in GetProvisionButtons(promotedProvisionInfos, optionDef.promoteProvisionOptions, faction, caravan, order))
								{
									yield return button;
									order++;
								}
							}
						}
						if (optionDef.protocolOptions.NullOrEmpty() is false)
						{
							yield return new Command_Action
							{
								defaultLabel = optionDef.protocolButtonLabelKey.Translate(),
								defaultDesc = optionDef.protocolButtonDescKey.Translate(),
								icon = ContentFinder<Texture2D>.Get(optionDef.protocolButtonIconTexPath),
								action = delegate
								{
									var dict = optionDef.protocolOptions.ToDictionary(x => x.protocolHashKey, x => x.action);
									Find.WindowStack.Add(new Window_Password(dict, optionDef.protocolEnterText, optionDef.protocolInvalidWarning));
								},
								order = order
							};
							order++;
						}


						foreach (var gizmo in optionDef.Worker.GetGizmos(parent.Faction, order))
						{
							yield return gizmo;
							order++;

						}

						if (optionDef.abilityTrainingIsEnabled && !optionDef.abilityTrainingOptions.NullOrEmpty())
						{
							activeTrainingSessions ??= new Dictionary<FactionEnlistOptionsDef, AbilityTrainingSession>();
							Command_Action command_Training = new Command_Action
							{
								defaultLabel = optionDef.abilityTrainingLabelKey.Translate(faction.Named("FACTION")),
								defaultDesc = optionDef.abilityTrainingDescKey.Translate(faction.Named("FACTION")),
								icon = ContentFinder<Texture2D>.Get(optionDef.abilityTrainingButtonIconTexPath),
								action = delegate
								{
									List<FloatMenuOption> trainingOptions = new List<FloatMenuOption>();
									for (int idx = 0; idx < optionDef.abilityTrainingOptions.Count; idx++)
									{
										AbilityTrainingOption trainingOpt = optionDef.abilityTrainingOptions[idx];
										int capturedIdx = idx;
										trainingOptions.Add(new FloatMenuOption(trainingOpt.labelKey.Translate(), delegate
										{
											ThingDef currency = optionDef.currencyDef ?? ThingDefOf.Silver;
											int available = caravan.AllThings.Where(t => t.def == currency).Sum(t => t.stackCount);
											if (available < trainingOpt.cost)
											{
												Messages.Message("FCP_TrainingNotEnoughFunds".Translate(), MessageTypeDefOf.RejectInput, historical: false);
												return;
											}
											List<FloatMenuOption> pawnOptions = new List<FloatMenuOption>();
											foreach (Pawn candidate in caravan.PawnsListForReading.Where(p => p.IsColonist && !p.IsPrisoner))
											{
												Pawn localPawn = candidate;
												pawnOptions.Add(new FloatMenuOption(localPawn.LabelShort, delegate
												{
													ExtractMoneyFromCaravan(caravan, trainingOpt.cost, optionDef);
													activeTrainingSessions[optionDef] = new AbilityTrainingSession
													{
														trainee = localPawn,
														startTick = Find.TickManager.TicksGame,
														durationTicks = trainingOpt.trainingDurationDays * GenDate.TicksPerDay,
														enlistOptionDef = optionDef,
														trainingOptionIndex = capturedIdx
													};
												}, MenuOptionPriority.Default, null, localPawn));
											}
											Find.WindowStack.Add(new FloatMenu(pawnOptions));
										}));
									}
									Find.WindowStack.Add(new FloatMenu(trainingOptions));
								},
								order = order
							};
							if (activeTrainingSessions.ContainsKey(optionDef))
							{
								AbilityTrainingSession running = activeTrainingSessions[optionDef];
								command_Training.Disable("FCP_TrainingAlreadyActive".Translate(running.trainee.Named("PAWN")));
							}
							yield return command_Training;
							order++;
						}

						if (optionDef.deliveryQuestsIsEnabled && !optionDef.deliveryQuestTemplates.NullOrEmpty())
						{
							deliveryBoards ??= new Dictionary<FactionEnlistOptionsDef, DeliveryQuestList>();
							if (!deliveryBoards.ContainsKey(optionDef))
								deliveryBoards[optionDef] = new DeliveryQuestList();

							Command_Action command_DeliveryBoard = new Command_Action
							{
								defaultLabel = optionDef.deliveryQuestsBoardLabelKey.Translate(faction.Named("FACTION")),
								defaultDesc = optionDef.deliveryQuestsBoardDescKey.Translate(faction.Named("FACTION")),
								icon = ContentFinder<Texture2D>.Get(optionDef.deliveryQuestsBoardButtonIconTexPath),
								action = delegate
								{
									DeliveryQuestList board = deliveryBoards[optionDef];
									bool needsRefresh = board.lastRefreshTick == 0 || Find.TickManager.TicksGame > board.lastRefreshTick + (optionDef.deliveryQuestsRerollDays * GenDate.TicksPerDay);
									if (needsRefresh)
										RefreshDeliveryBoard(optionDef);
									board.quests.RemoveAll(q => q.IsExpired || q.accepted);
									if (!board.quests.Any())
									{
										Messages.Message("FCP_NoDeliveryQuestsAvailable".Translate(), MessageTypeDefOf.RejectInput, historical: false);
										return;
									}
									List<FloatMenuOption> questOptions = new List<FloatMenuOption>();
									foreach (DeliveryQuest quest in board.quests)
									{
										DeliveryQuest localQuest = quest;
										Settlement dest = Find.WorldObjects.AllWorldObjects.OfType<Settlement>().FirstOrDefault(s => s.Tile == localQuest.destinationTile);
										string destName = dest?.LabelShort ?? localQuest.destinationTile.ToString();
										string stuffPart = localQuest.stuff != null ? " (" + localQuest.stuff.label + ")" : "";
										ThingDef rewardItem = localQuest.rewardDef ?? optionDef.salaryDef ?? ThingDefOf.Silver;
										string label = $"{localQuest.count}x {localQuest.thingToDeliver.label}{stuffPart} → {destName}: {localQuest.reward} {rewardItem.label}";
										questOptions.Add(new FloatMenuOption(label, delegate
										{
											localQuest.accepted = true;
											FactionOptions factionOpts = worldTracker.factionOptionsContainer[faction];
											factionOpts.activeDeliveries ??= new Dictionary<FactionEnlistOptionsDef, DeliveryQuestList>();
											if (!factionOpts.activeDeliveries.ContainsKey(optionDef))
												factionOpts.activeDeliveries[optionDef] = new DeliveryQuestList();
											factionOpts.activeDeliveries[optionDef].quests.Add(localQuest);
										}));
									}
									Find.WindowStack.Add(new FloatMenu(questOptions));
								},
								order = order
							};
							yield return command_DeliveryBoard;
							order++;

							if (worldTracker.factionOptionsContainer.TryGetValue(faction, out FactionOptions factionOptCheck) &&
								factionOptCheck.activeDeliveries != null &&
								factionOptCheck.activeDeliveries.TryGetValue(optionDef, out DeliveryQuestList activeList))
							{
								DeliveryQuest turnInQuest = activeList.quests.FirstOrDefault(q => q.destinationTile == parent.Tile && !q.IsExpired);
								if (turnInQuest != null)
								{
									DeliveryQuest localTurnIn = turnInQuest;
									string stuffPart = localTurnIn.stuff != null ? " (" + localTurnIn.stuff.label + ")" : "";
									Command_Action command_TurnIn = new Command_Action
									{
										defaultLabel = optionDef.deliveryQuestsTurnInLabelKey.Translate(faction.Named("FACTION")),
										defaultDesc = optionDef.deliveryQuestsTurnInDescKey.Translate(
											localTurnIn.count.Named("COUNT"),
											localTurnIn.thingToDeliver.Named("THING")),
										icon = ContentFinder<Texture2D>.Get(optionDef.deliveryQuestsTurnInButtonIconTexPath),
										action = delegate
										{
											localTurnIn.TurnIn(caravan, optionDef);
											activeList.quests.Remove(localTurnIn);
										},
										order = order
									};
									if (!localTurnIn.CaravanCanTurnIn(caravan))
									{
										command_TurnIn.Disable("FCP_DeliveryQuestNotEnoughItems".Translate(
											localTurnIn.count.Named("COUNT"),
											localTurnIn.thingToDeliver.Named("THING")));
									}
									yield return command_TurnIn;
									order++;
								}
							}
						}

						Command_Action command_Resign = new Command_Action
						{
							defaultLabel = optionDef.resignButtonLabelKey.Translate(faction.Named("FACTION")),
							defaultDesc = optionDef.resignButtonDescKey.Translate(faction.Named("FACTION")),
							icon = ContentFinder<Texture2D>.Get(optionDef.resignButtonIconTexPath),
							action = delegate
							{
								DiaNode dianode = new DiaNode("Resign");
								Dialog_ResignConfirmation resignWindow = new Dialog_ResignConfirmation(dianode, false, this, optionDef);
								Find.WindowStack.Add(resignWindow);
							},
							order = order
						};
						yield return command_Resign;
					}
				}
			}
		}
		yield break;
	}

	public void RefreshDeliveryBoard(FactionEnlistOptionsDef optionDef)
	{
		deliveryBoards ??= new Dictionary<FactionEnlistOptionsDef, DeliveryQuestList>();
		if (!deliveryBoards.ContainsKey(optionDef))
			deliveryBoards[optionDef] = new DeliveryQuestList();

		DeliveryQuestList board = deliveryBoards[optionDef];
		board.quests.Clear();

		if (optionDef.deliveryQuestTemplates.NullOrEmpty()) return;

		Faction faction = parent.Faction;
		List<Settlement> destinations = Find.WorldObjects.AllWorldObjects
			.OfType<Settlement>()
			.Where(s => s.Faction == faction && s.Tile != parent.Tile)
			.ToList();
		if (destinations.NullOrEmpty())
			destinations = Find.WorldObjects.AllWorldObjects
				.OfType<Settlement>()
				.Where(s => s.Faction != null && !s.Faction.HostileTo(Faction.OfPlayer) && s.Tile != parent.Tile)
				.ToList();
		if (destinations.NullOrEmpty()) return;

		foreach (DeliveryQuestTemplate template in optionDef.deliveryQuestTemplates)
		{
			Settlement dest = destinations.RandomElement();
			board.quests.Add(new DeliveryQuest
			{
				thingToDeliver = template.thingToDeliver,
				stuff = template.stuff,
				count = template.countRange.RandomInRange,
				reward = template.rewardRange.RandomInRange,
				rewardDef = template.rewardDef,
				destinationLabel = dest.LabelCap,
				sourceTile = parent.Tile,
				destinationTile = dest.Tile,
				createdTick = Find.TickManager.TicksGame,
				durationTicks = template.durationDays * GenDate.TicksPerDay
			});
		}
		board.lastRefreshTick = Find.TickManager.TicksGame;
	}

	public IEnumerable<Command_Action> GetProvisionButtons(Dictionary<int, ProvisionsInfo> provisionInfos, List<ProvisionOption> provisionOptions, Faction faction, Caravan caravan, int order)	{
		for (int i = 0; i < provisionOptions.Count; i++)
		{
			ProvisionOption provisionOption = provisionOptions[i];
			if (!provisionInfos.TryGetValue(i, out ProvisionsInfo provisionInfo))
			{
				provisionInfos[i] = provisionInfo = new ProvisionsInfo();
			}

			Command_Action command_Provisions = new Command_Action
			{
				defaultLabel = provisionOption.provisionsLabelKey.Translate(faction.Named("FACTION")),
				defaultDesc = provisionOption.provisionsDescKey.Translate(faction.Named("FACTION")),
				icon = ContentFinder<Texture2D>.Get(provisionOption.provisionsButtonIconTexPath),
				action = delegate
				{
					provisionInfo.GiveProvisions(provisionOption, caravan);
				},
				order = order
			};
			if (!provisionInfo.CanGiveProvisions(provisionOption))
			{
				command_Provisions.Disable();
			}
			yield return command_Provisions;
			order++;
		}
	}
	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref generatedQuestsLastTick, "generatedQuestsLastTick");
		Scribe_Collections.Look(ref generatedQuests, "generatedQuests", LookMode.Deep);
		if (caravanOptions != null)
		{
			caravanOptions.RemoveAll(x => x.Key is null);
		}
		Scribe_Collections.Look(ref caravanOptions, "caravanOptions", LookMode.Reference, LookMode.Deep, ref caravanKeys, ref caravanOptionsValues);
		Scribe_Collections.Look(ref pawnTraders, "pawnTraders", LookMode.Def, LookMode.Deep);
		Scribe_Collections.Look(ref bountyHunterTraders, "bountyHunterTraders", LookMode.Def, LookMode.Deep);
		Scribe_Collections.Look(ref exclusiveTraders, "exclusiveTraders", LookMode.Def, LookMode.Deep);
		Scribe_Collections.Look(ref taxCollectorTraders, "taxCollectorTraders", LookMode.Def, LookMode.Deep);
		Scribe_Collections.Look(ref provisionInfos, "provisionInfos", LookMode.Value, LookMode.Deep);
		Scribe_Collections.Look(ref promotedProvisionInfos, "promotedProvisionInfos", LookMode.Value, LookMode.Deep);
		Scribe_Collections.Look(ref promotedByOptions, "promotedByOptions", LookMode.Def, LookMode.Value);
		Scribe_Collections.Look(ref activeTrainingSessions, "activeTrainingSessions", LookMode.Def, LookMode.Deep);
		Scribe_Collections.Look(ref deliveryBoards, "deliveryBoards", LookMode.Def, LookMode.Deep);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			generatedQuests ??= new List<Quest>();
			promotedByOptions ??= new Dictionary<FactionEnlistOptionsDef, bool>();
		}
	}
	private List<Caravan> caravanKeys;
	private List<CaravanOptions> caravanOptionsValues;

	private List<FactionEnlistOptionsDef> defKeys;
	private List<PawnTrader> pawnTraderValues;

	private List<int> provisionKeys;
	private List<ProvisionsInfo> provisionValues;
	private int MaxLaunchDistance => 100;
	public bool CanTryLaunch => true;

	private List<FactionEnlistOptionsDef> defKeys2;
	private List<bool> boolValues;
	public void StartChoosingDestination(Caravan caravan, FactionEnlistOptionsDef optionsDef)
	{
		tmpCaravan = caravan;
		CameraJumper.TryJump(CameraJumper.GetWorldTarget(parent));
		Find.WorldSelector.ClearSelection();
		int tile = parent.Tile;
		curFactionEnlistOptionsDef = optionsDef;
		Find.WorldTargeter.BeginTargeting(ChoseWorldTarget, canTargetTiles: true, CompLaunchable.TargeterMouseAttachment, closeWorldTabWhenFinished: false, delegate
		{
			GenDraw.DrawWorldRadiusRing(tile, MaxLaunchDistance);
		}, (GlobalTargetInfo target) => TargetingLabelGetter(target, tile, MaxLaunchDistance, caravan));
	}

	private bool ChoseWorldTarget(GlobalTargetInfo target)
	{
		return ChoseWorldTarget(target, parent.Tile, MaxLaunchDistance, TryLaunch, tmpCaravan);
	}
	public bool ChoseWorldTarget(GlobalTargetInfo target, int tile, int maxLaunchDistance, Action<int, TransportersArrivalAction, Caravan> launchAction, Caravan caravan)
	{
		if (!target.IsValid)
		{
			Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput, historical: false);
			return false;
		}
		if (Find.WorldGrid.TraversalDistanceBetween(tile, target.Tile) > maxLaunchDistance)
		{
			Messages.Message("TransportPodDestinationBeyondMaximumRange".Translate(), MessageTypeDefOf.RejectInput, historical: false);
			return false;
		}
		IEnumerable<FloatMenuOption> source = GetTransportPodsFloatMenuOptionsAt(target.Tile, caravan, launchAction);
		if (!source.Any())
		{
			if (Find.World.Impassable(target.Tile))
			{
				Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
			launchAction(target.Tile, null, caravan);
			return true;
		}
		if (source.Count() == 1)
		{
			if (!source.First().Disabled)
			{
				source.First().action();
				return true;
			}
			return false;
		}
		Find.WindowStack.Add(new FloatMenu(source.ToList()));
		return false;
	}

	private FactionEnlistOptionsDef curFactionEnlistOptionsDef;
	public void TryLaunch(int destinationTile, TransportersArrivalAction arrivalAction, Caravan caravan)
	{
		int num = Find.WorldGrid.TraversalDistanceBetween(parent.Tile, destinationTile);
		if (num <= MaxLaunchDistance)
		{
			foreach (Pawn pawn in caravan.PawnsListForReading)
			{
				if (pawn.IsColonist && pawn.inventory != null)
				{
					pawn.inventory.unloadEverything = true;
				}
			}
			ExtractMoneyFromCaravan(caravan, curFactionEnlistOptionsDef.dropPodServiceCost, curFactionEnlistOptionsDef);

			ActiveTransporter ActiveTransporter = (ActiveTransporter)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod);
			ActiveTransporter.Contents = new ActiveTransporterInfo();
			ActiveTransporter.Contents.innerContainer.TryAddRangeOrTransfer(caravan.GetDirectlyHeldThings(), canMergeWithExistingStacks: true, destroyLeftover: true);
			FlyShipLeaving obj = (FlyShipLeaving)SkyfallerMaker.MakeSkyfaller(ThingDefOf.DropPodLeaving, ActiveTransporter);
			obj.groupID = 1;
			obj.destinationTile = destinationTile;
			obj.arrivalAction = arrivalAction;
			obj.worldObjectDef = WorldObjectDefOf.TravellingTransporters;

			TravellingTransporters TravellingTransporters = (TravellingTransporters)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.TravellingTransporters);
			TravellingTransporters.Tile = base.parent.Tile;
			TravellingTransporters.SetFaction(Faction.OfPlayer);
			TravellingTransporters.destinationTile = destinationTile;
			TravellingTransporters.arrivalAction = arrivalAction;
			Find.WorldObjects.Add(TravellingTransporters);
			TravellingTransporters.AddTransporter(ActiveTransporter.contents, true);
			caravan.Destroy();
		}
	}

	public void StartChoosingShuttleDestination(Caravan caravan, FactionEnlistOptionsDef optionsDef)
	{
		tmpCaravan = caravan;
		CameraJumper.TryJump(CameraJumper.GetWorldTarget(parent));
		Find.WorldSelector.ClearSelection();
		int tile = parent.Tile;
		curFactionEnlistOptionsDef = optionsDef;
		Find.WorldTargeter.BeginTargeting(ChoseShuttleWorldTarget, canTargetTiles: true, CompLaunchable.TargeterMouseAttachment, closeWorldTabWhenFinished: false, delegate
		{
			GenDraw.DrawWorldRadiusRing(tile, MaxLaunchDistance);
		}, (GlobalTargetInfo target) => TargetingLabelGetter(target, tile, MaxLaunchDistance, caravan));
	}

	private bool ChoseShuttleWorldTarget(GlobalTargetInfo target)
	{
		return ChoseWorldTarget(target, parent.Tile, MaxLaunchDistance, TryLaunchShuttle, tmpCaravan);
	}

	public void TryLaunchShuttle(int destinationTile, TransportersArrivalAction arrivalAction, Caravan caravan)
	{
		int num = Find.WorldGrid.TraversalDistanceBetween(parent.Tile, destinationTile);
		if (num <= MaxLaunchDistance)
		{
			foreach (Pawn pawn in caravan.PawnsListForReading)
			{
				if (pawn.IsColonist && pawn.inventory != null)
				{
					pawn.inventory.unloadEverything = true;
				}
			}
			ExtractMoneyFromCaravan(caravan, curFactionEnlistOptionsDef.shuttleServiceCost, curFactionEnlistOptionsDef);

			// Get custom shuttle from faction extension - direct type access (no reflection)
			FactionModExtension factionExtension = parent.Faction?.def?.GetModExtension<FactionModExtension>();
			TransportShipDef transportShipDef = factionExtension?.transportShipDef;

			if (transportShipDef != null && transportShipDef.worldObject != null)
			{
				// Create the shuttle thing - it's a Building with CompTransporter, not an ActiveTransporter
				Thing shuttleThing = ThingMaker.MakeThing(transportShipDef.shipThing);
				CompTransporter compTransporter = shuttleThing.TryGetComp<CompTransporter>();
				
				if (compTransporter == null)
				{
					Log.Error($"[FCP Enlist] Shuttle thing {shuttleThing.def.defName} has no CompTransporter!");
					TryLaunch(destinationTile, arrivalAction, caravan);
					return;
				}
				
				// Transfer caravan contents to shuttle's transporter
				compTransporter.GetDirectlyHeldThings().TryAddRangeOrTransfer(
					caravan.GetDirectlyHeldThings(), 
					canMergeWithExistingStacks: true, 
					destroyLeftover: true);
				
				// Create the traveling world object - cast to TravellingTransporters for direct property access
				TravellingTransporters travelingShuttle = (TravellingTransporters)WorldObjectMaker.MakeWorldObject(transportShipDef.worldObject);
				travelingShuttle.Tile = parent.Tile;
				travelingShuttle.SetFaction(Faction.OfPlayer);
				travelingShuttle.destinationTile = destinationTile;
				
				// Use custom arrival action that forces map loading to show shuttle landing
			travelingShuttle.arrivalAction = arrivalAction ?? new TransportersArrivalAction_FormCaravan();
				// Set the transport ship def (still need reflection for this private field)
				var transportShipField = typeof(TravellingTransporters).GetField("transportShip", 
					System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				if (transportShipField != null)
				{
					transportShipField.SetValue(travelingShuttle, transportShipDef);
				}
				
				Find.WorldObjects.Add(travelingShuttle);
				
				// Wrap the cargo in ActiveTransporterInfo for AddTransporter
				ActiveTransporterInfo transporterInfo = new ActiveTransporterInfo();
				transporterInfo.innerContainer.TryAddRangeOrTransfer(
					compTransporter.GetDirectlyHeldThings(), 
					canMergeWithExistingStacks: true, 
					destroyLeftover: false);
				
				travelingShuttle.AddTransporter(transporterInfo, true);
				
				caravan.Destroy();
				return;
			}
			
			// Fallback to drop pods
			Log.Warning("[FCP Enlist] Failed to launch shuttle, falling back to drop pods");
			TryLaunch(destinationTile, arrivalAction, caravan);
		}
	}

	public void ExtractMoneyFromCaravan(Caravan caravan, int fee, FactionEnlistOptionsDef optionDef)
	{
		ThingDef currencyDef = optionDef.currencyDef ?? ThingDefOf.Silver;
		while (true)
		{
			if (fee > 0)
			{
				List<Thing> currencies = caravan.AllThings.Where(x => x.def == currencyDef).ToList();
				for (int i = currencies.Count - 1; i >= 0; i--)
				{
					Thing currency = currencies[i];
					if (currency.stackCount > 0)
					{
						int num = Math.Min(fee, currency.stackCount);
						currency.SplitOff(num)?.Destroy();
						fee -= num;
						if (fee <= 0)
						{
							break;
						}
					}
				}
			}
			else
			{
				break;
			}
		}
	}
	private IEnumerable<FloatMenuOption> GetTransportPodsFloatMenuOptionsAt(int tile, Caravan caravan, Action<int, TransportersArrivalAction, Caravan> launchAction = null)
	{
		// Default to TryLaunch if no launchAction specified (for drop pods)
		if (launchAction == null)
			launchAction = TryLaunch;
		
		bool anything = false;
		if (!Find.World.Impassable(tile) && !Find.WorldObjects.AnySettlementBaseAt(tile) && !Find.WorldObjects.AnySiteAt(tile))
		{
			anything = true;
			yield return new FloatMenuOption("FormCaravanHere".Translate(), delegate
			{
				launchAction(tile, new TransportersArrivalAction_FormCaravan(), caravan);
			});
		}
		//List<WorldObject> worldObjects = Find.WorldObjects.AllWorldObjects;
		//for (int i = 0; i < worldObjects.Count; i++)
		//{
		//	if (worldObjects[i].Tile == tile)
		//	{
		//		foreach (FloatMenuOption transportPodsFloatMenuOption in worldObjects[i].GetTransportPodsFloatMenuOptions(TransportersInGroup.Cast<IThingHolder>(), this))
		//		{
		//			anything = true;
		//			yield return transportPodsFloatMenuOption;
		//		}
		//	}
		//}
		if (!anything && !Find.World.Impassable(tile))
		{
			yield return new FloatMenuOption("TransportPodsContentsWillBeLost".Translate(), delegate
			{
				launchAction(tile, null, caravan);
			});
		}
	}
	public string TargetingLabelGetter(GlobalTargetInfo target, int tile, int maxLaunchDistance, Caravan caravan)
	{
		if (!target.IsValid)
		{
			return null;
		}
		if (Find.WorldGrid.TraversalDistanceBetween(tile, target.Tile) > maxLaunchDistance)
		{
			GUI.color = ColorLibrary.RedReadable;
			return "TransportPodDestinationBeyondMaximumRange".Translate();
		}
		IEnumerable<FloatMenuOption> source = GetTransportPodsFloatMenuOptionsAt(target.Tile, caravan);
		if (!source.Any())
		{
			return string.Empty;
		}
		if (source.Count() == 1)
		{
			if (source.First().Disabled)
			{
				GUI.color = ColorLibrary.RedReadable;
			}
			return source.First().Label;
		}
		return target.WorldObject is MapParent mapParent
			? (string)"ClickToSeeAvailableOrders_WorldObject".Translate(mapParent.LabelCap)
			: (string)"ClickToSeeAvailableOrders_Empty".Translate();
	}
}
