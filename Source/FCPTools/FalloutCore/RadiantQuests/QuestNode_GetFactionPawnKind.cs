using System.Linq;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace FCP.Core.RadiantQuests
{
    public class QuestNode_GetFactionPawnKind : QuestNode
    {
        public SlateRef<Faction> faction;
        public SlateRef<Pawn> pawn;
        public SlateRef<string> storeAs;

        protected override bool TestRunInt(Slate slate) => true;

        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;
            
            Faction fac = faction.GetValue(slate);
            if (fac == null)
            {
                Pawn p = pawn.GetValue(slate);
                if (p != null)
                {
                    fac = p.Faction;
                }
            }
            
            if (fac == null)
            {
                Log.Error("[QuestNode_GetFactionPawnKind] Faction is null and no valid pawn provided");
                return;
            }

            PawnKindDef pawnKind = fac.def.pawnGroupMakers
                ?.FirstOrDefault(pgm => pgm.kindDef == PawnGroupKindDefOf.Combat)
                ?.options?.RandomElementByWeight(opt => opt.selectionWeight)?.kind;

            if (pawnKind == null)
            {
                pawnKind = fac.def.pawnGroupMakers
                    ?.FirstOrDefault()
                    ?.options?.RandomElementByWeight(opt => opt.selectionWeight)?.kind;
            }

            if (pawnKind == null)
            {
                Log.Error($"[QuestNode_GetFactionPawnKind] Could not find pawn kind for faction {fac.Name}");
                pawnKind = PawnKindDefOf.Villager;
            }

            string storeAsValue = storeAs.GetValue(slate);
            if (!storeAsValue.NullOrEmpty())
            {
                QuestGen.slate.Set(storeAsValue, pawnKind);
            }
        }
    }
}
