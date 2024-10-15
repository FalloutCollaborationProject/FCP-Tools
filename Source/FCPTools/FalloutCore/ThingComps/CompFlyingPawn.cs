using Verse.Sound;

namespace FCP.Core;

public class CompFlyingPawn : ThingComp
{
    public CompProperties_FlyingPawn Props => props as CompProperties_FlyingPawn;
        
    public Pawn pawn => parent as Pawn;
        
    public bool isFlyingCurrently;
        
    public void ChangeGraphic(bool withSound = false)
    {
        pawn.Drawer.renderer.renderTree.SetDirty();
            
        if (!withSound || Props.soundOnFly == null) return;
        SoundInfo info = SoundInfo.InMap(new TargetInfo(pawn.PositionHeld, pawn.MapHeld));
        Props.soundOnFly.PlayOneShot(info);
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref isFlyingCurrently, "isFlyingCurrently");
    }
}