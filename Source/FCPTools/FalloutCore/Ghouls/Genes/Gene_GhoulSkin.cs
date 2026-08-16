namespace FCP.Core.Ghouls;

public class Gene_GhoulSkin : Gene
{
    public override void PostAdd()
    {
        base.PostAdd();
        RefreshGraphics();
    }

    public override void PostRemove()
    {
        base.PostRemove();
        RefreshGraphics();
    }

    private void RefreshGraphics()
    {
        if (pawn?.Drawer?.renderer == null)
            return;

        pawn.Drawer.renderer.SetAllGraphicsDirty();

        if (pawn.Drawer.renderer.renderTree != null)
        {
            pawn.Drawer.renderer.renderTree.SetDirty();
        }

        pawn.Drawer.renderer.EnsureGraphicsInitialized();

        if (pawn.Map != null)
        {
            pawn.Map.mapDrawer.MapMeshDirty(pawn.Position, MapMeshFlagDefOf.Things);
        }
    }
}