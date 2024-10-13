using Verse.Sound;

namespace FCP.Core.Birds;

public class CompFlyingPawn : ThingComp
{
    public bool isFlyingCurrently;
    public CompProperties_FlyingPawn Props => props as CompProperties_FlyingPawn;
    public Pawn pawn => parent as Pawn;

    public void ChangeGraphic(bool withSound = false)
    {
        pawn.Drawer.renderer.renderTree.SetDirty();
        if (!withSound || Props.soundOnFly == null)
        {
            return;
        }

        var info = SoundInfo.InMap(new TargetInfo(pawn.PositionHeld, pawn.MapHeld));
        Props.soundOnFly.PlayOneShot(info);
    }


    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref isFlyingCurrently, "isFlyingCurrently");
    }
}