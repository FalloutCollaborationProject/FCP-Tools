using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace FCP.Core.RadiantQuests
{
    public class QuestNode_Root_CharacterJoin : QuestNode_Root_WandererJoin_WalkIn
    {
        public SlateRef<CharacterDef> characterDef;

        protected override bool TestRunInt(Slate slate)
        {
            CharacterDef charDef = characterDef.GetValue(slate);
            if (charDef != null && UniqueCharactersTracker.Instance.CharacterPawnExists(charDef))
            {
                return false;
            }

            return base.TestRunInt(slate);
        }

        public override Pawn GeneratePawn()
        {
            CharacterDef charDef = characterDef.GetValue(QuestGen.slate);
            Pawn pawn = UniqueCharactersTracker.Instance.GetOrGenPawn(charDef);
            if (!pawn.IsWorldPawn())
            {
                Find.WorldPawns.PassToWorld(pawn);
            }
            return pawn;
        }
    }
}
