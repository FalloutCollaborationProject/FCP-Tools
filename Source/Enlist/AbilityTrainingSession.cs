using RimWorld;
using System.Linq;
using Verse;

namespace FCP.Enlist;

public class AbilityTrainingSession : IExposable
{
	public Pawn trainee;
	public int startTick;
	public int durationTicks;
	public FactionEnlistOptionsDef enlistOptionDef;
	public int trainingOptionIndex;

	private AbilityTrainingOption optionCached;
	public AbilityTrainingOption Option
	{
		get
		{
			optionCached ??= enlistOptionDef?.abilityTrainingOptions?.ElementAtOrDefault(trainingOptionIndex);
			return optionCached;
		}
	}

	public bool IsDone => trainee != null && !trainee.Dead && Find.TickManager.TicksGame >= startTick + durationTicks;

	public void Complete()
	{
		AbilityTrainingOption option = Option;
		if (option == null || trainee == null || trainee.Dead) return;

		if (option.abilityDef != null)
			trainee.abilities?.GainAbility(option.abilityDef);
		else if (option.hediffDef != null)
			trainee.health.AddHediff(option.hediffDef);
		else if (option.traitDef != null && trainee.story?.traits != null && !trainee.story.traits.HasTrait(option.traitDef))
			trainee.story.traits.GainTrait(new Trait(option.traitDef, option.traitDegree));
	}

	public void ExposeData()
	{
		Scribe_References.Look(ref trainee, "trainee");
		Scribe_Values.Look(ref startTick, "startTick");
		Scribe_Values.Look(ref durationTicks, "durationTicks");
		Scribe_Defs.Look(ref enlistOptionDef, "enlistOptionDef");
		Scribe_Values.Look(ref trainingOptionIndex, "trainingOptionIndex");
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
			optionCached = null;
	}
}
