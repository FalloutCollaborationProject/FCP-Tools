using FCP.Core;

namespace FCP.Synths;

public static class SynthReplacementUtility
{
    private static InstituteSettings Settings => FCPCoreMod.SettingsTab<InstituteSettings>();

    public static void TryReplaceReturningPawns(List<Pawn> pawns, Map map)
    {
        if (pawns.NullOrEmpty() || map == null || !map.IsPlayerHome)
            return;

        foreach (Pawn pawn in pawns)
            TryReplace(pawn);
    }

    public static void TryReplace(Pawn pawn)
    {
        InstituteSettings settings = Settings;
        if (settings is not { synthReplacementEnabled: true })
            return;

        if (!IsEligible(pawn))
            return;

        if (!Rand.Chance(settings.synthReplacementChance))
            return;

        MarkAsSynth(pawn);
    }

    private static bool IsEligible(Pawn pawn)
    {
        if (pawn == null || pawn.Dead || pawn.health?.hediffSet == null)
            return false;

        if (!pawn.RaceProps.Humanlike || pawn.Faction != Faction.OfPlayer || !pawn.IsFreeColonist)
            return false;

        if (Find.FactionManager.FirstFactionOfDef(SynthDefOf.FCP_Faction_Institute) == null)
            return false;

        XenotypeDef xenotype = pawn.genes?.Xenotype;
        if (xenotype == SynthDefOf.FCP_Xenotype_Synth_GenI || xenotype == SynthDefOf.FCP_Xenotype_Synth_GenII)
            return false;

        return !pawn.health.hediffSet.HasHediff(SynthDefOf.FCP_Hediff_SynthComponents_GenIII);
    }

    private static void MarkAsSynth(Pawn pawn)
    {
        pawn.health.AddHediff(SynthDefOf.FCP_Hediff_SynthComponents_GenIII);

        Find.LetterStack.ReceiveLetter(
            "FCP_Institute_SynthReplacement_Title".Translate(pawn.LabelShortCap),
            "FCP_Institute_SynthReplacement_Text".Translate(pawn.LabelShortCap),
            LetterDefOf.NeutralEvent,
            pawn);
    }
}
