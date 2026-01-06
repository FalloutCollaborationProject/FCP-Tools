using System.Xml;
using UnityEngine;

namespace RangerRick_PowerArmor;

public class ThingDefFloatClass
{
    public ThingDef thingDef;

    public float count = 1f;

    public ThingDefFloatClass()
    {
    }

    public ThingDefFloatClass(ThingDef thingDef, float count)
    {
        this.thingDef = thingDef;
        this.count = count;
    }

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        if (xmlRoot.ChildNodes.Count != 1)
        {
            Log.Error("Misconfigured ThingDefFloatClass: " + xmlRoot.OuterXml);
            return;
        }
        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thingDef", xmlRoot.Name);
        count = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
    }

    public override string ToString()
    {
        return "(" + ((thingDef != null) ? thingDef.defName : "null") + ", count=" + count.ToString("0.##") + ")";
    }

    public IngredientCount ToIngredientCount()
    {
        IngredientCount ingredientCount = new IngredientCount();
        ingredientCount.SetBaseCount(Mathf.CeilToInt(count));
        ingredientCount.filter.SetAllow(thingDef, allow: true);
        return ingredientCount;
    }
}
