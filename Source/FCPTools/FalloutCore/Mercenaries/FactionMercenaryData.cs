using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace FCP.Core
{
    public class FactionMercenaryData : IExposable
    {
        public int lastPaymentTicks;
        public int missedPaymentCount;
        public int firstMissedPaymentTick = -1;
        public int hiringBlockedUntilTick = -1;
        public List<MercenaryGroup> groups = new List<MercenaryGroup>();
        public List<QueuedMercenaryArrival> reservedArrivals = new List<QueuedMercenaryArrival>();
        public List<MercenaryCaravanData> activeCaravans = new List<MercenaryCaravanData>();
        public List<Pawn> AllHiredPawns => groups.SelectMany(g => g.members).ToList();
        private List<MercenaryGroup> groupsWorkingList;
        public void ExposeData()
        {
            Scribe_Values.Look(ref lastPaymentTicks, "lastPaymentTicks");
            Scribe_Values.Look(ref missedPaymentCount, "missedPaymentCount");
            Scribe_Values.Look(ref firstMissedPaymentTick, "firstMissedPaymentTick", -1);
            Scribe_Values.Look(ref hiringBlockedUntilTick, "hiringBlockedUntilTick", -1);

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                groupsWorkingList = groups.Where(g => g != null).ToList();
                foreach (var group in groupsWorkingList)
                {
                    group.members.RemoveAll(p => p == null || p.Destroyed);
                }
            }
            Scribe_Collections.Look(ref groupsWorkingList, "groups", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.LoadingVars || Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                groups = groupsWorkingList ?? new List<MercenaryGroup>();
            }

            Scribe_Collections.Look(ref reservedArrivals, "reservedArrivals", LookMode.Deep);
            Scribe_Collections.Look(ref activeCaravans, "activeCaravans", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                groups ??= new List<MercenaryGroup>();
                reservedArrivals ??= new List<QueuedMercenaryArrival>();
                activeCaravans ??= new List<MercenaryCaravanData>();
            }
        }
    }
}
