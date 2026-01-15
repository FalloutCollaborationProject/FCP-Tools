using Verse;

namespace StimPacks.ModConfig;

public class Configs : ModSettings
{
    public bool AutoStim = true;
    public bool TeetotalerAutoStim = false;
    public override void ExposeData()
    {
        Scribe_Values.Look(ref AutoStim, "MaxLevel", true);
        Scribe_Values.Look(ref TeetotalerAutoStim, "TeetotalerAutoStim", false);
        base.ExposeData();
    }
}
