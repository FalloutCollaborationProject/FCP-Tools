using System.Linq;
using ArmorRacks.Things;
using RimWorld;
using Verse;

namespace ArmorRacks.ThingComps
{
    public class CompAssignableToPawn_ArmorRacks : CompAssignableToPawn
    {
        protected override bool ShouldShowAssignmentGizmo() => false;
        public new int MaxAssignedPawnsCount => 1;

        public override void TryAssignPawn(Pawn pawn)
        {
            if (this.assignedPawns.Contains(pawn))
                return;
            var racks = Find.CurrentMap.listerBuildings.AllBuildingsColonistOfClass<ArmorRack>();
            foreach (var rack in racks)
            {
                var c = rack.GetComp<CompAssignableToPawn_ArmorRacks>();
                c.TryUnassignPawn(pawn);
            }
            assignedPawns.Add(pawn);
            this.SortAssignedPawns();
        }

        public override bool AssignedAnything(Pawn pawn)
        {
            var racks = Find.CurrentMap.listerBuildings.AllBuildingsColonistOfClass<ArmorRack>();
            foreach (var rack in racks)
            {
                var c = rack.GetComp<CompAssignableToPawn_ArmorRacks>();
                if (c.AssignedPawns.Contains(pawn))
                {
                    return true;
                }
            }
            return false;
        }
    }
}