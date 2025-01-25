using System.Collections.Generic;
using Verse;


namespace FCP.Core
{
    public abstract class DamageWithFilth : DamageWorker_AddInjury
    {
        public static Dictionary<Thing, DamageInfo> curDinfo = new Dictionary<Thing, DamageInfo>();

        public abstract string FilthToSpawn { get; }

        public override DamageResult Apply(DamageInfo dinfo, Thing thing)
        {
            curDinfo[thing] = new DamageInfo(dinfo);
            DamageResult damageResult = base.Apply(dinfo, thing);
            curDinfo.Remove(thing);
            return damageResult;
        }
    }
}
