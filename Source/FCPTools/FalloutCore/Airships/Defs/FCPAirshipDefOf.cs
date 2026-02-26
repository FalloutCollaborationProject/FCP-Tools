namespace FCP.Core;

[DefOf]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public static class FCPAirshipDefOf
{ 
    public static WorldObjectDef FCP_BOS_Airship;
    
    static FCPAirshipDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(FCPAirshipDefOf));
    }
}