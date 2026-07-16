using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace FCP.Core.Robotics
{
    public class QuestNode_Root_RobotCharacterJoin : QuestNode_Root_RobotJoin
    {
        public SlateRef<CharacterDef> characterDef;

        public override Pawn GeneratePawn()
        {
            CharacterDef charDef = characterDef.GetValue(QuestGen.slate);
            Pawn pawn;
            WildRobotSpawn_Patch.Suppress = true;
            try
            {
                pawn = UniqueCharactersTracker.Instance.GetOrGenPawn(charDef);
            }
            finally
            {
                WildRobotSpawn_Patch.Suppress = false;
            }

            if (!pawn.IsWorldPawn())
            {
                Find.WorldPawns.PassToWorld(pawn);
            }
            return pawn;
        }
    }
}
