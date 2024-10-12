using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace FCP.Core;

public class QuestNode_Root_StorytellerJoin : QuestNode_Root_WandererJoin
{
	private const int TimeoutTicks = 30000;

	private string signalAccept;
	private string signalReject;

	protected override bool TestRunInt(Slate slate)
	{
		return Find.Storyteller.def.HasModExtension<StoryTellerIsJoinerExtension>();
	}

	protected override void RunInt()
	{
		base.RunInt();
		var quest = QuestGen.quest;
		quest.Delay(TimeoutTicks, delegate
		{
			QuestGen_End.End(quest, QuestEndOutcome.Fail);
		});
	}

	public override Pawn GeneratePawn()
	{
		var extension = Find.Storyteller.def.GetModExtension<StoryTellerIsJoinerExtension>();
		var slate = QuestGen.slate;
		
		var request = new PawnGenerationRequest(extension.pawnKindDef ?? PawnKindDefOf.Colonist, Faction.OfPlayer,
			forceGenerateNewPawn: true,
			canGeneratePawnRelations: false,
			fixedBiologicalAge: extension.biologicalAge,
			fixedChronologicalAge: extension.chronologicalAge ?? extension.biologicalAge,
			fixedGender: extension.gender,
			onlyUseForcedBackstories: extension.onlyFixedPawnKindBackstories,
			forcedXenotype: extension.forcedXenotypeDef ?? XenotypeDefOf.Baseliner,
			developmentalStages: DevelopmentalStage.Adult);

		// Factional Stuff
		if (extension?.originalFactionDef != null)
		{
			var faction = Find.FactionManager.FirstFactionOfDef(extension.originalFactionDef);
			if (faction != null)
			{
				request.Faction = faction;
				
				if (extension.useOriginalFactionIdeo)
				{
					request.FixedIdeo = faction.ideos.PrimaryIdeo;
				}

				if (extension.originalFactionTitle != null)
				{
					request.ForbidAnyTitle = false;
					request.FixedTitle = extension.originalFactionTitle;
				}
			}
			else
			{
				Log.Warning($"Couldn't find a faction of def {extension.originalFactionDef.defName}, despite it being defined for the storyteller pawn.");
			}
		}
		
		request.ValidateAndFix();
		
		// Generate the pawn
		var pawn = PawnGenerator.GeneratePawn(request);
		PostGeneratePawn(pawn, extension);
		
		if (!pawn.IsWorldPawn())
		{
			Find.WorldPawns.PassToWorld(pawn);
		}
		return pawn;
	}

	private static void PostGeneratePawn(Pawn pawn, StoryTellerIsJoinerExtension extension)
	{
		if (pawn.Name is NameTriple name)
		{
			pawn.Name = new NameTriple(extension.firstName ?? name.First, extension.nickname ?? name.Nick, extension.lastName ?? name.Last);
		}

		// Remove all permits, since tynan didn't check to see if they're actually valid.
		if (pawn.royalty != null && pawn.royalty.AllTitlesForReading.Any())
		{
			pawn.royalty.AllFactionPermits.Clear();
		}

		if (extension.hairDef != null)
		{
			pawn.story.hairDef = extension.hairDef;
		}
		
		if (extension.hairColor != null)
		{
			pawn.story.HairColor = (Color)extension.hairColor;
		}

		if (extension.beardDef != null)
		{
			pawn.style.beardDef = extension.beardDef;
		}

		if (extension.skinColorOverride != null)
		{
			pawn.story.skinColorOverride = extension.skinColorOverride;
		}

		pawn.Drawer?.renderer?.SetAllGraphicsDirty();

	}

	private static readonly FieldInfo PawnSkinColorBaseField = AccessTools.Field(typeof(Pawn), "skinColorBase");

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
		var letterTitle = "FCP_LetterLabel_SpecialWandererJoins".Translate(pawn.Named("PAWN")).AdjustedFor(pawn);
		var letterText = "FCP_Letter_SpecialWandererJoins".Translate(pawn.Named("PAWN")).AdjustedFor(pawn);
		letterText += $"\n\n{Find.Storyteller.def.description}";
		
		var choiceLetter = (ChoiceLetter_AcceptJoiner)LetterMaker.MakeLetter(letterTitle, letterText, FCPDefOf.FCP_Letter_AcceptStoryteller);
		choiceLetter.signalAccept = signalAccept;
		choiceLetter.signalReject = signalReject;
		choiceLetter.quest = quest;
		choiceLetter.StartTimeout(TimeoutTicks);
		
		Find.LetterStack.ReceiveLetter(choiceLetter);
	}
	
}