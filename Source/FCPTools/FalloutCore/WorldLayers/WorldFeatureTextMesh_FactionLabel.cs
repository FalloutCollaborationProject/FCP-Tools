using RimWorld.Planet;
using UnityEngine;

namespace FCP.Core
{
    public class WorldFeatureTextMesh_FactionLabel : WorldFeatureTextMesh_TextMeshPro
    {
        public void Initialize(Vector3 position, string factionName, 
            float size = 1f, Color? color = null)
        {
            Init();
            Text = factionName;
            LocalPosition = position;
            Size = size;
            Color = color ?? Color.white;
            
            Vector3 normalized = position.normalized;
            Quaternion rotation = Quaternion.LookRotation(
                Vector3.Cross(normalized, Vector3.up), 
                normalized);
            rotation *= Quaternion.Euler(Vector3.right * 90f);
            rotation *= Quaternion.Euler(Vector3.forward * 90f);
            
            Rotation = rotation;
            
            WrapAroundPlanetSurface();
            SetActive(true);
        }
    }
}