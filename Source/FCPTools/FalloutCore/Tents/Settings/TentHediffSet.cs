namespace FCP.Core;

public class TentHediffSet : IExposable
{
    public string hediffDefName;
    public string label;
    public float comfyTemperatureMin;
    public float comfyTemperatureMax;

    public void ExposeData()
    {
        Scribe_Values.Look(ref hediffDefName, "hediffDefName");
        Scribe_Values.Look(ref comfyTemperatureMin, "comfyTemperatureMin");
        Scribe_Values.Look(ref comfyTemperatureMax, "comfyTemperatureMax");
    }
}