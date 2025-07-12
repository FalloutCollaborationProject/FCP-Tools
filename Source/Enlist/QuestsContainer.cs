using RimWorld;
using System.Collections.Generic;
using Verse;

namespace  FCP.Enlist
{
    public class QuestsContainer : IExposable
    {
        public List<Quest> quests;
        public QuestsContainer() { }
        public QuestsContainer(Quest quest)
        {
            quests = new List<Quest> { quest };
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref quests, "quests", LookMode.Reference);
        }
    }
}
