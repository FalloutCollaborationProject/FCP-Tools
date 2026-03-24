using Verse;

namespace FCP.Core
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class TechprintExtension : DefModExtension
    {
        public string baseLabel;
        public string baseDescription;
        public string texPath;
        public ThingDef requiredBench;
    }
}