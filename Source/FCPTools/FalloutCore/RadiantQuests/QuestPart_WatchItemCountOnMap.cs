using RimWorld;
using UnityEngine;
using Verse;

namespace FCP.Core.RadiantQuests;

public class QuestPart_WatchItemCountOnMap : QuestPart
{
    public Map map;
    public ThingDef itemDef;
    public ThingCategoryDef itemCategory;
    public int requiredCount;
    public string outSignalComplete;

    internal WatchItemCountComp comp;

    public int CountOnMap => itemCategory != null ? map.resourceCounter.GetCountIn(itemCategory) : map.resourceCounter.GetCount(itemDef);

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
        IEnumerable<ThingDef> defs = itemCategory != null ? itemCategory.DescendantThingDefs : new[] { itemDef };
        foreach (ThingDef def in defs)
        {
            foreach (Thing stack in map.listerThings.ThingsOfDef(def).ToList())
            {
                if (remaining <= 0)
                {
                    break;
                }
                if (!stack.IsInAnyStorage())
                {
                    continue;
                }
                int take = Mathf.Min(remaining, stack.stackCount);
                remaining -= take;
                stack.SplitOff(take).Destroy();
            }
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
        Scribe_Defs.Look(ref itemCategory, "itemCategory");
        Scribe_Values.Look(ref requiredCount, "requiredCount");
        Scribe_Values.Look(ref outSignalComplete, "outSignalComplete");
    }
}

public class WatchItemCountComp : GameComponent
{
    public QuestPart_WatchItemCountOnMap questPart;

    private const int CheckIntervalTicks = 250;
    private int ticksUntilNextCheck;
    private int questId = -1;
    private int partIndex = -1;

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

        if (questPart.quest?.State != QuestState.Ongoing || questPart.map == null || questPart.itemDef == null && questPart.itemCategory == null || !Find.Maps.Contains(questPart.map))
        {
            return;
        }

        if (questPart.CountOnMap < questPart.requiredCount)
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

        if (Scribe.mode == LoadSaveMode.Saving && questPart?.quest != null)
        {
            questId = questPart.quest.id;
            partIndex = QuestPartCompLink.PartIndex(questPart);
        }
        Scribe_Values.Look(ref questId, "questId", -1);
        Scribe_Values.Look(ref partIndex, "partIndex", -1);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            questPart = QuestPartCompLink.Resolve<QuestPart_WatchItemCountOnMap>(questId, partIndex, p => p.comp == null);
            if (questPart != null)
            {
                questPart.comp = this;
            }
        }
    }
}
