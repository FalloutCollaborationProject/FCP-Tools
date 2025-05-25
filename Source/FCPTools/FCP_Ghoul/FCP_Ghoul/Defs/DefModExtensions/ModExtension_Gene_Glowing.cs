using JetBrains.Annotations;
using Verse;

namespace FCP_Ghoul
{
    [UsedImplicitly]
    public class ModExtension_Gene_Glowing : DefModExtension
    {
        public FloatRange redRange = new(0f, 1f);
        public FloatRange greenRange = new(0f, 1f);
        public FloatRange blueRange = new(0f, 1f);
    }
}