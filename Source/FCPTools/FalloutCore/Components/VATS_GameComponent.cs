using FCP.Core;
using FCP.Core.VATS;

namespace FCP.Core;

public class VATS_GameComponent(Game game) : GameComponent
{
    public static Dictionary<Pawn, VATSAction> ActiveAttacks = new Dictionary<Pawn, VATSAction>();

    public static bool SlowMoActive;
    public static int SlowMoStarted = -1;
    public static Thing SlowMoCauser;
    public Game Game = game;

    public static void SetSlowMo(Thing slowMoCauser)
    {
        if (!FCPCoreMod.SettingsTab<VATSSettings>().enableSlowDownTime)
        {
            return;
        }

        SlowMoStarted = Current.Game.tickManager.TicksGame;
        SlowMoActive = true;
        SlowMoCauser = slowMoCauser;
    }

    public static void ResetSlowMo()
    {
        SlowMoStarted = -1;
        SlowMoActive = false;
        SlowMoCauser = null;
    }

    public override void ExposeData()
    {
        ActiveAttacks ??= new Dictionary<Pawn, VATSAction>();
        Scribe_Collections.Look(ref ActiveAttacks, "ActiveAttacks", LookMode.Reference, LookMode.Reference);
    }

    public override void GameComponentTick()
    {
        // Safety barrier to prevent getting stuck in slowmo
        if (SlowMoCauser == null || SlowMoCauser.Destroyed || (SlowMoStarted > 0 && SlowMoStarted + 600 < Current.Game.tickManager.TicksGame))
        {
            ResetSlowMo();
        }
    }

    public class VATSAction(Pawn p, BodyPartRecord b, ThingWithComps t, float chance, ShotReport shotReport)
    {
        public ThingWithComps Equipment = t;
        public float HitChance = chance;
        public BodyPartRecord Part = b;
        public ShotReport ShotReport = shotReport;
        public Pawn Target = p;
    }
}
