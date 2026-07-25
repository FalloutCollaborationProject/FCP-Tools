using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace FCP.Core.RadiantQuests;

public class QuestNode_WatchItemCountOnMap : QuestNode
{
    public SlateRef<Map> map;
    public SlateRef<ThingDef> itemDef;
    public SlateRef<int> requiredCount;
    public SlateRef<string> outSignalComplete;

    protected override bool TestRunInt(Slate slate) => true;

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        QuestPart_WatchItemCountOnMap part = new QuestPart_WatchItemCountOnMap
        {
            map = map.GetValue(slate),
            itemDef = itemDef.GetValue(slate),
            requiredCount = requiredCount.GetValue(slate),
            outSignalComplete = QuestGenUtility.HardcodedSignalWithQuestID(outSignalComplete.GetValue(slate))
        };
        QuestGen.quest.AddPart(part);
        part.StartWatching();
    }
}
