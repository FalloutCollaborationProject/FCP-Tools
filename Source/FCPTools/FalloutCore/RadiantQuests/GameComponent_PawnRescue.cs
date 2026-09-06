namespace FCP.Core.RadiantQuests;

public class GameComponent_PawnRescue : GameComponent
{
    private List<Pawn> prisonersWillingJoin = new List<Pawn>();
    private static GameComponent_PawnRescue instance;

    public static GameComponent_PawnRescue Instance
    {
        get
        {
            if (instance == null)
                instance = Current.Game.GetComponent<GameComponent_PawnRescue>();
            return instance;
        }
    }

    public GameComponent_PawnRescue(Game game)
    {
        instance = this;
    }

    public void MarkWillingToJoin(Pawn pawn)
    {
        if (!prisonersWillingJoin.Contains(pawn))
            prisonersWillingJoin.Add(pawn);
    }

    public bool IsWillingToJoin(Pawn pawn) => prisonersWillingJoin.Contains(pawn);

    public void ClearWillingToJoin(Pawn pawn) => prisonersWillingJoin.Remove(pawn);

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref prisonersWillingJoin, "prisonersWillingJoin", LookMode.Reference);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            prisonersWillingJoin ??= new List<Pawn>();
            prisonersWillingJoin.RemoveAll(p => p == null || p.Destroyed);
        }
    }
}
