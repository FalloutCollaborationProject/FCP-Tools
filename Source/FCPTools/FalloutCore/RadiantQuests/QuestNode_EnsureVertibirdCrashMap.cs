using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace FCP.Core.RadiantQuests;

public class QuestNode_EnsureVertibirdCrashMap : QuestNode
{
    public SlateRef<string> inSignal;
    public SlateRef<MapParent> mapParent;
    public SlateRef<ThingDef> vertibirdDef;

    protected override bool TestRunInt(Slate slate)
    {
        return true;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        QuestPart_EnsureVertibirdCrashMap part = new QuestPart_EnsureVertibirdCrashMap();
        part.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? "site.MapGenerated";
        part.mapParent = mapParent.GetValue(slate);
        part.vertibirdDef = vertibirdDef.GetValue(slate);
        QuestGen.quest.AddPart(part);
    }
}
