namespace FCP.Core;

public class ChoiceLetter_Squatters : ChoiceLetter
{
    public List<Pawn> squatters = new List<Pawn>();

    public override IEnumerable<DiaOption> Choices
    {
        get
        {
            if (!squatters.Any(p => p.Spawned))
            {
                yield return DiaOption.DefaultOK;
                yield break;
            }

            yield return new DiaOption("FCP_Squatters_LetStay".Translate())
            {
                action = LetStay,
                resolveTree = true
            };

            yield return new DiaOption("FCP_Squatters_TurnAway".Translate())
            {
                action = TurnAway,
                resolveTree = true
            };

            yield return new DiaOption("FCP_Squatters_DriveOff".Translate())
            {
                action = DriveOff,
                resolveTree = true
            };
        }
    }

    private void LetStay()
    {
        foreach (Pawn pawn in squatters)
        {
            if (!pawn.Spawned)
                continue;
            pawn.SetFaction(Faction.OfPlayer);
        }
        Messages.Message("FCP_Squatters_LetStay_Message".Translate(squatters.Count), MessageTypeDefOf.PositiveEvent);
    }

    private void TurnAway()
    {
        foreach (Pawn pawn in squatters)
        {
            if (pawn.Spawned)
                pawn.mindState.exitMapAfterTick = Find.TickManager.TicksGame + 1;
        }
    }

    private void DriveOff()
    {
        foreach (Pawn pawn in squatters)
        {
            if (pawn.Spawned)
                pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk);
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref squatters, "squatters", LookMode.Reference);
    }
}
