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
		var extension = Find.Storyteller.def.GetModExtension<ModExtension_StoryTellerIsJoiner>();
		return extension != null && extension.AllCharacterDefs.Any(def => !UniqueCharactersTracker.Instance.CharacterPawnExists(def));
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
		return GeneratePawn_NewTemp(null);
	}

	public override Pawn GeneratePawn_NewTemp(Map map)
	{
		var extension = Find.Storyteller.def.GetModExtension<ModExtension_StoryTellerIsJoiner>();
		List<CharacterDef> available = extension.AllCharacterDefs.Where(def => !UniqueCharactersTracker.Instance.CharacterPawnExists(def)).ToList();
		CharacterDef charDef = available.Any() ? available.RandomElement() : extension.AllCharacterDefs.First();
		Pawn pawn = UniqueCharactersTracker.Instance.GetOrGenPawn(charDef);

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
	
    [Obsolete]
    public override void SendLetter(Quest quest, Pawn pawn)
    {
		TaggedString letterTitle = "FCP_LetterLabel_SpecialWandererJoins".Translate(pawn.Named("PAWN")).AdjustedFor(pawn);
		TaggedString letterText = "FCP_Letter_SpecialWandererJoins".Translate(pawn.Named("PAWN")).AdjustedFor(pawn);
		letterText += $"\n\n{Find.Storyteller.def.description}";
		ChoiceLetter_AcceptJoinerScenario choiceLetter = (ChoiceLetter_AcceptJoinerScenario)LetterMaker.MakeLetter(letterTitle, letterText, FCPDefOf.FCP_Letter_AcceptStoryteller);
		choiceLetter.signalAccept = signalAccept;
		choiceLetter.signalReject = signalReject;
		choiceLetter.quest = quest;
		choiceLetter.overrideMap = Find.AnyPlayerHomeMap;
		choiceLetter.StartTimeout(TimeoutTicks);
		Find.LetterStack.ReceiveLetter(choiceLetter);
	}
}
