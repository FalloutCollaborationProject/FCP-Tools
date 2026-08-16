using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using HarmonyLib;
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
		caravanOptions = new Dictionary<Caravan, CaravanOptions>();
		pawnTraders = new Dictionary<FactionEnlistOptionsDef, PawnTrader>();
	}

	public Dictionary<FactionEnlistOptionsDef, BountyBoard> bountyBoards;
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

	public bool IsPromoted(FactionEnlistOptionsDef optionDef)
	{
		return promotedByOptions != null && promotedByOptions.TryGetValue(optionDef, out bool promoted) && promoted;
	}

	public void SetPromoted(FactionEnlistOptionsDef optionDef, bool promoted)
	{
		promotedByOptions ??= new Dictionary<FactionEnlistOptionsDef, bool>();
		promotedByOptions[optionDef] = promoted;
	}

	public PawnTrader GetOrMakeBountyHunterTrader(FactionEnlistOptionsDef optionDef)
	{
		bountyHunterTraders ??= new Dictionary<FactionEnlistOptionsDef, PawnTrader>();
		if (!bountyHunterTraders.TryGetValue(optionDef, out PawnTrader trader))
		{
			trader = new PawnTrader
			{
				faction = parent.Faction,
				factionOptionDef = optionDef,
				isBountyHunter = true,
				refreshDays = optionDef.bountyHunterRefreshSilverInDays
			};
			trader.GenerateThings();
			bountyHunterTraders[optionDef] = trader;
		}
		return trader;
	}

	public PawnTrader GetOrMakeTurnInTrader(FactionEnlistOptionsDef optionDef)
	{
		pawnTraders ??= new Dictionary<FactionEnlistOptionsDef, PawnTrader>();
		if (!pawnTraders.TryGetValue(optionDef, out PawnTrader trader))
		{
			trader = new PawnTrader
			{
				faction = parent.Faction,
				factionOptionDef = optionDef
			};
			trader.GenerateThings();
			pawnTraders[optionDef] = trader;
		}
		return trader;
	}

	public ExclusiveTrader GetOrMakeExclusiveTrader(FactionEnlistOptionsDef optionDef)
	{
		exclusiveTraders ??= new Dictionary<FactionEnlistOptionsDef, ExclusiveTrader>();
		if (!exclusiveTraders.TryGetValue(optionDef, out ExclusiveTrader trader))
		{
			trader = new ExclusiveTrader
			{
				faction = parent.Faction,
				factionOptionDef = optionDef
			};
			trader.GenerateThings();
			exclusiveTraders[optionDef] = trader;
		}
		return trader;
	}

	public ExclusiveTrader GetOrMakeTaxCollectorTrader(FactionEnlistOptionsDef optionDef)
	{
		taxCollectorTraders ??= new Dictionary<FactionEnlistOptionsDef, ExclusiveTrader>();
		if (!taxCollectorTraders.TryGetValue(optionDef, out ExclusiveTrader trader))
		{
			trader = new ExclusiveTrader
			{
				faction = parent.Faction,
				factionOptionDef = optionDef,
				traderKindDef = optionDef.taxCollectorTraderKind,
				traderNameKey = optionDef.taxCollectorTraderNameKey
			};
			trader.GenerateThings();
			taxCollectorTraders[optionDef] = trader;
		}
		return trader;
	}

	public void UseMechSerum(Caravan caravan, FactionEnlistOptionsDef optionDef)
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
				Traverse.Create(pawn.health).Method("MakeUndowned", new Type[] { typeof(Hediff) }, new object[] { null }).GetValue();
			}
			pawn.health.hediffSet.DirtyCache();
		}
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
						if (caravan.pather.Moving || caravan.Destroyed || caravan.Tile != parent.Tile)
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
										Traverse.Create(pawn.needs.joy).Field("lastGainTick").SetValue(Find.TickManager.TicksGame);
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
	public BountyBoard GetBountyBoard(FactionEnlistOptionsDef optionsDef)
	{
		bountyBoards ??= new Dictionary<FactionEnlistOptionsDef, BountyBoard>();
		if (!bountyBoards.TryGetValue(optionsDef, out BountyBoard board))
		{
			board = bountyBoards[optionsDef] = new BountyBoard();
		}
		return board;
	}

	public void AcceptBounty(GeneratedBounty bounty, FactionEnlistOptionsDef optionsDef)
	{
		BountyBoard board = GetBountyBoard(optionsDef);
		if (!board.available.Remove(bounty))
			return;

		Find.QuestManager.Add(bounty.quest);
		board.accepted.Add(bounty);
	}

	public void CollectBounty(GeneratedBounty bounty, FactionEnlistOptionsDef optionsDef, Caravan caravan)
	{
		BountyBoard board = GetBountyBoard(optionsDef);
		if (!board.accepted.Remove(bounty))
			return;

		Thing payment = ThingMaker.MakeThing(optionsDef.currencyDef ?? ThingDefOf.Silver);
		payment.stackCount = bounty.reward;
		CaravanInventoryUtility.GiveThing(caravan, payment);
	}

	public void RefreshBountyBoard(FactionEnlistOptionsDef optionsDef)
	{
		BountyBoard board = GetBountyBoard(optionsDef);
		board.accepted.RemoveAll(bounty => bounty.Failed);
		board.available.Clear();

		Patch_TryFindTile.worldObject = parent;
		int questCountToGenerate = Rand.RangeInclusive(15, 20);
		float points = StorytellerUtility.DefaultThreatPointsNow(Find.World);
		List<QuestScriptDef> questDefsToProcess = DefDatabase<QuestScriptDef>.AllDefs.Where(x => !x.isRootSpecial && x.IsRootAny).ToList();

		while (board.available.Count < questCountToGenerate)
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
					board.available.Add(new GeneratedBounty { quest = quest, reward = optionsDef.missionsBountyRewardRange.RandomInRange });
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex + " can't generate " + newQuestCandidate);
			}
		}

		Patch_TryFindTile.worldObject = null;
		board.lastRefreshTick = Find.TickManager.TicksGame;
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
					bool enlisted = worldTracker.EnlistedTo(faction, optionDef);
					string label = enlisted
						? "FCP_Enlist_OpenTerminal".Translate(faction.Named("FACTION"))
						: optionDef.enlistButtonLabelKey.Translate(faction.Named("FACTION"));
					string desc = enlisted
						? "FCP_Enlist_OpenTerminalDesc".Translate(faction.Named("FACTION"))
						: optionDef.enlistButtonDescKey.Translate(faction.Named("FACTION"));

					Command_Action command_Terminal = new Command_Action
					{
						defaultLabel = label,
						defaultDesc = desc,
						icon = ContentFinder<Texture2D>.Get(optionDef.enlistButtonIconTexPath),
						action = delegate
						{
							Find.WindowStack.Add(new Window_EnlistTerminal(caravan, this, optionDef));
						},
						Order = order
					};
					yield return command_Terminal;
					order++;

					foreach (var gizmo in optionDef.Worker.GetGizmos(parent.Faction, order))
					{
						yield return gizmo;
						order++;
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

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Collections.Look(ref bountyBoards, "bountyBoards", LookMode.Def, LookMode.Deep);
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
					pawn.inventory.UnloadEverything = true;
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
			TravellingTransporters.AddTransporter(ActiveTransporter.Contents, true);
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
					pawn.inventory.UnloadEverything = true;
				}
			}
			ExtractMoneyFromCaravan(caravan, curFactionEnlistOptionsDef.shuttleServiceCost, curFactionEnlistOptionsDef);

			FactionModExtension factionExtension = parent.Faction?.def?.GetModExtension<FactionModExtension>();
			TransportShipDef transportShipDef = factionExtension?.transportShipDef;

			if (transportShipDef != null && transportShipDef.worldObject != null)
			{
				Thing shuttleThing = ThingMaker.MakeThing(transportShipDef.shipThing);
				CompTransporter compTransporter = shuttleThing.TryGetComp<CompTransporter>();
				
				if (compTransporter == null)
				{
					Log.Error($"[FCP Enlist] Shuttle thing {shuttleThing.def.defName} has no CompTransporter!");
					TryLaunch(destinationTile, arrivalAction, caravan);
					return;
				}
				
				compTransporter.GetDirectlyHeldThings().TryAddRangeOrTransfer(
					caravan.GetDirectlyHeldThings(), 
					canMergeWithExistingStacks: true, 
					destroyLeftover: true);
				
				TravellingTransporters travelingShuttle = (TravellingTransporters)WorldObjectMaker.MakeWorldObject(transportShipDef.worldObject);
				travelingShuttle.Tile = parent.Tile;
				travelingShuttle.SetFaction(Faction.OfPlayer);
				travelingShuttle.destinationTile = destinationTile;
			travelingShuttle.arrivalAction = arrivalAction ?? new TransportersArrivalAction_FormCaravan();
				var transportShipField = typeof(TravellingTransporters).GetField("transportShip", 
					System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				if (transportShipField != null)
				{
					transportShipField.SetValue(travelingShuttle, transportShipDef);
				}
				
				Find.WorldObjects.Add(travelingShuttle);
				
				ActiveTransporterInfo transporterInfo = new ActiveTransporterInfo();
				transporterInfo.innerContainer.TryAddRangeOrTransfer(
					compTransporter.GetDirectlyHeldThings(), 
					canMergeWithExistingStacks: true, 
					destroyLeftover: false);
				
				travelingShuttle.AddTransporter(transporterInfo, true);
				
				caravan.Destroy();
				return;
			}
			
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
