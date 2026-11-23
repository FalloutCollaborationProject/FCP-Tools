using FCP.Core;
using UnityEngine;

namespace FCP.Synths;

public class IncidentWorker_InstituteRelocationOffer : IncidentWorker
{
    private const int AcceptWindowDays = 4;

    private static InstituteSettings Settings => FCPCoreMod.SettingsTab<InstituteSettings>();

    protected override bool CanFireNowSub(IncidentParms parms)
    {
        if (Settings is not { relocationOfferEnabled: true })
            return false;

        if (parms.target is not Map map || !map.IsPlayerHome)
            return false;

        Faction institute = Find.FactionManager.FirstFactionOfDef(SynthDefOf.FCP_Faction_Institute);
        if (institute == null || institute.HostileTo(Faction.OfPlayer))
            return false;

        return GetEligiblePawns(map).Any();
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        InstituteSettings settings = Settings;
        if (settings is not { relocationOfferEnabled: true })
            return false;

        Map map = (Map)parms.target;
        Faction institute = Find.FactionManager.FirstFactionOfDef(SynthDefOf.FCP_Faction_Institute);
        if (institute == null)
            return false;

        List<Pawn> candidates = GetEligiblePawns(map).ToList();
        if (candidates.Count == 0)
            return false;

        Pawn target = candidates.RandomElementByWeight(ScoreFor);

        ChoiceLetter_InstituteRelocationOffer letter = (ChoiceLetter_InstituteRelocationOffer)LetterMaker.MakeLetter(
            "FCP_InstituteRelocation_Title".Translate(),
            "FCP_InstituteRelocation_Text".Translate(target.LabelShortCap, institute.Name),
            SynthDefOf.FCP_Letter_InstituteRelocationOffer);

        letter.target = target;
        letter.map = map;
        letter.rewardMarketValue = Mathf.RoundToInt(new FloatRange(settings.relocationRewardMin, settings.relocationRewardMax).RandomInRange);
        letter.goodwillGain = settings.relocationGoodwillGain;
        letter.relatedFaction = institute;
        letter.radioMode = true;
        letter.StartTimeout(AcceptWindowDays * GenDate.TicksPerDay);

        Find.LetterStack.ReceiveLetter(letter);
        return true;
    }

    private static IEnumerable<Pawn> GetEligiblePawns(Map map)
    {
        return map.mapPawns.FreeColonists.Where(p =>
            p.DevelopmentalStage == DevelopmentalStage.Adult &&
            !p.health.hediffSet.HasHediff(SynthDefOf.FCP_Hediff_SynthComponents_GenIII));
    }

    private static float ScoreFor(Pawn pawn)
    {
        float score = 1f;
        score += pawn.skills?.GetSkill(SkillDefOf.Intellectual)?.Level ?? 0;
        score += pawn.skills?.GetSkill(SkillDefOf.Medicine)?.Level ?? 0;
        score += pawn.skills?.GetSkill(SkillDefOf.Crafting)?.Level ?? 0;
        return score;
    }
}
