using System.Xml;

namespace FCP.PocketMaps;

public class ThingsContainer
{
    public List<PrefabItem> items = new List<PrefabItem>();

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        foreach (XmlNode thingNode in xmlRoot.ChildNodes)
        {
            if (thingNode.NodeType != XmlNodeType.Element) continue;
            if (!ModsSatisfied(thingNode)) continue;

            string defName = thingNode.Name;
            string stuff = thingNode["stuff"]?.InnerText;
            string rotation = thingNode["relativeRotation"]?.InnerText;
            float chance = ParseFloat(thingNode["chance"]?.InnerText, 1f);
            bool fullFuel = thingNode["fullFuel"]?.InnerText == "true";

            XmlNode rectsNode = thingNode["rects"];
            XmlNode positionsNode = thingNode["positions"];
            XmlNode positionNode = thingNode["position"];

            if (rectsNode != null)
            {
                foreach (XmlNode rectNode in rectsNode.ChildNodes)
                {
                    if (rectNode.NodeType != XmlNodeType.Element) continue;
                    items.Add(new PrefabItem {
                        thingDefName = defName,
                        stuffDefName = stuff,
                        rect = rectNode.InnerText,
                        relativeRotation = rotation,
                        chance = chance,
                        fullFuel = fullFuel
                    });
                }
            }
            else if (positionsNode != null)
            {
                foreach (XmlNode posNode in positionsNode.ChildNodes)
                {
                    if (posNode.NodeType != XmlNodeType.Element) continue;
                    items.Add(CreateItem(thingNode, defName, stuff, posNode.InnerText, rotation, chance, fullFuel));
                }
            }
            else
            {
                string pos = positionNode?.InnerText ?? "(0,0,0)";
                items.Add(CreateItem(thingNode, defName, stuff, pos, rotation, chance, fullFuel));
            }
        }
    }

    private PrefabItem CreateItem(XmlNode node, string defName, string stuff, string pos, string rot, float chance, bool fullFuel)
    {
        return new PrefabItem {
            thingDefName = defName,
            stuffDefName = stuff,
            position = pos,
            relativeRotation = rot,
            chance = chance,
            hp = ParseInt(node["hp"]?.InnerText),
            quality = node["quality"]?.InnerText,
            fullFuel = fullFuel
        };
    }

    private int? ParseInt(string s)
    {
        return int.TryParse(s, out int v) ? v : (int?)null;
    }

    private float ParseFloat(string s, float def)
    {
        return float.TryParse(s, out float v) ? v : def;
    }

    internal static bool ModsSatisfied(XmlNode node)
    {
        string mayRequire = node.Attributes?["MayRequire"]?.Value;
        if (!mayRequire.NullOrEmpty() && !ModLister.AllModsActiveNoSuffix(mayRequire.ToLower().Split(',')))
            return false;

        string mayRequireAnyOf = node.Attributes?["MayRequireAnyOf"]?.Value;
        if (!mayRequireAnyOf.NullOrEmpty() && !ModLister.AnyModActiveNoSuffix(mayRequireAnyOf.ToLower().Split(',')))
            return false;

        return true;
    }
}

public class PawnKindsContainer
{
    public List<PrefabPawn> items = new List<PrefabPawn>();

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        foreach (XmlNode node in xmlRoot.ChildNodes)
        {
            if (node.NodeType != XmlNodeType.Element) continue;
            if (!ThingsContainer.ModsSatisfied(node)) continue;

            int count = int.TryParse(node.InnerText, out int c) ? c : 1;
            items.Add(new PrefabPawn { pawnKindDefName = node.Name, count = count });
        }
    }
}

public class PocketMapPrefabDef : Def
{
    public string size;
    public TerrainDef floorDef;
    public MapGeneratorDef mapGeneratorDef;
    public FactionDef factionDef;
    public ThingsContainer things;
    public PawnKindsContainer pawnKinds;

    public override void ResolveReferences()
    {
        base.ResolveReferences();

        if (things?.items != null)
        {
            foreach (var item in things.items)
            {
                if (!string.IsNullOrEmpty(item.thingDefName))
                {
                    item.thingDef = DefDatabase<ThingDef>.GetNamed(item.thingDefName, false);
                    if (item.thingDef == null)
                        Log.Warning($"{defName}: ThingDef '{item.thingDefName}' not found");
                }

                if (!string.IsNullOrEmpty(item.stuffDefName))
                    item.stuff = DefDatabase<ThingDef>.GetNamed(item.stuffDefName, false);
            }
        }

        if (pawnKinds?.items != null)
        {
            foreach (var pawn in pawnKinds.items)
            {
                if (!string.IsNullOrEmpty(pawn.pawnKindDefName))
                {
                    pawn.pawnKindDef = DefDatabase<PawnKindDef>.GetNamed(pawn.pawnKindDefName, false);
                    if (pawn.pawnKindDef == null)
                        Log.Warning($"{defName}: PawnKindDef '{pawn.pawnKindDefName}' not found");
                }
            }
        }
    }
}

public class PrefabItem
{
    public string thingDefName;
    public string stuffDefName;
    public ThingDef thingDef;
    public ThingDef stuff;
    public string position;
    public string rect;
    public int? hp;
    public string quality;
    public string relativeRotation;
    public float chance = 1f;
    public bool fullFuel;
}

public class PrefabPawn
{
    public string pawnKindDefName;
    public PawnKindDef pawnKindDef;
    public int count = 1;
}