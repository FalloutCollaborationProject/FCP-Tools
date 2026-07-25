using RimWorld;
using UnityEngine;
using Verse;

namespace FCP.Core.RadiantQuests;

public class QuestPart_WatchItemCountOnMap : QuestPart
{
    public Map map;
    public ThingDef itemDef;
    public int requiredCount;
    public string outSignalComplete;

    internal WatchItemCountComp comp;

    public void StartWatching()
    {
        if (comp != null)
        {
            return;
        }
        comp = new WatchItemCountComp(Current.Game) { questPart = this };
        Current.Game.components.Add(comp);
    }

    public void Complete()
    {
        int remaining = requiredCount;
        foreach (Thing stack in map.listerThings.ThingsOfDef(itemDef).ToList())
        {
            if (remaining <= 0)
            {
                break;
            }
            int take = Mathf.Min(remaining, stack.stackCount);
            remaining -= take;
            stack.SplitOff(take).Destroy();
        }

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
        Scribe_References.Look(ref map, "map");
        Scribe_Defs.Look(ref itemDef, "itemDef");
        Scribe_Values.Look(ref requiredCount, "requiredCount");
        Scribe_Values.Look(ref outSignalComplete, "outSignalComplete");

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            comp = Current.Game?.GetComponent<WatchItemCountComp>();
            if (comp != null && comp.questPart == null)
            {
                comp.questPart = this;
            }
        }
    }
}

public class WatchItemCountComp : GameComponent
{
    public QuestPart_WatchItemCountOnMap questPart;

    private const int CheckIntervalTicks = 250;
    private int ticksUntilNextCheck;

    public WatchItemCountComp()
    {
    }

    public WatchItemCountComp(Game game)
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

        if (questPart.map == null || questPart.itemDef == null || !Find.Maps.Contains(questPart.map))
        {
            return;
        }

        if (questPart.map.resourceCounter.GetCount(questPart.itemDef) < questPart.requiredCount)
        {
            return;
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
