using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FCP.Core.Backpacks;

public class CompWeaponBackpack : ThingComp
{
    private Graphic backpackGraphic;
    
    public CompProperties_WeaponBackpack Props => (CompProperties_WeaponBackpack)props;
    
    public Graphic BackpackGraphic
    {
        get
        {
            if (backpackGraphic == null && !Props.backpackTexPath.NullOrEmpty())
            {
                backpackGraphic = GraphicDatabase.Get<Graphic_Single>(
                    Props.backpackTexPath,
                    ShaderDatabase.Cutout,
                    Vector2.one * Props.backpackScale,
                    Color.white
                );
            }
            return backpackGraphic;
        }
    }
    
    public override void PostExposeData()
    {
        base.PostExposeData();
    }
}

[HarmonyPatch(typeof(PawnRenderer), "DrawEquipment")]
public static class PawnRenderer_DrawEquipment_Patch
{
    private static readonly System.Reflection.FieldInfo pawnField = AccessTools.Field(typeof(PawnRenderer), "pawn");
    
    public static void Postfix(PawnRenderer __instance, Vector3 rootLoc, Rot4 pawnRotation, PawnRenderFlags flags)
    {
        if (pawnField?.GetValue(__instance) is not Pawn pawn) return;
        if (pawn?.equipment?.Primary == null) return;
        
        var comp = pawn.equipment.Primary.TryGetComp<CompWeaponBackpack>();
        if (comp == null || comp.BackpackGraphic == null) return;
        
        if (comp.Props.drawBackOnlyWhenDrafted && !pawn.Drafted) return;
        
        // Draw for north (back) and east (side) views
        if (pawnRotation != Rot4.North && pawnRotation != Rot4.East) return;
        
        Vector3 backpackLoc = rootLoc;
        backpackLoc.y += 0.04f; // Slightly above pawn layer
        
        if (pawnRotation == Rot4.North)
        {
            backpackLoc.z -= 0.25f; // Behind the pawn
        }
        else if (pawnRotation == Rot4.East)
        {
            backpackLoc.x += 0.2f; // To the right side
            backpackLoc.z -= 0.1f; // Slightly behind
        }
        
        Mesh mesh = MeshPool.plane10;
        Quaternion quat = Quaternion.AngleAxis(0f, Vector3.up);
        
        Graphics.DrawMesh(mesh, backpackLoc, quat, comp.BackpackGraphic.MatSingle, 0);
    }
}
