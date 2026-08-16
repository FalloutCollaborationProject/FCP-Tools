using System.Collections.Generic;
using FCP.Core;
using Verse;

namespace FCP.BrotherhoodOfSteel
{
    public class HiddenValleyNamedPawnEntry
    {
        public CharacterDef character;
        public IntVec3 position;
    }

    public class GenStep_HiddenValleyNamedPawns : GenStep
    {
        public List<HiddenValleyNamedPawnEntry> pawns = new List<HiddenValleyNamedPawnEntry>();

        public override int SeedPart => 1935240611;

        public override void Generate(Map map, GenStepParams parms)
        {
            UniqueCharactersTracker tracker = UniqueCharactersTracker.Instance;
            if (tracker == null || pawns.NullOrEmpty()) return;

            foreach (HiddenValleyNamedPawnEntry entry in pawns)
            {
                if (entry.character == null) continue;

                Pawn pawn = tracker.GetOrGenPawn(entry.character);
                if (pawn == null || pawn.Spawned) continue;

                GenSpawn.Spawn(pawn, entry.position, map);
            }
        }
    }
}
