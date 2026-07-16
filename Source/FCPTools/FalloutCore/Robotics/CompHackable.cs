using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FCP.Core.Robotics
{
    public class CompProperties_Hackable : CompProperties
    {
        public float baseSuccessChance = 0.5f;
        public SimpleCurve skillFactorCurve = new SimpleCurve
        {
            new CurvePoint(0f, 0.5f),
            new CurvePoint(20f, 1.5f),
        };
        public int workTicks = 600;

        public CompProperties_Hackable()
        {
            compClass = typeof(CompHackable);
        }
    }

    public class CompHackable : ThingComp
    {
        public CompProperties_Hackable Props => (CompProperties_Hackable)props;

        public bool CanBeHacked
        {
            get
            {
                Pawn pawn = parent as Pawn;
                if (pawn == null || pawn.Faction == Faction.OfPlayer)
                {
                    return false;
                }
                CompRefuelable fuel = pawn.GetComp<CompRefuelable>();
                return fuel != null && !fuel.HasFuel;
            }
        }

        public float GetSuccessChance(Pawn hacker)
        {
            float skillLevel = hacker.skills?.GetSkill(SkillDefOf.Intellectual).Level ?? 0f;
            float factor = Props.skillFactorCurve.Evaluate(skillLevel);
            return Mathf.Clamp01(Props.baseSuccessChance * factor);
        }

        public void AttemptHack(Pawn hacker)
        {
            Pawn target = parent as Pawn;
            if (target == null || !target.Spawned)
            {
                return;
            }

            if (Rand.Chance(GetSuccessChance(hacker)))
            {
                target.SetFaction(Faction.OfPlayer, hacker);
                target.GetComp<CompSecuritronFace>()?.Notify_FactionChanged();
                Messages.Message("FCP_Hack_Success".Translate(target.LabelShort), target, MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                target.SetFaction(Faction.OfMechanoids);
                target.GetComp<CompSecuritronFace>()?.Notify_FactionChanged();
                Messages.Message("FCP_Hack_Failure".Translate(target.LabelShort), target, MessageTypeDefOf.NegativeEvent);
            }
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (!CanBeHacked)
            {
                yield break;
            }

            if (!selPawn.CanReach(parent, PathEndMode.Touch, Danger.Deadly))
            {
                yield return new FloatMenuOption("FCP_HackRobot_Perform".Translate() + ": " + "NoPath".Translate(), null);
                yield break;
            }

            float chance = GetSuccessChance(selPawn);
            string label = "FCP_HackRobot_Perform".Translate() + " (" + chance.ToStringPercent() + ")";
            yield return new FloatMenuOption(label, delegate
            {
                Job job = JobMaker.MakeJob(JobDefOf_Robotics.FCP_HackRobot, parent);
                selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            });
        }
    }
}
