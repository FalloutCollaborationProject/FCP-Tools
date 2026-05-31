using RimWorld.QuestGen;

namespace FCP.Core.RadiantQuests;

public class QuestNode_AddCacheSecuredPart : QuestNode
{
    public SlateRef<int> mapTile;
    public SlateRef<string> completeSignal;

    protected override bool TestRunInt(Slate slate)
    {
        return true;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        QuestPart_CacheSecured questPart = new QuestPart_CacheSecured();
        questPart.checkSignal = "site.AllEnemiesDefeated";
        questPart.mapTile = mapTile.GetValue(slate);
        questPart.outSignal = QuestGenUtility.HardcodedSignalWithQuestID(completeSignal.GetValue(slate));
        QuestGen.quest.AddPart(questPart);
    }
}
