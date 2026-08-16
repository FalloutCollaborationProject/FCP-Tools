using HarmonyLib;

namespace FCP.Core;

[HarmonyPatch(typeof(PrefabUtility), nameof(PrefabUtility.SpawnPrefab))]
public static class PrefabUtility_SpawnPrefab_Roof_Patch
{
    public static void Postfix(PrefabDef prefab, Map map, IntVec3 pos, Rot4 rot)
    {
        PrefabRoofExtension ext = prefab.GetModExtension<PrefabRoofExtension>();
        if (ext?.roofs == null || ext.roofs.Count == 0)
            return;

        rot = PrefabUtility.ValidateRotation(prefab, rot);
        IntVec3 root = PrefabUtility.GetRoot(prefab, pos, rot);

        foreach (PrefabRoofData roofData in ext.roofs)
        {
            foreach (CellRect rect in roofData.rects)
            {
                foreach (IntVec3 local in rect.Cells)
                {
                    IntVec3 cell = root + PrefabUtility.GetAdjustedLocalPosition(local, rot);
                    if (cell.InBounds(map))
                        map.roofGrid.SetRoof(cell, roofData.def);
                }
            }
        }
    }
}
