namespace FCP.Core;

public class SleepingEffects : IExposable
{
    public string tentDefName;
    public string hediffDefName;
    public string label;
    public bool negateWater;
    public bool negateSleptOutside;
    public bool negateSleptInCold;
    public bool negateSleptInHeat;
    public bool negateSleptInBarracks;
    public bool ideologyTentAssignmentAllowed;
    public float fuelCapacity;
    public float fuelConsumptionRate;
    public bool fuelEnabled;

    public void ExposeData()
    {
        Scribe_Values.Look(ref tentDefName, "tentDefName");
        Scribe_Values.Look(ref hediffDefName, "hediffDefName");
        Scribe_Values.Look(ref negateWater, "negateWater");
        Scribe_Values.Look(ref negateSleptOutside, "negateSleptOutside");
        Scribe_Values.Look(ref negateSleptInCold, "negateSleptInCold");
        Scribe_Values.Look(ref negateSleptInHeat, "negateSleptInHeat");
        Scribe_Values.Look(ref negateSleptInBarracks, "negateSleptInBarracks");
        Scribe_Values.Look(ref ideologyTentAssignmentAllowed, "ideologyTentAssignmentAllowed");
        Scribe_Values.Look(ref fuelCapacity, "fuelCapacity");
        Scribe_Values.Look(ref fuelConsumptionRate, "fuelConsumptionRate");
        Scribe_Values.Look(ref fuelEnabled, "fuelEnabled", true);
    }
}