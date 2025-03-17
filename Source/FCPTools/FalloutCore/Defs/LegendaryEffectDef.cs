using FCP.Core.LegendaryEffectWorkers;

namespace FCP.Core;

public class LegendaryEffectDef : Def
{
    public List<StatModifier> statOffsets;
    public List<StatModifier> statFactors;
    public List<CompProperties> AdditionalComps;
    public bool IsForWeapon = false;
    public bool IsForMelee = false;
    public bool IsForApparel = false;
    public bool Stuns = false;
    public int StunDuration = 600;
    public float VATS_Multiplier = 1f;
    public Type workerClass = typeof(LegendaryEffectWorker);
    public HediffDef hediffToApply;

    [Unsaved(false)]
    private LegendaryEffectWorker workerInt;

    public List<StatModifier> StatOffsets => statOffsets ?? [];
    public List<StatModifier> StatFactors => statFactors ?? [];

    public LegendaryEffectWorker Worker
    {
        get
        {
            if (workerInt == null)
            {
                workerInt = (LegendaryEffectWorker)Activator.CreateInstance(workerClass);
                workerInt.InitSetEffect(this);
            }
            return workerInt;
        }
    }
}
