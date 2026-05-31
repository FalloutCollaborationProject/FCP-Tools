using RimWorld;
using RimWorld.QuestGen;
using System.Linq;
using Verse;

namespace FCP.Core.RadiantQuests;

public class QuestNode_GetFactionWithVertibird : QuestNode
{
    public SlateRef<string> storeFactionAs;
    public SlateRef<string> storeLeaderAs;
    public SlateRef<bool> allowPermanentEnemy = false;
    public SlateRef<bool> allowEnemy = false;
    public SlateRef<bool> allowNeutral = true;
    public SlateRef<bool> allowAlly = true;

    protected override bool TestRunInt(Slate slate)
    {
        return TryGetFaction(out Faction _);
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        if (TryGetFaction(out Faction faction))
        {
            slate.Set(storeFactionAs.GetValue(slate), faction);
            if (faction.leader != null)
            {
                slate.Set(storeLeaderAs.GetValue(slate), faction.leader);
            }
        }
    }

    private bool TryGetFaction(out Faction faction)
    {
        bool allowPerm = allowPermanentEnemy.GetValue(QuestGen.slate);
        bool allowEnem = allowEnemy.GetValue(QuestGen.slate);
        bool allowNeut = allowNeutral.GetValue(QuestGen.slate);
        bool allowAll = allowAlly.GetValue(QuestGen.slate);

        var candidates = Find.FactionManager.AllFactionsListForReading
            .Where(f => f.def.GetModExtension<FactionExtension_Vertibird>()?.crashedVertibird != null)
            .Where(f => f.leader != null && !f.leader.Dead && !f.IsPlayer && !f.defeated && !f.Hidden);

        faction = candidates.Where(f =>
        {
            if (f.HostileTo(Faction.OfPlayer))
            {
                return f.def.permanentEnemy ? allowPerm : allowEnem;
            }
            return f.PlayerRelationKind == FactionRelationKind.Ally ? allowAll : allowNeut;
        }).RandomElementWithFallback();

        return faction != null && faction.leader != null;
    }
}
