using UnityEngine;

namespace FCP.Factions;

public class FactionExtension_HasTerritory : DefModExtension
{
    public Color territoryColor = new();
    public Color territoryBorderColor = new();
    public Color factionLabelColor = new();
    public int initialTerritoryRadius = 10;
    public bool renderTerritoryOverWater = false;
}