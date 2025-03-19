using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace FCP.Core
{
    [HarmonyPatch(typeof(ThingDefGenerator_Techprints), nameof(ThingDefGenerator_Techprints.ImpliedTechprintDefs))]
    public static class ThingDefGenerator_Techprints_Patch
    {
        public static IEnumerable<ThingDef> Postfix(IEnumerable<ThingDef> __result)
        {
            foreach (var thingDef in __result)
            {
                CompProperties_Techprint compProps = thingDef.GetCompProperties<CompProperties_Techprint>();
                if (compProps != null && compProps.project != null)
                {
                    var project = compProps.project;
                    var extension = project.GetModExtension<TechprintExtension>();
                    if (extension != null)
                    {
                        thingDef.label = extension.baseLabel.Translate(NamedArgumentUtility.Named(project, "PROJECT"));
                        thingDef.description = extension.baseDescription.Translate(NamedArgumentUtility.Named(project, "PROJECT")) + "\n\n" + project.LabelCap + "\n\n" + project.description;
                        thingDef.graphicData.texPath = extension.texPath;
                    }
                }
                yield return thingDef;
            }
        }
    }
}