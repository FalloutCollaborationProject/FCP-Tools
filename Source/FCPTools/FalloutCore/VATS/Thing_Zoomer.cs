﻿namespace FCP.Core.VATS;

public class Thing_Zoomer : ThingWithComps
{
    public int startTicks = -1;

    public override void PostMake()
    {
        base.PostMake();
        startTicks = Find.TickManager.TicksGame;
    }

    protected override void Tick()
    {
        base.Tick();

        if (startTicks < 0)
        {
            return;
        }

        if (startTicks + FCPCoreMod.Settings.ZoomTimeout < Find.TickManager.TicksGame)
        {
            Destroy();
        }
    }
}
