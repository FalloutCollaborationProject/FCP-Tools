using System.Linq;
using System.Reflection;
using Verse.AI;

namespace FCP.Core.Turrets;

public class CompTurretWeaponMount : ThingComp
{
    private static readonly MethodInfo UpdateGunVerbsMethod = typeof(Building_TurretGun).GetMethod("UpdateGunVerbs", BindingFlags.NonPublic | BindingFlags.Instance);

    public CompProperties_TurretWeaponMount Props => (CompProperties_TurretWeaponMount)props;

    private ThingDef installedWeaponDef;
    private Thing reservedWeapon;

    public bool Armed => installedWeaponDef != null;
    public Thing ReservedWeapon => reservedWeapon;

    private Building_TurretGun Turret => (Building_TurretGun)parent;

    private CompPowerTrader PowerComp => parent.GetComp<CompPowerTrader>();

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Defs.Look(ref installedWeaponDef, "installedWeaponDef");
        Scribe_References.Look(ref reservedWeapon, "reservedWeapon");
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        if (installedWeaponDef != null)
        {
            RebuildGun(installedWeaponDef);
        }
        UpdatePower();
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (Armed)
        {
            yield return new Command_Action
            {
                defaultLabel = "Remove weapon",
                defaultDesc = "Unmount the installed weapon and return it.",
                icon = TexCommand.ClearPrioritizedWork,
                action = RemoveWeapon
            };
            yield break;
        }

        if (reservedWeapon != null)
        {
            yield return new Command_Action
            {
                defaultLabel = "Cancel install",
                defaultDesc = $"Cancel installing {reservedWeapon.LabelShort} on this mount.",
                icon = TexCommand.ClearPrioritizedWork,
                action = () => reservedWeapon = null
            };
            yield break;
        }

        yield return new Command_Action
        {
            defaultLabel = "Install weapon",
            defaultDesc = "Mount a ranged weapon on this turret. Consumes the weapon and " + Props.componentCost + " components, and increases power draw while armed.",
            icon = TexCommand.Install,
            action = OpenInstallMenu
        };
    }

    private void OpenInstallMenu()
    {
        Map map = parent.Map;
        List<Thing> candidates = map.listerThings.ThingsInGroup(ThingRequestGroup.Weapon)
            .Where(t => t.def.IsRangedWeapon
                && !t.def.destroyOnDrop
                && t.TryGetComp<CompEquippable>() != null
                && !t.IsForbidden(Faction.OfPlayer)
                && map.reachability.CanReach(parent.Position, t, PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.PassDoors)))
            .ToList();

        if (candidates.NullOrEmpty())
        {
            Messages.Message("No ranged weapons available to install.", parent, MessageTypeDefOf.RejectInput, false);
            return;
        }

        List<FloatMenuOption> options = new List<FloatMenuOption>();
        foreach (Thing weapon in candidates)
        {
            Thing target = weapon;
            options.Add(new FloatMenuOption(target.LabelCap, () =>
            {
                reservedWeapon = target;
                Messages.Message($"{target.LabelShort} reserved for installation on {parent.LabelShort}.", parent, MessageTypeDefOf.TaskCompletion, false);
                TryAssignImmediately();
            }));
        }
        Find.WindowStack.Add(new FloatMenu(options));
    }

    private void TryAssignImmediately()
    {
        List<Pawn> pawns = parent.Map.mapPawns.FreeColonistsSpawned;
        if (pawns.Count == 0)
        {
            Messages.Message("No free colonists on the map.", parent, MessageTypeDefOf.RejectInput, false);
            return;
        }

        Pawn best = null;
        string blockReason = null;
        float bestDist = float.MaxValue;
        foreach (Pawn p in pawns)
        {
            string reason = WhyCannotAssign(p);
            if (reason != null)
            {
                blockReason ??= $"{p.LabelShort}: {reason}";
                continue;
            }
            float dist = p.Position.DistanceToSquared(parent.Position);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = p;
            }
        }

        if (best == null)
        {
            Messages.Message($"No colonist can install it right now ({blockReason}). It'll be picked up automatically once someone's free.", parent, MessageTypeDefOf.NeutralEvent, false);
            return;
        }

        Job job = JobMaker.MakeJob(JobDefOf_Turrets.FCP_Job_InstallTurretWeapon, reservedWeapon, parent);
        bool started = best.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        Messages.Message(started
            ? $"{best.LabelShort} is heading to install the weapon."
            : $"{best.LabelShort} could not start the install job (job assignment rejected).",
            parent, started ? MessageTypeDefOf.TaskCompletion : MessageTypeDefOf.RejectInput, false);
    }

    private string WhyCannotAssign(Pawn p)
    {
        if (p.Downed) return "downed";
        if (p.Drafted) return "drafted";
        if (p.InMentalState) return "in a mental state";
        if (!p.CanReserve(reservedWeapon)) return "can't reserve the weapon";
        if (!p.CanReach(reservedWeapon, PathEndMode.ClosestTouch, Danger.None)) return "can't reach the weapon";
        if (!p.CanReserve(parent)) return "can't reserve the turret";
        if (!p.CanReach(parent, PathEndMode.ClosestTouch, Danger.None)) return "can't reach the turret";
        return null;
    }

    public void FinishInstall(ThingDef weaponDef)
    {
        installedWeaponDef = weaponDef;
        reservedWeapon = null;
        RebuildGun(weaponDef);
        UpdatePower();
    }

    private void RemoveWeapon()
    {
        if (!Armed)
        {
            return;
        }

        ThingDef weaponDef = installedWeaponDef;
        installedWeaponDef = null;
        RebuildGun(Props.emptyGunDef);
        UpdatePower();

        if (parent.Map != null)
        {
            Thing dropped = ThingMaker.MakeThing(weaponDef);
            GenPlace.TryPlaceThing(dropped, parent.Position, parent.Map, ThingPlaceMode.Near);
        }
    }

    private void RebuildGun(ThingDef gunDef)
    {
        Turret.gun = ThingMaker.MakeThing(gunDef);
        UpdateGunVerbsMethod?.Invoke(Turret, null);
    }

    private void UpdatePower()
    {
        CompPowerTrader power = PowerComp;
        if (power == null)
        {
            return;
        }
        power.PowerOutput = Armed ? -Props.powerConsumptionArmed : -Props.powerConsumptionUnarmed;
    }
}
