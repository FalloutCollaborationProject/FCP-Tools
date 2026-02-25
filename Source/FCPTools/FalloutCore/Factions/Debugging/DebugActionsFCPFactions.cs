using System.Text;
using FCP.Factions;
using LudeonTK;

namespace FCP.Core.Factions;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class DebugActionsFCPFactions
{
    public const string CategoryName = "FCP: Factions";
    
    [DebugAction(CategoryName, "Log Precept Relations", actionType = DebugActionType.Action)]
    private static void LogAlignmentPrecepts()
    {
        var defsWithExtension = DefDatabase<PreceptDef>.AllDefsListForReading
            .Where(def => def.HasModExtension<PreceptExtension_AlignmentRelation>())
            .Select(def => (def: def, ext: def.GetModExtension<PreceptExtension_AlignmentRelation>()) )
            .ToList();
        
        FCPLog.Message("Logging Precept Relations...");
        
        var sb = new StringBuilder();
        foreach ((PreceptDef def, PreceptExtension_AlignmentRelation ext) tuple in defsWithExtension)
        {
            sb.AppendLine($"Def: {tuple.def.defName}");

            foreach (PreceptAlignmentRelation relation in tuple.ext.alignments)
            {
                sb.AppendLine($" - {relation.precept.defName} -> {relation.alignment}");
            }
            
            Log.Message(sb.ToString());
            sb.Clear();
        }
    }
}