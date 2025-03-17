using UnityEngine;

namespace FCP.Core
{
    public class ModExtension_FactionTerritory : DefModExtension
    {
        public Color territoryColor = new();
        public Color territoryBorderColor = new();
        public Color factionLabelColor = new();
        public int initialTerritoryRadius = 10;
        public bool renderTerritoryOverWater = false;
    }
}