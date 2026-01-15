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
            // Won't do anything at all if the list is empty (Which should be empty except when Arrive() runs)
            // If it's not empty, it'll run depending on the cooldown specified in the modExtension.
            if (!pawnsToSpawn.NullOrEmpty() && parent.IsHashIntervalTick(modExtension.cooldownBetweenPawnsInTicks))
            {
                Pawn pawn = pawnsToSpawn.First();
                //Gets the first pawn from the list of pawns to spawn, we don't need a loop this way!
                IntVec3 spawnLocation = ThingUtility.InteractionCell(new IntVec3(0, 0, -1), parent.Position, parent.Rotation);
                //The place to spawn is the same tile that we're checking if it's valid in TryResolveRaidSpawnCenter.
                GenSpawn.Spawn(pawn, spawnLocation, parent.Map, spawnRot);
                modExtension.soundWhenSpawning?.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
                //If you've not specified a sound, it won't do anything. But if you have, it'll just play it.
                if (modExtension.fleckWhenSpawning != null) parent.Map.flecks.CreateFleck(FleckMaker.GetDataStatic(pawn.DrawPos, pawn.Map, modExtension.fleckWhenSpawning));
                //Same with the fleck

                pawnsToSpawn.Remove(pawn);
                //Removes the pawn, so the first pawn in the list changes, essentially making it iterate through all the list, until it's empty, then it won't run anymore.
            }
            base.CompTick();
        }

        /// <summary>
        /// This just passes the list of pawns that have to spawn from the raid into this comp so the intended pawns spawn
        /// </summary>
        /// <param name="pawnsFromArrivalMode"></param>
        public void Spawn(List<Pawn> pawnsFromArrivalMode)
        {
            foreach (Pawn pawn in pawnsFromArrivalMode)
            {
                pawnsToSpawn.Add(pawn);
            }
        }
    }
}