using RimWorld.Planet;
using RimWorld.QuestGen;

namespace FCP.Core.RadiantQuests;

public class QuestNode_DefendCrashSite : QuestNode
{
    public SlateRef<string> inSignal;
    public SlateRef<Faction> enemyFaction;
    public SlateRef<float> points;
    public SlateRef<float> groupDifficultyMult;
    public SlateRef<int> waveCount;
    public SlateRef<int> ticksBetweenWaves;
    public SlateRef<string> outSignalComplete;
    public SlateRef<MapParent> mapParent;

    protected override bool TestRunInt(Slate slate)
    {
        return true;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        QuestPart_DefendCrashSite part = new QuestPart_DefendCrashSite();
        part.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal");
        part.enemyFaction = enemyFaction.GetValue(slate);
        part.points = points.GetValue(slate);
        part.groupDifficultyMult = groupDifficultyMult.TryGetValue(slate, out float mult) ? mult : (slate.Exists("groupDifficultyMult") ? slate.Get<float>("groupDifficultyMult") : 1f);
        part.waveCount = waveCount.TryGetValue(slate, out int waves) ? waves : 3;
        part.ticksBetweenWaves = ticksBetweenWaves.TryGetValue(slate, out int ticks) ? ticks : 15000;
        part.outSignalComplete = outSignalComplete.TryGetValue(slate, out string sig) ? QuestGenUtility.HardcodedSignalWithQuestID(sig) : null;
        part.mapParent = mapParent.GetValue(slate);
        QuestGen.quest.AddPart(part);
    }
}
