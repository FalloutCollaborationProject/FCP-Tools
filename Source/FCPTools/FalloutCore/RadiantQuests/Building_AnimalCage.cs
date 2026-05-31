using Verse;

namespace FCP.Core.RadiantQuests;

public class Building_AnimalCage : Building
{
    private Graphic cachedGraphicClosed;
    private Graphic cachedGraphicOpen;
    private bool lastOccupiedState;

    private CompAnimalCage CageComp => GetComp<CompAnimalCage>();

    public override Graphic Graphic
    {
        get
        {
            bool hasOccupant = CageComp?.Occupant != null;
            if (hasOccupant != lastOccupiedState)
            {
                cachedGraphicClosed = null;
                cachedGraphicOpen = null;
                lastOccupiedState = hasOccupant;
            }

            if (hasOccupant)
            {
                if (cachedGraphicClosed == null)
                {
                    cachedGraphicClosed = def.graphicData.GraphicColoredFor(this);
                }
                return cachedGraphicClosed;
            }

            if (cachedGraphicOpen == null)
            {
                string openPath = def.graphicData.texPath.Replace("Closed", "Open").Replace("closed", "open");
                GraphicData openData = new GraphicData();
                openData.CopyFrom(def.graphicData);
                openData.texPath = openPath;
                cachedGraphicOpen = openData.GraphicColoredFor(this);
            }
            return cachedGraphicOpen;
        }
    }
}
