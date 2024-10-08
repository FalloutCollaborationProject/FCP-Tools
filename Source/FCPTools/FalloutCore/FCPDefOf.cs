using RimWorld;
using Verse;
// ReSharper disable InconsistentNaming
// ReSharper disable UnassignedField.Global

namespace FCP.Core;

[DefOf]
public class FCPDefOf
{
    [MayRequire("Rick.FCP.Legion")]
    public static FactionDef FCP_Caesars_Legion;
    
    [MayRequire("Rick.FCP.NCR")]
    public static FactionDef FCP_Faction_NCR;
    
    public static PawnGroupKindDef FCP_PawnGroupKind_TaxCollector;
    
    static FCPDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(FCPDefOf));
    }
}