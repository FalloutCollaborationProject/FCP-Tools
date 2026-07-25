using RimWorld;
using Verse;

namespace FCP.Core.RadiantQuests;

public class QuestPart_WatchPawnsSkillAtLeast : QuestPart
{
    public List<Pawn> pawns = new List<Pawn>();
    public SkillDef skill;
    public int level;
    public string outSignalComplete;

    internal WatchPawnsSkillComp comp;

    public void StartWatching()
    {
        if (comp != null)
        {
            return;
        }
        comp = new WatchPawnsSkillComp(Current.Game) { questPart = this };
        Current.Game.components.Add(comp);
    }

    public void Complete()
    {
        if (!outSignalComplete.NullOrEmpty())
        {
            Find.SignalManager.SendSignal(new Signal(outSignalComplete));
        }
    }

    public override void Cleanup()
    {
        base.Cleanup();
        if (comp != null)
        {
            Current.Game.components.Remove(comp);
            comp = null;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
        Scribe_Defs.Look(ref skill, "skill");
        Scribe_Values.Look(ref level, "level");
        Scribe_Values.Look(ref outSignalComplete, "outSignalComplete");

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            comp = Current.Game?.GetComponent<WatchPawnsSkillComp>();
            if (comp != null && comp.questPart == null)
            {
                comp.questPart = this;
            }
        }
    }
}

public class WatchPawnsSkillComp : GameComponent
{
    public QuestPart_WatchPawnsSkillAtLeast questPart;

    private const int CheckIntervalTicks = 250;
    private int ticksUntilNextCheck;

    public WatchPawnsSkillComp()
    {
    }

    public WatchPawnsSkillComp(Game game)
    {
    }

    public override void GameComponentTick()
    {
        base.GameComponentTick();

        if (questPart == null)
        {
            Current.Game.components.Remove(this);
            return;
        }

        ticksUntilNextCheck--;
        if (ticksUntilNextCheck > 0)
        {
            return;
        }
        ticksUntilNextCheck = CheckIntervalTicks;

        if (questPart.skill == null || questPart.pawns.NullOrEmpty())
        {
            return;
        }

        foreach (Pawn pawn in questPart.pawns)
        {
            SkillRecord record = pawn?.skills?.GetSkill(questPart.skill);
            if (record == null || record.Level < questPart.level)
            {
                return;
            }
        }

        questPart.Complete();
        Current.Game.components.Remove(this);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref ticksUntilNextCheck, "ticksUntilNextCheck");
    }
}
