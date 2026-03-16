using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FCP.Core.Backpacks;

public class CompWeaponBackpack : ThingComp
{
    private Graphic cachedGraphic;
    
    public CompProperties_WeaponBackpack Props => (CompProperties_WeaponBackpack)props;
    
    public Graphic BackpackGraphic
    {
        get
        {
            if (cachedGraphic == null && !Props.backpackTexPath.NullOrEmpty())
            {
                cachedGraphic = GraphicDatabase.Get<Graphic_Multi>(
                    Props.backpackTexPath,
                    ShaderDatabase.Cutout,
                    Vector2.one * Props.backpackScale,
                    Color.white
                );
            }
            return cachedGraphic;
        }
    }
}

[HarmonyPatch(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawEquipmentAndApparelExtras))]
public static class PawnRenderUtility_DrawEquipmentAndApparelExtras_Patch
{
    public static void Prefix(Pawn pawn, Vector3 drawPos, Rot4 facing, PawnRenderFlags flags)
    {
        if (pawn?.equipment?.Primary == null) return;
        
        var comp = pawn.equipment.Primary.TryGetComp<CompWeaponBackpack>();
        if (comp?.BackpackGraphic == null) return;
        if (comp.Props.drawBackOnlyWhenDrafted && !pawn.Drafted) return;
        
        Vector3 backpackPos = pawn.DrawPos;
        if (facing == Rot4.North)
            backpackPos += comp.Props.northOffset;
        else if (facing == Rot4.South)
            backpackPos += comp.Props.southOffset;
        else if (facing == Rot4.East)
            backpackPos += comp.Props.eastOffset;
        else // Rot4.West
            backpackPos += new Vector3(-comp.Props.eastOffset.x, comp.Props.eastOffset.y, comp.Props.eastOffset.z);
        
        Mesh mesh = (facing == Rot4.West) ? MeshPool.plane10Flip : MeshPool.plane10;
        Graphics.DrawMesh(mesh, backpackPos, Quaternion.identity, comp.BackpackGraphic.MatAt(facing), 0);
    }
}

