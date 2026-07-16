using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FCP.Core.Robotics
{
    public class CompProperties_EyebotExplorer : CompProperties
    {
        public CompProperties_EyebotExplorer()
        {
            compClass = typeof(CompEyebotExplorer);
        }
    }

    public class CompEyebotExplorer : ThingComp
    {
        private HashSet<int> reportedTiles = new HashSet<int>();

        public int NextScanTick;

        public CompProperties_EyebotExplorer Props => (CompProperties_EyebotExplorer)props;

        public void ScanForSites(int radius)
        {
            Map map = parent.Map;
            if (map?.Parent == null)
            {
                return;
            }

            PlanetTile homeTile = map.Parent.Tile;
            foreach (Site site in Find.WorldObjects.AllWorldObjects.OfType<Site>())
            {
                int tileId = (int)site.Tile;
                if (reportedTiles.Contains(tileId))
                {
                    continue;
                }
                if (Find.WorldGrid.TraversalDistanceBetween(homeTile, site.Tile, true, radius) > radius)
                {
                    continue;
                }

                reportedTiles.Add(tileId);
                Messages.Message("FCP_EyebotFoundSite".Translate(parent.LabelShort, site.LabelCap), site, MessageTypeDefOf.PositiveEvent);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref reportedTiles, "reportedTiles", LookMode.Value);
            Scribe_Values.Look(ref NextScanTick, "nextScanTick");
            reportedTiles ??= new HashSet<int>();
        }
    }
}
