using RimWorld;
using Verse;

namespace FCP_Ghoul
{
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

            // Force complete graphics refresh
            pawn.Drawer.renderer.SetAllGraphicsDirty();
            
            // Regenerate render tree
            if (pawn.Drawer.renderer.renderTree != null)
            {
                pawn.Drawer.renderer.renderTree.SetDirty();
            }
            
            // Force graphics to be reinitialized
            pawn.Drawer.renderer.EnsureGraphicsInitialized();
            
            // Notify the graphics are dirty
            if (pawn.Map != null)
            {
                pawn.Map.mapDrawer.MapMeshDirty(pawn.Position, MapMeshFlagDefOf.Things);
            }
        }
    }
}
