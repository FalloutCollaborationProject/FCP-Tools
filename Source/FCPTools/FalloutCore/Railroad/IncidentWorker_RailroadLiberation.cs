namespace FCP.Railroad;

public class IncidentWorker_RailroadLiberation : IncidentWorker
{
    private const int AcceptWindowDays = 4;

    protected override bool CanFireNowSub(IncidentParms parms)
    {
        if (parms.target is not Map map || !map.IsPlayerHome)
            return false;

        Faction railroad = Find.FactionManager.FirstFactionOfDef(RailroadDefOf.FCP_Faction_Railroad);
        if (railroad == null || railroad.HostileTo(Faction.OfPlayer))
            return false;

        return SynthDetectionUtility.GetHiddenSynthColonists(map).Any();
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        Map map = (Map)parms.target;
        Faction railroad = Find.FactionManager.FirstFactionOfDef(RailroadDefOf.FCP_Faction_Railroad);
        if (railroad == null)
            return false;

        Pawn target = SynthDetectionUtility.GetHiddenSynthColonists(map).RandomElementWithFallback(null);
        if (target == null)
            return false;

        ChoiceLetter_RailroadLiberation letter = (ChoiceLetter_RailroadLiberation)LetterMaker.MakeLetter(
            "FCP_RailroadLiberation_Title".Translate(),
            "FCP_RailroadLiberation_Text".Translate(target.LabelShortCap),
            RailroadDefOf.FCP_Letter_RailroadLiberation);

        letter.target = target;
        letter.map = map;
        letter.relatedFaction = railroad;
        letter.radioMode = true;
        letter.StartTimeout(AcceptWindowDays * GenDate.TicksPerDay);

        Find.LetterStack.ReceiveLetter(letter);
        return true;
    }
}
