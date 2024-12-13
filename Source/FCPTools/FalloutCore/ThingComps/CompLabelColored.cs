using UnityEngine;

namespace FCP.Core;

public class CompLabelColored : ThingComp
{
    CompProperties_LabelColored Props => (CompProperties_LabelColored)props;
    //public override string TransformLabel(string label)
    //{
    //    Log.Message("colourizing " + Props.rarity.ToString() + GetRarityColor(Props.rarity));
    //    return  label + "test".Colorize(GetRarityColor(Props.rarity));
    //}

    //public override string CompTipStringExtra()
    //{
    //    return GetRarityName(Props.rarity);
    //}

    public Color GetRarityColor()
    {
        return GetRarityColor(Props.rarity);
    }

    static Color GetRarityColor(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                return Common;
            case Rarity.Rare:
                return Rare;
            case Rarity.Unique:
                return Unique;

            default:
                return Color.white;
        }
    }


    public static Color Common => new Color(1f, 1f, 1f, 1f);

    public static Color Rare => new Color(0.23f, 0.17f, 1f, 1f);

    public static Color Unique => new Color(1f, 0.97f, 0.45f, 1f);


    static string GetRarityName(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                return "FCP_RarityCommon".Translate();
            case Rarity.Rare:
                return "FCP_RarityRare".Translate();
            case Rarity.Unique:
                return "FCP_RarityUnique".Translate();

            default:
                return string.Empty;
        }
    }
}
public enum Rarity
{
    Common,
    Rare,
    Unique,
}