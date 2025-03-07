using UnityEngine;

namespace FCP.Core
{
    [StaticConstructorOnStartup]
    public static class TextureCache
    {
        public static readonly Texture2D FactionTerritoryTex = 
            ContentFinder<Texture2D>.Get("WorldOverlays/FactionTerritoryTex");
        
        public static readonly Texture2D FactionTerritoryBorderTex = 
            ContentFinder<Texture2D>.Get("WorldOverlays/FactionTerritoryBorderTex");
    }
}