using UnityEngine;
using Verse;

namespace FCP.Core.Backpacks;

public class CompProperties_WeaponBackpack : CompProperties
{
    public string backpackTexPath;
    public float backpackScale = 1f;
    public bool drawBackOnlyWhenDrafted = false;
    public Vector3 northOffset = new Vector3(0f, 0.04f, -0.25f);
    public Vector3 southOffset = new Vector3(0f, 0.04f, 0.25f);
    public Vector3 eastOffset = new Vector3(0.35f, 0.04f, -0.2f);
    
    public CompProperties_WeaponBackpack()
    {
        compClass = typeof(CompWeaponBackpack);
    }
}
