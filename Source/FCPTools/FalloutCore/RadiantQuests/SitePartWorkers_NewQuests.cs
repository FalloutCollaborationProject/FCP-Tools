using RimWorld.Planet;

namespace FCP.Core.RadiantQuests;

public class SitePartWorker_DefenseWaves : SitePartWorker
{
    public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
    {
        return base.GetPostProcessedThreatLabel(site, sitePart) + ": " + "DefenseWavesLabel".Translate();
    }

    public override SitePartParams GenerateDefaultParams(float myThreatPoints, PlanetTile tile, Faction faction)
    {
        SitePartParams parms = base.GenerateDefaultParams(myThreatPoints, tile, faction);
        parms.threatPoints = myThreatPoints;
        return parms;
    }
}

public class SitePartWorker_TechCache : SitePartWorker
{
    public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
    {
        return base.GetPostProcessedThreatLabel(site, sitePart) + ": " + "TechCacheLabel".Translate();
    }

    public override SitePartParams GenerateDefaultParams(float myThreatPoints, PlanetTile tile, Faction faction)
    {
        SitePartParams parms = base.GenerateDefaultParams(myThreatPoints, tile, faction);
        parms.threatPoints = myThreatPoints;
        return parms;
    }
}

public class SitePartWorker_WarlordStronghold : SitePartWorker
{
    public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
    {
        return base.GetPostProcessedThreatLabel(site, sitePart) + ": " + "WarlordStrongholdLabel".Translate();
    }

    public override SitePartParams GenerateDefaultParams(float myThreatPoints, PlanetTile tile, Faction faction)
    {
        SitePartParams parms = base.GenerateDefaultParams(myThreatPoints, tile, faction);
        parms.threatPoints = myThreatPoints * 1.25f;
        return parms;
    }
}
