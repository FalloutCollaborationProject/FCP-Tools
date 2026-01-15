using RimWorld;
using System.Linq;
using Verse;

namespace BM_HeavyWeapon
{
    public class Verb_Shoot_ApparelAmmo : Verb_Shoot
    {
        public VerbProperties_ApparelAmmo Props => base.verbProps as VerbProperties_ApparelAmmo;
        public override bool Available()
        {
            var comp = CasterPawn.GetAvailableApparelAmmo(this, out _, out _);
            if (comp is null || !comp.CanBeUsed(out _))
            {
                return false;
            }
            return base.Available();
        }

        public override bool TryCastShot()
        {
            var pelletCount = Props.pelletCount;
            bool result = false;
            for (int i = 0; i < pelletCount; i++)
            {
                var resultTmp = base.TryCastShot();
                if (resultTmp)
                {
                    result = true;
                }
            }

            if (result)
            {
                var comp = CasterPawn.GetAvailableApparelAmmo(this, out _, out _);
                comp.UsedOnce();
            }
            return result;
        }
    }
}
