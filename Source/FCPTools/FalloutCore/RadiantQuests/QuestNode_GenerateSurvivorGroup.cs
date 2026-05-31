using System;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace FCP.Core.RadiantQuests;

public class QuestNode_GenerateSurvivorGroup : QuestNode
{
    public SlateRef<Faction> faction;
    public SlateRef<string> storeAs;

    protected override bool TestRunInt(Slate slate)
    {
        return true;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        Faction fac = faction != null ? faction.GetValue(slate) : null;
        if (fac == null && slate.Exists("askerFaction"))
        {
            fac = slate.Get<Faction>("askerFaction");
        }
        if (fac == null) return;
        
        int tile = -1;
        if (slate.Exists("site"))
        {
            MapParent site = slate.Get<MapParent>("site");
            if (site != null) tile = site.Tile;
        }

        FactionExtension_Vertibird ext = fac.def?.GetModExtension<FactionExtension_Vertibird>();
        SurvivorGroupConfig config = null;
        float difficultyMult = 1f;

        if (ext?.survivorGroups?.Any() == true)
        {
            config = ext.survivorGroups.RandomElement();
            difficultyMult = config.difficultyMultiplier;
        }

        List<Pawn> pawns = config != null 
            ? GenerateFromConfig(config, fac, tile) 
            : GenerateDefaultGroup(fac, tile);

        slate.Set(storeAs.GetValue(slate), pawns);
        slate.Set("groupDifficultyMult", difficultyMult);
    }

    List<Pawn> GenerateDefaultGroup(Faction fac, int tile)
    {
        int groupCount = Rand.RangeInclusive(3, 8);
        PawnKindDef kind = GetCombatPawns(fac).RandomElementWithFallback(fac.def.basicMemberKind);
        
        List<Pawn> pawns = new List<Pawn>(groupCount);
        for (int i = 0; i < groupCount && kind != null; i++)
        {
            pawns.Add(GeneratePawn(kind, fac, tile));
        }
        
        return pawns;
    }

    List<Pawn> GenerateFromConfig(SurvivorGroupConfig config, Faction fac, int tile)
    {
        List<Pawn> pawns = new List<Pawn>(config.count);
        
        if (config.leaderKind != null)
        {
            pawns.Add(GeneratePawn(config.leaderKind, fac, tile));
        }
        
        PawnKindDef basic = config.basicKind ?? fac.def.basicMemberKind;
        while (pawns.Count < config.count && basic != null)
        {
            pawns.Add(GeneratePawn(basic, fac, tile));
        }
        
        return pawns;
    }

    List<PawnKindDef> GetCombatPawns(Faction fac)
    {
        if (fac?.def?.pawnGroupMakers == null) return new List<PawnKindDef>();

        HashSet<PawnKindDef> kinds = new HashSet<PawnKindDef>();
        foreach (var maker in fac.def.pawnGroupMakers)
        {
            if (maker.kindDef != PawnGroupKindDefOf.Combat && maker.kindDef != PawnGroupKindDefOf.Settlement) continue;
            if (maker.options == null) continue;

            foreach (var option in maker.options)
            {
                if (option.kind?.combatPower > 0)
                {
                    kinds.Add(option.kind);
                }
            }
        }

        if (kinds.Count == 0 && fac.def.basicMemberKind != null)
        {
            kinds.Add(fac.def.basicMemberKind);
        }

        return kinds.OrderBy(k => k.combatPower).ToList();
    }

    Pawn GeneratePawn(PawnKindDef kind, Faction fac, int tile)
    {
        PawnGenerationRequest request = new PawnGenerationRequest(kind, fac, PawnGenerationContext.NonPlayer, tile, forceGenerateNewPawn: true, allowDead: false, allowDowned: false, canGeneratePawnRelations: false);
        return PawnGenerator.GeneratePawn(request);
    }
}
