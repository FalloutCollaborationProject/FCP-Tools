using RimWorld;
using RimWorld.Planet;

namespace FCP.Core.RadiantQuests;

public class SitePartWorker_VertibirdCrash : SitePartWorker
{
    public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
    {
        return base.GetPostProcessedThreatLabel(site, sitePart) + ": Multiple combat waves";
    }
}
