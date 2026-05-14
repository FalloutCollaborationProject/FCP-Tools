using UnityEngine;
using Verse.AI;

namespace FCP.Core.WeaponCondition;

public class CompProperties_WeaponCondition : CompProperties
{
    public float conditionLossPerShot = 0.15f;
    public float jamThreshold = 30f;
    public float maxJamChance = 0.25f;
    public float spawnConditionMin = 25f;
    public float spawnConditionMax = 100f;

    public CompProperties_WeaponCondition() => compClass = typeof(CompWeaponCondition);
}

public class CompWeaponCondition : ThingComp
{
    private float condition = 100f;
    private bool isJammed;
    private bool spawned;

    public float Condition => condition;
    public bool IsDisabled => condition <= 1f;
    public bool IsJammed => isJammed;

    public CompProperties_WeaponCondition Props => (CompProperties_WeaponCondition)props;

    public override void PostExposeData()
    {
        Scribe_Values.Look(ref condition, "condition", 100f);
        Scribe_Values.Look(ref isJammed, "isJammed", false);
        Scribe_Values.Look(ref spawned, "spawned", false);
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        if (spawned || respawningAfterLoad) return;
        spawned = true;
        condition = Rand.Range(Props.spawnConditionMin, Props.spawnConditionMax);
    }

    public void NotifyShot()
    {
        condition = Mathf.Max(1f, condition - Props.conditionLossPerShot);
    }

    public void AddCondition(float amount)
    {
        condition = Mathf.Min(100f, condition + amount);
    }

    public void SetCondition(float value)
    {
        condition = Mathf.Clamp(value, 1f, 100f);
    }

    public bool TryJam(Pawn wielder)
    {
        if (condition >= Props.jamThreshold)
            return false;

        float t = 1f - (condition - 1f) / (Props.jamThreshold - 1f);
        if (Rand.Value >= Props.maxJamChance * t)
            return false;

        isJammed = true;
        if (wielder != null)
            Messages.Message("FCP_WeaponCondition_JamMessage".Translate(wielder.LabelShort, parent.LabelShort),
                wielder, MessageTypeDefOf.NegativeEvent, historical: false);
        return true;
    }

    public void ClearJam()
    {
        isJammed = false;
    }

    public override string CompInspectStringExtra()
    {
        string condStr = "FCP_WeaponCondition_Condition".Translate(condition.ToString("F0"));
        if (IsDisabled)
            return condStr + "\n" + "FCP_WeaponCondition_Disabled".Translate();
        if (IsJammed)
            return condStr + "\n" + "FCP_WeaponCondition_Jammed".Translate();
        return condStr;
    }

    public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
    {
        if (!FCPCoreMod.Settings.General.weaponConditionEnabled)
            yield break;
        if (!selPawn.story.traits.HasTrait(WeaponConditionDefOf.FCP_Trait_JuryRigging))
            yield break;

        ThingWithComps equipped = selPawn.equipment.Primary;
        if (equipped == null || equipped.def != parent.def)
            yield break;

        CompWeaponCondition equippedComp = equipped.GetComp<CompWeaponCondition>();
        if (equippedComp == null || (equippedComp.Condition >= 100f && !equippedComp.IsJammed))
            yield break;

        if (!selPawn.CanReserveAndReach(parent, PathEndMode.Touch, Danger.Deadly))
        {
            yield return new FloatMenuOption("FCP_JuryRig_FloatMenu".Translate(equipped.LabelShort) + " (" + "NoPath".Translate() + ")", null);
            yield break;
        }

        yield return new FloatMenuOption("FCP_JuryRig_FloatMenu".Translate(equipped.LabelShort), () =>
        {
            selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(WeaponConditionDefOf.FCP_Job_JuryRigWeapon, parent));
        });
    }
}
