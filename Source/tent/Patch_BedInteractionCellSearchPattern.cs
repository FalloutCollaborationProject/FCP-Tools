using System.Collections.Generic;
using HarmonyLib;
using Verse;
using RimWorld;

namespace Tent
{
    [HarmonyPatch(typeof(BedInteractionCellSearchPattern),nameof(BedInteractionCellSearchPattern.BedCellOffsets))]
    public class Patch_BedInteractionCellSearchPattern
    {
        public static bool Prefix(BedInteractionCellSearchPattern __instance, List<IntVec3> offsets, IntVec2 size)
        {

            if (size.z > 2)
            {
                offsets.Add(IntVec3.West);
                offsets.Add(IntVec3.East);
                offsets.Add(IntVec3.South);
                offsets.Add(IntVec3.North);
                offsets.Add(IntVec3.South + IntVec3.West);
                offsets.Add(IntVec3.South + IntVec3.East);
                offsets.Add(IntVec3.North + IntVec3.West);
                offsets.Add(IntVec3.North + IntVec3.East);
                offsets.Add(IntVec3.Zero);
                return false;
            }
            return true;

        }
    }
}   
