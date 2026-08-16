using UnityEngine;

namespace FCPVT;

public static class AssetsUtil
{
    public static Texture2D GetNumberTexture(int num)
    {
        return num switch
        {
            0 => Assets.Num_0,
            1 => Assets.Num_1,
            2 => Assets.Num_2,
            3 => Assets.Num_3,
            4 => Assets.Num_4,
            5 => Assets.Num_5,
            6 => Assets.Num_6,
            7 => Assets.Num_7,
            8 => Assets.Num_8,
            9 => Assets.Num_9,
            _ => null
        };
    }
}
