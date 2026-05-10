using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FCP.Core.Buildings;

public class CompPowerLineGizmos : ThingComp
{
    private Building_PowerLine PowerLine => parent as Building_PowerLine;
    private CompProperties_PowerLineGizmos Props => props as CompProperties_PowerLineGizmos;
    
    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (PowerLine.HasPawn || PowerLine.HasCorpse)
        {
            string label = PowerLine.HasPawn ? PowerLine.ContainedPawn.LabelShort : PowerLine.ContainedCorpse.InnerPawn.LabelShort;
            Command_Action remove = new Command_Action
            {
                defaultLabel = "FCP_RemoveFromPowerLine".Translate(label),
                defaultDesc = "FCP_RemoveFromPowerLineDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Commands/ReleaseAnimals"),
                iconDrawScale = Props?.iconScale ?? 0.65f,
                action = delegate
                {
                    Pawn hauler = FindBestHauler();
                    if (hauler != null)
                    {
                        Job job = JobMaker.MakeJob(JobDefOf.FCP_RemoveFromPowerLine, PowerLine);
                        hauler.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    }
                }
            };
            yield return remove;
        }
        else
        {
            Command_Action strap = new Command_Action
            {
                defaultLabel = "FCP_StrapToPowerLine".Translate(),
                defaultDesc = "FCP_StrapToPowerLineDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Commands/Attack"),
                iconDrawScale = Props?.iconScale ?? 0.65f,
                action = delegate
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    foreach (Pawn target in parent.Map.mapPawns.AllPawnsSpawned)
                    {
                        if (!target.Dead && target.RaceProps.Humanlike && (target.Downed || target.IsPrisonerOfColony || target.Faction != Faction.OfPlayer))
                        {
                            Pawn targetLocal = target;
                            string label = targetLocal.LabelShort;
                            if (targetLocal.IsPrisonerOfColony)
                                label += " (prisoner)";
                            else if (targetLocal.Downed)
                                label += " (downed)";
                            options.Add(new FloatMenuOption(label, delegate
                            {
                                Pawn hauler = FindBestHauler();
                                if (hauler != null)
                                {
                                    Job job = JobMaker.MakeJob(JobDefOf.FCP_StrapToPowerLine, targetLocal, PowerLine);
                                    hauler.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                                }
                            }));
                        }
                    }
                    if (options.Count == 0)
                        options.Add(new FloatMenuOption("FCP_NoTargets".Translate(), null));
                    Find.WindowStack.Add(new FloatMenu(options));
                }
            };
            yield return strap;
        }
    }
    
    private Pawn FindBestHauler()
    {
        return parent.Map.mapPawns.FreeColonistsSpawned
            .Where(p => !p.Downed && p.CanReach(parent, PathEndMode.InteractionCell, Danger.Deadly))
            .OrderBy(p => p.Position.DistanceTo(parent.Position))
            .FirstOrDefault();
    }
}
