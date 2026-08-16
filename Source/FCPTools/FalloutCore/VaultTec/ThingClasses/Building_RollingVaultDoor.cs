using UnityEngine;
using Verse.Sound;

namespace FCPVT;

[StaticConstructorOnStartup]
public class Building_RollingVaultDoor : Building_Door
{
    private const string PipboyApparelTag = "FCP_Pipboy";
    private const float SteamFleckSize = 3f;

    public new float OpenPct => Mathf.Clamp01((float)ticksSinceOpen / (float)TicksToOpenNow);

    public override bool PawnCanOpen(Pawn p)
    {
        return base.PawnCanOpen(p) && PawnHasPipboy(p);
    }

    protected override void DoorOpen(int ticksToClose = 110)
    {
        bool wasOpen = Open;
        base.DoorOpen(ticksToClose);

        if (wasOpen || Map == null)
        {
            return;
        }

        if (VTDefOf.FCPVT_VaultDoorOpen != null)
        {
            SoundStarter.PlayOneShot(VTDefOf.FCPVT_VaultDoorOpen, SoundInfo.InMap(new TargetInfo(Position, Map), MaintenanceType.None));
        }

        FleckMaker.ThrowSmoke(Position.ToVector3Shifted(), Map, SteamFleckSize);
    }

    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        base.DrawAt(drawLoc, flip);
        Comps_PostDraw();
    }

    private static bool PawnHasPipboy(Pawn p)
    {
        List<Apparel> worn = p.apparel?.WornApparel;
        if (worn == null)
        {
            return false;
        }

        for (int i = 0; i < worn.Count; i++)
        {
            if (worn[i].def.apparel.tags.Contains(PipboyApparelTag))
            {
                return true;
            }
        }

        return false;
    }
}
