using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FCP.Core.PowerArmor;

public class JobDriver_RepairPowerArmor : JobDriver
{
	private const TargetIndex StationInd = TargetIndex.A;
	private const TargetIndex IngredientInd = TargetIndex.B;
	private const TargetIndex IngredientPlaceCellInd = TargetIndex.C;

	private Building Station => (Building)job.GetTarget(TargetIndex.A).Thing;
	private CompPowerArmorStation StationComp => Station.GetComp<CompPowerArmorStation>();

	private float ticksToNextRepair;
	private float repairTimeCostPerHP;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (!pawn.Reserve(Station, job, 1, -1, null, errorOnFailed))
		{
			return false;
		}
		pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.B), job);
		return true;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref ticksToNextRepair, "ticksToNextRepair", 0f);
		Scribe_Values.Look(ref repairTimeCostPerHP, "repairTimeCostPerHP", 60f);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedNullOrForbidden(StationInd);
		Toil setupCell = ToilMaker.MakeToil("SetupCell");
		setupCell.initAction = delegate
		{
			Pawn actor = setupCell.actor;
			Building station = (Building)actor.jobs.curJob.GetTarget(StationInd).Thing;
			actor.jobs.curJob.SetTarget(IngredientPlaceCellInd, station.InteractionCell);
		};
		setupCell.defaultCompleteMode = ToilCompleteMode.Instant;
		yield return setupCell;
		Toil extract = Toils_JobTransforms.ExtractNextTargetFromQueue(IngredientInd);
		yield return extract;
		yield return Toils_Goto.GotoThing(IngredientInd, PathEndMode.ClosestTouch)
			.FailOnDespawnedNullOrForbidden(IngredientInd)
			.FailOnSomeonePhysicallyInteracting(IngredientInd);
		yield return Toils_Haul.StartCarryThing(IngredientInd,
			putRemainderInQueue: true,
			subtractNumTakenFromJobCount: false,
			failIfStackCountLessThanJobCount: true);
		yield return Toils_Goto.GotoCell(IngredientPlaceCellInd, PathEndMode.Touch);
		Toil place = ToilMaker.MakeToil("PlaceIngredient");
		place.initAction = delegate
		{
			Pawn actor = place.actor;
			Job curJob = actor.jobs.curJob;
			IntVec3 cell = curJob.GetTarget(IngredientPlaceCellInd).Cell;
			Thing carriedThing = actor.carryTracker.CarriedThing;

			if (carriedThing != null)
			{
				Action<Thing, int> onPlaced = (th, added) =>
				{
					HaulAIUtility.UpdateJobWithPlacedThings(curJob, th, added);
				};
				if (!actor.carryTracker.TryDropCarriedThing(cell, ThingPlaceMode.Near, out var _, onPlaced))
				{
					actor.jobs.EndCurrentJob(JobCondition.Incompletable);
				}
			}
		};
		place.defaultCompleteMode = ToilCompleteMode.Instant;
		yield return place;
		yield return Toils_Jump.JumpIfHaveTargetInQueue(IngredientInd, extract);
		yield return CreateRepairToil();
	}

	private Toil CreateRepairToil()
	{
		Toil repair = ToilMaker.MakeToil("Repair");
		repair.initAction = delegate
		{
			ticksToNextRepair = 0f;
			repairTimeCostPerHP = GetRepairTimeCostPerHP();
		};
		repair.tickAction = delegate
		{
			Pawn actor = repair.actor;
			actor.rotationTracker.FaceTarget(Station);
			float craftingSpeed = actor.GetStatValue(StatDefOf.WorkSpeedGlobal);
			ticksToNextRepair += craftingSpeed;

			if (ticksToNextRepair >= repairTimeCostPerHP)
			{
				ticksToNextRepair -= repairTimeCostPerHP;

				List<Thing> resources = new List<Thing>();
				if (job.placedThings != null)
				{
					foreach (var tc in job.placedThings)
					{
						if (tc.thing != null && !tc.thing.Destroyed)
						{
							resources.Add(tc.thing);
						}
					}
				}

				if (StationComp.RepairTick(actor, resources))
				{
					actor.skills?.Learn(SkillDefOf.Crafting, 0.05f);
				}
				else
				{
					actor.jobs.EndCurrentJob(JobCondition.Succeeded);
				}
			}
		};
		repair.FailOnDespawnedOrNull(StationInd);
		repair.FailOnCannotTouch(StationInd, PathEndMode.Touch);
		repair.WithEffect(Station.def.repairEffect, StationInd);
		repair.defaultCompleteMode = ToilCompleteMode.Never;
		repair.activeSkill = () => SkillDefOf.Crafting;
		repair.handlingFacing = true;

		repair.AddFinishAction(delegate
		{
			StationComp?.ClearAccumulatedCosts();
		});

		return repair;
	}

	private float GetRepairTimeCostPerHP()
	{
		foreach (Apparel apparel in StationComp.HeldApparels)
		{
			if (apparel.HitPoints < apparel.MaxHitPoints)
			{
				var repairComp = apparel.GetComp<CompRepairableAtStation>();
				if (repairComp != null && repairComp.Props.repairResourcesPerHP != null && repairComp.Props.repairResourcesPerHP.Count > 0)
				{
					float skillLevel = pawn.skills.GetSkill(SkillDefOf.Crafting)?.Level ?? 0f;
					float skillFactor = 1f - (skillLevel * 0.05f);
					skillFactor = Mathf.Max(skillFactor, 0.1f);
					return repairComp.Props.repairTimeCostPerHP * skillFactor;
				}
			}
		}
		return 60f;
	}
}
