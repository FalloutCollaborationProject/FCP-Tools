using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FCP.Core
{
    public class CompLabelColored : ThingComp
    {
        CompProperties_LabelColored Props => (CompProperties_LabelColored)props;
        public override string TransformLabel(string label)
        {
            return label.Colorize(GetRarityColor(Props.rarity));
        }

        public override string CompTipStringExtra()
        {
            return GetRarityName(Props.rarity);
        }

        static Color GetRarityColor(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Common:
                    return Color.white;
                case Rarity.Rare:
                    return new Color(0, 102, 255);
                case Rarity.Unique:
                    return new Color(255, 255, 51);

                default:
                    return Color.white;
            }
        }

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
}
