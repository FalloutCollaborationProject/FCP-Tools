//namespace FCP.Tents;

namespace Tent;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class TentModExtension : DefModExtension
{
    public bool negateWater = false;
    public bool negateSleptOutside = false;
    public bool negateSleptInCold = false;
    public bool negateSleptInHeat = false;
    public bool negateSleptInBarracks = false;
    public bool ideologyTentAssignmentAllowed = false;
    public HediffDef customHediff = null;
}