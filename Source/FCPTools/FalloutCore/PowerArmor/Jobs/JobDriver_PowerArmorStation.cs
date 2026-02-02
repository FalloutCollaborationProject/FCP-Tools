using Verse.AI;

namespace FCP.Core.PowerArmor;

public abstract class JobDriver_PowerArmorStation : JobDriver
{
	private Building Station => (Building)job.targetA.Thing;

	protected CompPowerArmorStation StationComp => Station.GetComp<CompPowerArmorStation>();

	protected int duration;

	protected int unequipBuffer;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref duration, "duration", 0);
		Scribe_Values.Look(ref unequipBuffer, "unequipBuffer", 0);
	}

	public override void Notify_Starting()
	{
		base.Notify_Starting();
		duration = GetDuration();
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnBurningImmobile(TargetIndex.A);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDespawnedNullOrForbidden(TargetIndex.A);
		int unequipDuration = GetUnequipDuration();
		if (unequipDuration > 0)
		{
			var unequipWait = Toils_General.Wait(unequipDuration).WithProgressBarToilDelay(TargetIndex.A);
			unequipWait.handlingFacing = true;
			unequipWait.tickIntervalAction = delegate(int delta)
			{
				unequipBuffer += delta;
				TryUnequipSomething();
				IntVec3 faceCell = pawn.Position + Station.Rotation.FacingCell;
				pawn.rotationTracker.FaceCell(faceCell);
			};
			unequipWait.PlaySustainerOrSound(() => GetCurrentUnequipSound());
			yield return unequipWait;
		}
		var wait = Toils_General.Wait(GetDuration()).WithProgressBarToilDelay(TargetIndex.A);
		wait.handlingFacing = true;
		wait.tickAction = delegate
		{
			IntVec3 faceCell = pawn.Position + Station.Rotation.FacingCell;
			pawn.rotationTracker.FaceCell(faceCell);
		};
		yield return wait;
		Toil action = ToilMaker.MakeToil();
		action.AddFinishAction(DoAction);
		action.defaultCompleteMode = ToilCompleteMode.Instant;
		yield return action;
	}

	protected abstract int GetDuration();

	protected abstract void DoAction();

	protected virtual int GetUnequipDuration()
	{
		int unequipDuration = 0;
		var heldApparels = StationComp.HeldApparels;
		var wornApparel = pawn.apparel.WornApparel;
		foreach (Apparel apparel in heldApparels)
		{
			for (int num = wornApparel.Count - 1; num >= 0; num--)
			{
				if (!ApparelUtility.CanWearTogether(apparel.def, wornApparel[num].def, pawn.RaceProps.body))
				{
					unequipDuration += (int)(wornApparel[num].GetStatValue(StatDefOf.EquipDelay) * 60f);
				}
			}
		}
		return unequipDuration;
	}

	protected virtual void TryUnequipSomething()
	{
		var heldApparels = StationComp.HeldApparels;
		foreach (Apparel apparel in heldApparels)
		{
			TryDropConflictingApparel(apparel);
		}
	}

	protected virtual SoundDef GetCurrentUnequipSound()
	{
		var heldApparels = StationComp.HeldApparels;
		var wornApparel = pawn.apparel.WornApparel;
		foreach (Apparel apparel in heldApparels)
		{
			for (int num = wornApparel.Count - 1; num >= 0; num--)
			{
				if (!ApparelUtility.CanWearTogether(apparel.def, wornApparel[num].def, pawn.RaceProps.body))
				{
					if (unequipBuffer >= (int)(wornApparel[num].GetStatValue(StatDefOf.EquipDelay) * 60f))
					{
						break;
					}
					return wornApparel[num].def.apparel.soundRemove;
				}
			}
		}
		return null;
	}

	protected void TryDropConflictingApparel(Apparel apparel)
	{
		List<Apparel> wornApparel = pawn.apparel.WornApparel;
		for (int num = wornApparel.Count - 1; num >= 0; num--)
		{
			if (!ApparelUtility.CanWearTogether(apparel.def, wornApparel[num].def, pawn.RaceProps.body))
			{
				int num2 = (int)(wornApparel[num].GetStatValue(StatDefOf.EquipDelay) * 60f);
				if (unequipBuffer >= num2)
				{
					bool forbid = pawn.Faction != null && pawn.Faction.HostileTo(Faction.OfPlayer);
					if (!pawn.apparel.TryDrop(wornApparel[num], out var _, pawn.PositionHeld, forbid))
					{
						Log.Error(pawn?.ToString() + " could not drop " + wornApparel[num].ToStringSafe());
						EndJobWith(JobCondition.Errored);
					}
				}
				break;
			}
		}
	}
}
