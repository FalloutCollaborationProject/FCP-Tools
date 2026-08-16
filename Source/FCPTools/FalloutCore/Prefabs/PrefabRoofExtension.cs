namespace FCP.Core;

public class PrefabRoofData
{
    public RoofDef def;
    public List<CellRect> rects = new List<CellRect>();
}

public class PrefabRoofExtension : DefModExtension
{
    public List<PrefabRoofData> roofs = new List<PrefabRoofData>();
}
