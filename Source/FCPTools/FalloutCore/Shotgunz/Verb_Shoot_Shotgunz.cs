namespace FCP.Core;

public class Verb_Shoot_Shotgunz : Verb_Shoot
{
    protected override bool TryCastShot()
    {
        bool flag = base.TryCastShot();
        Verb_Properties_Shotgunz properties = (Verb_Properties_Shotgunz)verbProps;
        if (flag && properties.pellets - 1 > 0)
        {
            for (int i = 1; i < properties.pellets; i++)
            {
                base.TryCastShot();
            }
        }
        return flag;
    }
}
