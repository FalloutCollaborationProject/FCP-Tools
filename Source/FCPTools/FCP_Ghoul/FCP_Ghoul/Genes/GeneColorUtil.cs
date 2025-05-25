using System.Collections.Generic;
using UnityEngine;

namespace FCP_Ghoul
{
    public static class GeneColorUtil
    {
        private static readonly Dictionary<string, Color> NamedColors = new()
        {
            {"Pale", new Color(0.8f, 0.8f, 0.7f)},
            {"Milky", new Color(0.75f, 0.75f, 0.65f)},
            {"Tarnished", new Color(0.6f, 0.6f, 0.5f)},
            {"Wheaten", new Color(0.7f, 0.65f, 0.5f)},
            {"Sepia", new Color(0.6f, 0.5f, 0.4f)},
            {"Ashen", new Color(0.5f, 0.5f, 0.5f)},
            {"Murky", new Color(0.4f, 0.5f, 0.5f)},
            {"Duskwither", new Color(0.5f, 0.55f, 0.6f)},
            {"Rotted", new Color(0.5f, 0.45f, 0.4f)},
            {"Sallow", new Color(0.65f, 0.7f, 0.4f)},
            {"Pustule", new Color(0.6f, 0.6f, 0.4f)},
            {"Oozing", new Color(0.8f, 0.7f, 0.4f)},
            {"Goldenrot", new Color(0.7f, 0.6f, 0.3f)},
            {"Jaundiced", new Color(0.7f, 0.7f, 0.3f)},
            {"Festered", new Color(0.55f, 0.65f, 0.5f)},
            {"Blighted", new Color(0.5f, 0.7f, 0.5f)},
            {"Spoiled", new Color(0.45f, 0.55f, 0.4f)},
            {"Sickly", new Color(0.6f, 0.75f, 0.6f)},
            {"Putrid", new Color(0.4f, 0.6f, 0.4f)},
            {"Mossy", new Color(0.3f, 0.5f, 0.3f)},
        };
        
        public static string ClosestColorName(Color color)
        {
            string bestMatch = "unknown";
            float smallestDistance = float.MaxValue;
            
            foreach (var kvp in NamedColors)
            {
                float distance = ColorDistance(color, kvp.Value);
                if (distance < smallestDistance)
                {
                    smallestDistance = distance;
                    bestMatch = kvp.Key;
                }
            }
            return bestMatch;
        }
        
        private static float ColorDistance(Color a, Color b)
        {
            float r = a.r - b.r;
            float g = a.g - b.g;
            float bVal = a.b - b.b;
            return r * r + g * g + bVal * bVal;
        }
    }
}