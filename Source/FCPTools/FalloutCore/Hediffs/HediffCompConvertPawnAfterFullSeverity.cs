using UnityEngine;

namespace FCP.Core.Hediffs;

public class HediffCompConvertPawnAfterFullSeverity : HediffComp
{
    public HediffCompProperties_ConvertPawnAfterFullSeverity Props =>
        (HediffCompProperties_ConvertPawnAfterFullSeverity)props;

    public Map currentMap => parent.pawn.Map;

    public IntVec3 curPos => parent.pawn.Position;

    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);
        if (Pawn.IsHashIntervalTick(60))
        {
            if (!Props.isInfectiousToMechanoid && Pawn.RaceProps.IsMechanoid)
            {
                Pawn.health.RemoveHediff(parent);
            }
        }

        if (parent.Severity >= Props.severityToTransform)
        {
            if (Props.isUsingImmunityList)
            {
                if (ModsConfig.BiotechActive && !Props.immuneXenotypeDef.NullOrEmpty())
                {
                    if (!Props.immuneXenotypeDef.Contains(parent.pawn.genes.Xenotype))
                    {
                        if (Props.isPlayer)
                        {
                            DoTransformation(Props.pawnKindDefs.RandomElement(), Faction.OfPlayer);
                        }
                        else
                        {
                            DoTransformation(Props.pawnKindDefs.RandomElement(),
                                Find.FactionManager.FirstFactionOfDef(Props.factionDef));
                        }
                    }
                    else
                    {
                        if (Props.removeIfImmune)
                        {
                            Pawn.health.RemoveHediff(parent);
                        }
                    }
                }

                if (!Props.immuneHediffDef.NullOrEmpty())
                {
                    if (!Props.immuneHediffDef.Any(x => Pawn.health.hediffSet.HasHediff(x)))
                    {
                        if (Props.isPlayer)
                        {
                            DoTransformation(Props.pawnKindDefs.RandomElement(), Faction.OfPlayer);
                        }
                        else
                        {
                            DoTransformation(Props.pawnKindDefs.RandomElement(),
                                Find.FactionManager.FirstFactionOfDef(Props.factionDef));
                        }
                    }
                    else
                    {
                        if (Props.removeIfImmune)
                        {
                            Pawn.health.RemoveHediff(parent);
                            MoteMaker.ThrowText(Pawn.Position.ToVector3(), Pawn.Map, "Immunity", Color.green);
                        }
                    }
                }
            }
            else
            {
                if (Props.isPlayer)
                {
                    DoTransformation(Props.pawnKindDefs.RandomElement(), Faction.OfPlayer);
                }
                else
                {
                    DoTransformation(Props.pawnKindDefs.RandomElement(),
                        Find.FactionManager.FirstFactionOfDef(Props.factionDef));
                }
            }
        }
    }

    public void DoTransformation(PawnKindDef PawnKind, Faction faction)
    {
        for (int i = 0; i < Props.numberToSpawn; i++)
        {
            Pawn newThing = PawnGenerator.GeneratePawn(PawnKind, faction);
            if (Props.factionDef != null)
            {
                if (newThing.Faction != faction)
                {
                    newThing.SetFaction(faction);
                }
            }

            if (Props.inheritName)
            {
                newThing.Name = Pawn.Name;
            }

            if (Props.inheritAge)
            {
                newThing.ageTracker.AgeBiologicalTicks = Pawn.ageTracker.AgeBiologicalTicks;
            }

            if (Props.inheritBackground)
            {
                if (newThing.story != null)
                {
                    if (Pawn.story?.Childhood != null)
                    {
                        newThing.story.Childhood = Pawn.story.Childhood;
                    }

                    if (Pawn.story?.Adulthood != null)
                    {
                        newThing.story.Adulthood = Pawn.story.Adulthood;
                    }
                }
            }

            if (Props.inheritSkills)
            {
                if (newThing.skills != null)
                {
                    IReadOnlyList<SkillRecord> skills = newThing.skills.skills;
                    if (Pawn.skills != null)
                    {
                        foreach (var item in skills)
                        {
                            SkillRecord skillRecord = Pawn.skills.GetSkill(item.def);
                            if (skillRecord != null)
                            {
                                item.Level = skillRecord.Level;
                            }
                        }
                    }
                }
            }

            GenSpawn.Spawn(newThing, curPos, currentMap);
        }

        parent.pawn.Kill(new DamageInfo(DamageDefOf.Cut, 999f, 999f));
        parent.pawn.Corpse.Destroy();
    }
}
