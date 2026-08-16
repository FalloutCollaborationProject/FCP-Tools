using System.Linq;
using Verse.AI;

namespace FCP.Core.Turrets;

public class WorkGiver_InstallTurretWeapon : WorkGiver_Scanner
{
    public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);

    public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        CompTurretWeaponMount mount = t.TryGetComp<CompTurretWeaponMount>();
        Thing weapon = mount?.ReservedWeapon;
        if (mount == null || mount.Armed || weapon == null)
        {
            return false;
        }
        if (weapon.Destroyed || !weapon.Spawned || weapon.IsForbidden(pawn))
        {
            return false;
        }
        if (!pawn.CanReserve(weapon, 1, -1, null, forced) || !pawn.CanReserve(t, 1, -1, null, forced))
        {
            return false;
        }
        if (!pawn.CanReach(weapon, PathEndMode.ClosestTouch, Danger.Deadly))
        {
            return false;
        }
        return CountAvailableComponents(pawn.Map) >= mount.Props.componentCost;
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        CompTurretWeaponMount mount = t.TryGetComp<CompTurretWeaponMount>();
        return JobMaker.MakeJob(JobDefOf_Turrets.FCP_Job_InstallTurretWeapon, mount.ReservedWeapon, t);
    }

    private static int CountAvailableComponents(Map map) =>
        map.listerThings.ThingsOfDef(ThingDefOf.ComponentIndustrial)
            .Where(comp => !comp.IsForbidden(Faction.OfPlayer))
            .Sum(comp => comp.stackCount);
}
