using RimWorld.Planet;
using RimWorld.QuestGen;

namespace FCP.Core;

public class QuestNode_Root_StorytellerJoin : QuestNode_Root_WandererJoin
{
	private const int TimeoutTicks = 30000;

	private string signalAccept;
	private string signalReject;

	protected override bool TestRunInt(Slate slate)
	{
		return Find.Storyteller.def.HasModExtension<ModExtension_StoryTellerIsJoiner>();
	}

	protected override void RunInt()
	{
		base.RunInt();
		Quest quest = QuestGen.quest;
		quest.Delay(TimeoutTicks, delegate
		{
			QuestGen_End.End(quest, QuestEndOutcome.Fail);
		});
	}

	public override Pawn GeneratePawn()
	{
		var extension = Find.Storyteller.def.GetModExtension<ModExtension_StoryTellerIsJoiner>();
		Pawn pawn = UniqueCharactersTracker.Instance.GetOrGenPawn(extension.characterDef);
		
		if (!pawn.IsWorldPawn())
		{
			Find.WorldPawns.PassToWorld(pawn);
		}
		return pawn;
	}

	protected override void AddSpawnPawnQuestParts(Quest quest, Map map, Pawn pawn)
	{
		signalAccept = QuestGenUtility.HardcodedSignalWithQuestID("Accept");
		signalReject = QuestGenUtility.HardcodedSignalWithQuestID("Reject");
			
		quest.Signal(signalAccept, delegate
		{
			quest.SetFaction(Gen.YieldSingle(pawn), Faction.OfPlayer);
			quest.PawnsArrive(Gen.YieldSingle(pawn), null, map.Parent);
			QuestGen_End.End(quest, QuestEndOutcome.Success);
		});
			
		quest.Signal(signalReject, delegate
		{
			quest.GiveDiedOrDownedThoughts(pawn, PawnDiedOrDownedThoughtsKind.DeniedJoining);
			QuestGen_End.End(quest, QuestEndOutcome.Fail);
		});
	}

	public override void SendLetter(Quest quest, Pawn pawn)
	{
		TaggedString letterTitle = "FCP_LetterLabel_SpecialWandererJoins".Translate(pawn.Named("PAWN")).AdjustedFor(pawn);
		TaggedString letterText = "FCP_Letter_SpecialWandererJoins".Translate(pawn.Named("PAWN")).AdjustedFor(pawn);
		letterText += $"\n\n{Find.Storyteller.def.description}";
			
		var choiceLetter = (ChoiceLetter_AcceptJoiner)LetterMaker.MakeLetter(letterTitle, letterText, FCPDefOf.FCP_Letter_AcceptStoryteller);
		choiceLetter.signalAccept = signalAccept;
		choiceLetter.signalReject = signalReject;
		choiceLetter.quest = quest;
		choiceLetter.StartTimeout(TimeoutTicks);
			
		Find.LetterStack.ReceiveLetter(choiceLetter);
	}
}