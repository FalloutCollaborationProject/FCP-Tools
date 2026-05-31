using Verse;

namespace FCP.Core;

public class CompProperties_UsableOnce : CompProperties_Usable
{
    public CompProperties_UsableOnce()
    {
        compClass = typeof(CompUsableOnce);
    }
}
