using Verse.Sound;

namespace Thek_BuildingArrivalMode
{
    public class Comp_TickBuildingSpawn : ThingComp
    {
        readonly List<Pawn> pawnsToSpawn = new();
        internal Rot4 spawnRot;
        internal BuildingArrivalModeModExtension modExtension;
        public override void CompTick()
        {
            if (!pawnsToSpawn.NullOrEmpty() && parent.IsHashIntervalTick(modExtension.cooldownBetweenPawnsInTicks))
            {
                Pawn pawn = pawnsToSpawn.First();
                IntVec3 spawnLocation = ThingUtility.InteractionCell(new IntVec3(0, 0, -1), parent.Position, parent.Rotation);
                GenSpawn.Spawn(pawn, spawnLocation, parent.Map, spawnRot);
                modExtension.soundWhenSpawning?.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
                if (modExtension.fleckWhenSpawning != null) parent.Map.flecks.CreateFleck(FleckMaker.GetDataStatic(pawn.DrawPos, pawn.Map, modExtension.fleckWhenSpawning));

                pawnsToSpawn.Remove(pawn);
            }
            base.CompTick();
        }

        public void Spawn(List<Pawn> pawnsFromArrivalMode)
        {
            foreach (Pawn pawn in pawnsFromArrivalMode)
            {
                pawnsToSpawn.Add(pawn);
            }
        }
    }
}