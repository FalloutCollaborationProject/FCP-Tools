using RimWorld;
using System.Collections.Generic;
using Verse;

namespace FCP.Enlist;

public class BountyBoard : IExposable
{
    public List<GeneratedBounty> available = new List<GeneratedBounty>();
    public List<GeneratedBounty> accepted = new List<GeneratedBounty>();
    public int lastRefreshTick;

    public void ExposeData()
    {
        Scribe_Collections.Look(ref available, "available", LookMode.Deep);
        Scribe_Collections.Look(ref accepted, "accepted", LookMode.Deep);
        Scribe_Values.Look(ref lastRefreshTick, "lastRefreshTick");
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            available ??= new List<GeneratedBounty>();
            accepted ??= new List<GeneratedBounty>();
        }
    }
}

public class GeneratedBounty : IExposable
{
    public Quest quest;
    public int reward;

    public bool ReadyToCollect => quest != null && quest.State == QuestState.EndedSuccess;
    public bool Failed => quest == null
        || quest.State == QuestState.EndedFailed
        || quest.State == QuestState.EndedInvalid
        || quest.State == QuestState.EndedOfferExpired
        || quest.State == QuestState.EndedUnknownOutcome;

    public void ExposeData()
    {
        Scribe_References.Look(ref quest, "quest");
        Scribe_Values.Look(ref reward, "reward");
    }
}
