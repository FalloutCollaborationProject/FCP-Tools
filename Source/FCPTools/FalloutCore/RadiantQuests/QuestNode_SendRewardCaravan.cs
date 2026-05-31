using RimWorld.QuestGen;
using Verse;

namespace FCP.Core.RadiantQuests;

public class QuestNode_SendRewardCaravan : QuestNode
{
    public SlateRef<string> inSignal;
    public SlateRef<Faction> faction;
    public SlateRef<PawnKindDef> traderKind;
    public SlateRef<List<PawnKindDef>> guardKinds;
    public SlateRef<ThingDef> currency;
    public SlateRef<float?> rewardValue;
    public SlateRef<int?> guardsCount;
    public SlateRef<string> letterLabel;
    public SlateRef<string> letterText;

    protected override bool TestRunInt(Slate slate) => true;

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        QuestPart_SendRewardCaravan part = new QuestPart_SendRewardCaravan();
        part.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate));
        part.faction = faction.GetValue(slate);
        part.traderKind = traderKind.GetValue(slate);
        part.guardKinds = guardKinds.GetValue(slate);
        part.currency = currency.GetValue(slate);
        part.rewardValue = rewardValue.GetValue(slate) ?? slate.Get<float>("rewardValue", 0f);
        part.guardsCount = guardsCount.GetValue(slate) ?? 2;
        part.letterLabel = letterLabel.GetValue(slate);
        part.letterText = letterText.GetValue(slate);
        QuestGen.quest.AddPart(part);
    }
}
