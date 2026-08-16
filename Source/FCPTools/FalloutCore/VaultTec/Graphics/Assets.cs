using UnityEngine;

namespace FCPVT;

[StaticConstructorOnStartup]
public static class Assets
{
    public static readonly Texture2D RVD_SingleTex = ContentFinder<Texture2D>.Get("Things/Buildings/DoorsSingle/RollingVaultDoor_1x1_Blueprint", false);
    public static readonly Texture2D RVD_DoubleTex = ContentFinder<Texture2D>.Get("Things/Buildings/DoorsDouble/RollingVaultDoor_2x2_Blueprint", false);

    public static readonly Texture2D Num_0 = ContentFinder<Texture2D>.Get("Things/Overlays/Num_0", false);
    public static readonly Texture2D Num_1 = ContentFinder<Texture2D>.Get("Things/Overlays/Num_1", false);
    public static readonly Texture2D Num_2 = ContentFinder<Texture2D>.Get("Things/Overlays/Num_2", false);
    public static readonly Texture2D Num_3 = ContentFinder<Texture2D>.Get("Things/Overlays/Num_3", false);
    public static readonly Texture2D Num_4 = ContentFinder<Texture2D>.Get("Things/Overlays/Num_4", false);
    public static readonly Texture2D Num_5 = ContentFinder<Texture2D>.Get("Things/Overlays/Num_5", false);
    public static readonly Texture2D Num_6 = ContentFinder<Texture2D>.Get("Things/Overlays/Num_6", false);
    public static readonly Texture2D Num_7 = ContentFinder<Texture2D>.Get("Things/Overlays/Num_7", false);
    public static readonly Texture2D Num_8 = ContentFinder<Texture2D>.Get("Things/Overlays/Num_8", false);
    public static readonly Texture2D Num_9 = ContentFinder<Texture2D>.Get("Things/Overlays/Num_9", false);
}
