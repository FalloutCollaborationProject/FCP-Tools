using UnityEngine;

namespace FCP.Core;

// Based on Caravan_Tweener
public class AirshipTweener
{
    private readonly Airship airship;

    private const float SpringTightness = 0.09f;

    public Vector3 TweenedPos { get; private set; } = Vector3.zero;

    public AirshipTweener(Airship airship)
    {
        this.airship = airship;
    }

    public void TweenerTickInterval(int delta)
    {
        Vector3 target = airship.TargetPos;
        TweenedPos += (target - TweenedPos) * SpringTightness;
    }

    public void ResetToTarget()
    {
        TweenedPos = airship.TargetPos;
    }
}
