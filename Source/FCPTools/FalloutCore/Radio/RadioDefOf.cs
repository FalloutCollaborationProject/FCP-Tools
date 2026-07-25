namespace FCP.Core.Radio;

[DefOf]
public class RadioDefOf
{
    public static JobDef FCP_RadioTuneStation;
    public static JobDef FCP_RadioCallCaravan;
    public static JobDef FCP_RadioCallReinforcements;
    public static JobDef FCP_RadioGatherIntel;

    static RadioDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(RadioDefOf));
    }
}
