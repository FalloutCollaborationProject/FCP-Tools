using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;

namespace FCP.Core.RadiantQuests
{
        public class WorkGiver_AnimalCageRefuel : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public virtual JobDef JobStandard => DefOfs.FCP_RefuelAnimalCage;

        public virtual JobDef JobAtomic => DefOfs.FCP_RefuelAnimalCage_Atomic;

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map.listerBuildings.allBuildingsColonist.Where(c => c.HasComp<CompAnimalCage>());
        }

        public virtual bool CanRefuelThing(Thing t)
        {
            return !(t is Building_Turret);
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return CanRefuelThing(t) && AnimalCageUtility.CanRefuel(pawn, t, forced);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return AnimalCageUtility.RefuelJob(pawn, t, forced, JobStandard, JobAtomic);
        }
    }
}

