using Verse;

namespace FCP.Core.Backpacks;

public class CompProperties_WeaponBackpack : CompProperties
{
    public string backpackTexPath;
    public float backpackScale = 1f;
    public bool drawBackOnlyWhenDrafted = false;
    
    public CompProperties_WeaponBackpack()
    {
        compClass = typeof(CompWeaponBackpack);
    }
}
