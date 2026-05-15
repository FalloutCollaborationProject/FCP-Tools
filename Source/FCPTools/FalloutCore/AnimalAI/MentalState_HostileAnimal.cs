using Verse;
using Verse.AI;

namespace FCP.Core;

public class MentalState_HostileAnimal : MentalState
{
    public override bool ForceHostileTo(Thing t)
    {
        if (t.def.race == null) return false;
        return t.def.race.Humanlike || t.def.race.IsMechanoid;
    }
}
