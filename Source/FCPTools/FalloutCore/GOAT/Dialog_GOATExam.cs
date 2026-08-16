using UnityEngine;

namespace FCP.Core.GOAT;

public class Dialog_GOATExam : Window
{
    private const float RowHeight = 32f;

    private readonly Pawn pawn;
    private readonly Queue<Pawn> remaining;
    private readonly Dictionary<SkillDef, int> tally = new Dictionary<SkillDef, int>();
    private List<(string text, SkillDef skill)> currentAnswers;
    private int questionIndex;
    private bool showingResult;
    private SkillDef resultSkill;
    private TraitDef resultTrait;
    private SkillDef downgradedSkill;

    public override Vector2 InitialSize => new Vector2(620f, 420f);

    public Dialog_GOATExam(Pawn pawn, Queue<Pawn> remaining)
    {
        this.pawn = pawn;
        this.remaining = remaining;
        currentAnswers = GOATExamQuestions.AnswersFor(0);
        doCloseX = false;
        forcePause = true;
        absorbInputAroundWindow = true;
        closeOnClickedOutside = false;
        closeOnAccept = false;
        closeOnCancel = false;
    }

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Medium;
        GUI.color = TerminalColors.PrimaryColor;
        Widgets.Label(new Rect(0f, 0f, inRect.width, 30f), "FCP_GOATExam_Title".Translate(pawn.LabelShortCap));
        Text.Font = GameFont.Small;
        Widgets.DrawLineHorizontal(0f, 34f, inRect.width);

        Rect contentRect = new Rect(0f, 44f, inRect.width, inRect.height - 44f);
        if (showingResult)
            DrawResult(contentRect);
        else
            DrawQuestion(contentRect);

        GUI.color = Color.white;
    }

    private void DrawQuestion(Rect rect)
    {
        GUI.color = TerminalColors.PrimaryColor;

        string question = GOATExamQuestions.Questions[questionIndex];
        float questionHeight = Text.CalcHeight(question, rect.width);
        Widgets.Label(new Rect(rect.x, rect.y, rect.width, questionHeight), question);

        float y = rect.y + questionHeight + 16f;
        foreach ((string text, SkillDef skill) option in currentAnswers)
        {
            Rect optionRect = new Rect(rect.x, y, rect.width, RowHeight);

            if (Mouse.IsOver(optionRect))
            {
                GUI.color = TerminalColors.HighlightColor;
                Widgets.DrawHighlight(optionRect);
            }

            GUI.color = TerminalColors.PrimaryColor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(optionRect.ContractedBy(6f, 0f), option.text);
            Text.Anchor = TextAnchor.UpperLeft;

            if (Widgets.ButtonInvisible(optionRect))
                Answer(option.skill);

            y += RowHeight + 4f;
        }

        GUI.color = TerminalColors.PrimaryColor;
        Rect skipRect = new Rect(rect.x, rect.yMax - 30f, 120f, 30f);
        if (Widgets.ButtonText(skipRect, "FCP_GOATExam_Skip".Translate()))
        {
            Skip();
        }
    }

    private void Skip()
    {
        Close();
        ScenPart_GOATExam.RunNext(remaining);
    }

    private void Answer(SkillDef skill)
    {
        tally.TryGetValue(skill, out int current);
        tally[skill] = current + 1;
        questionIndex++;

        if (questionIndex < GOATExamQuestions.Questions.Count)
        {
            currentAnswers = GOATExamQuestions.AnswersFor(questionIndex);
            return;
        }

        resultSkill = tally.OrderByDescending(kv => kv.Value).Select(kv => kv.Key).FirstOrDefault();
        if (resultSkill != null)
        {
            SkillRecord record = pawn.skills.GetSkill(resultSkill);
            if (record.passion < Passion.Major)
                record.passion = (Passion)((int)record.passion + 1);
            record.Learn(200f, direct: true);

            downgradedSkill = GOATTraitPool.TryDowngradePassionElsewhere(pawn, resultSkill);
            resultTrait = GOATTraitPool.TryGrantTrait(pawn, resultSkill);
        }

        showingResult = true;
    }

    private void DrawResult(Rect rect)
    {
        GUI.color = TerminalColors.PrimaryColor;

        string text = resultSkill != null
            ? "FCP_GOATExam_Result".Translate(pawn.LabelShortCap, resultSkill.label)
            : "FCP_GOATExam_ResultNone".Translate(pawn.LabelShortCap);

        if (resultTrait != null)
        {
            text += "\n\n" + "FCP_GOATExam_ResultTrait".Translate(pawn.LabelShortCap, resultTrait.degreeDatas[0].label);
        }

        if (downgradedSkill != null)
        {
            text += "\n\n" + "FCP_GOATExam_ResultTradeoff".Translate(pawn.LabelShortCap, downgradedSkill.label);
        }

        float height = Text.CalcHeight(text, rect.width);
        Widgets.Label(new Rect(rect.x, rect.y, rect.width, height), text);

        Rect buttonRect = new Rect(rect.x, rect.yMax - 35f, 120f, 35f);
        if (Widgets.ButtonText(buttonRect, "OK".Translate()))
        {
            Close();
            ScenPart_GOATExam.RunNext(remaining);
        }
    }
}
