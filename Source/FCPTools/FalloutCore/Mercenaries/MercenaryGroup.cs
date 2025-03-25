using Verse;
using System.Collections.Generic;
using System.Linq;

namespace FCP.Core
{
    public class MercenaryGroup : IExposable
    {
        public string name;
        public List<Pawn> members = new List<Pawn>();
        public bool isActive = true;
        public MercenaryGroupType groupType = MercenaryGroupType.Combat;

        public void ExposeData()
        {
            Scribe_Values.Look(ref name, "name");
            Scribe_Collections.Look(ref members, "members", LookMode.Reference);
            Scribe_Values.Look(ref isActive, "isActive", true);
            Scribe_Values.Look(ref groupType, "groupType", MercenaryGroupType.Combat);
        }

        public enum MercenaryGroupType
        {
            Combat,
            Support,
            Specialized
        }
    }
}
