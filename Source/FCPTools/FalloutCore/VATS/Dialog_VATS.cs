using UnityEngine;

namespace FCP.Core.VATS;

[StaticConstructorOnStartup]
public class Dialog_VATS(Verb_AbilityVATS verb, LocalTargetInfo target, IWindowDrawing customWindowDrawing = null) : Window(customWindowDrawing)
{
    private readonly Color ButtonTextColour = new (0.357f, 0.825f, 0.278f);

    private readonly Texture2D Logo = ContentFinder<Texture2D>.Get("UI/FCP_VATS_Logo_Small");

    public Verb Selected;

    public Dictionary<string, float> MultiplierLookup => VATSMod.Settings.MultiplierLookup;

    public override Vector2 InitialSize => new(845f, 740f);
    protected virtual Vector2 ButtonSize => new(200f, 40f);
    public virtual TaggedString OkButtonLabel => "OK".Translate();

    public virtual TaggedString CancelButtonLabel => "CancelButton".Translate();
    public virtual TaggedString WarningText => "";

    public float GetPartMultiplier(BodyPartDef def)
    {
        return MultiplierLookup.GetWithFallback(def.defName, 1.0f);
    }

    public virtual void DoButtonRow(ref RectDivider layout)
    {
        RectDivider rectDivider1 = layout.NewRow(ButtonSize.y, VerticalJustification.Bottom, 0.0f);
        RectDivider rectDivider3 = rectDivider1.NewCol(ButtonSize.x);
        RectDivider rectDivider4 = rectDivider1.NewCol(rectDivider1.Rect.width, HorizontalJustification.Right);

        if (Widgets.ButtonText(rectDivider3, CancelButtonLabel))
        {
            Cancel();
        }

        Widgets.Label(rectDivider4, WarningText);
    }

    public virtual void DoVATS(ref RectDivider layout)
    {
        ShotReport shotReport = verb.GetShotReport(target);

        float pawnMultiplier = 1 + Mathf.Clamp01(0.02f * verb.CasterPawn.skills.GetSkill(SkillDefOf.Shooting).Level);

        float estimatedHitChance = shotReport.TotalEstimatedHitChance;

        using (new ProfilerBlock(nameof(DoVATS)))
        {
            using (TextBlock.Default())
            {
                // Fetch the list of parts to show in the UI
                List<BodyPartRecord> parts = target
                    .Pawn.health.hediffSet.GetNotMissingParts()
                    .Where(p => p.def == target.Pawn.def.race.body.corePart.def || 
                                p.parent?.def == target.Pawn.def.race.body.corePart.def)
                    .Where(p => p.coverageAbs > 0.0)
                    .ToList();

                int partCount = parts.Count;

                RectDivider rowBtn = layout.NewRow(600f);

                RectDivider colLeft = rowBtn.NewCol(100f);
                RectDivider colMid = rowBtn.NewCol(InitialSize.x - 300f);
                RectDivider colRight = rowBtn.NewCol(100f, HorizontalJustification.Right);

                RectDivider logoRect = colMid.NewRow(100f);

                GUI.DrawTexture(logoRect, Logo, ScaleMode.ScaleToFit);

                Rect portraitRect = colMid.Rect.ContractedBy(30f);
                RenderTexture texture = PortraitsCache.Get(
                    target.Pawn,
                    portraitRect.size,
                    Rot4.South,
                    new Vector3(0f, 0f, 0.1f),
                    1.5f,
                    healthStateOverride: PawnHealthState.Mobile
                );

                GUI.DrawTexture(portraitRect, texture);

                for (int i = 0; i < partCount; i++)
                {
                    RectDivider rectDivider = i <= partCount / 2 
                        ? colLeft.NewRow(45f, marginOverride: 5f) 
                        : colRight.NewRow(45f, marginOverride: 5f);

                    float partAccuracy = Mathf.Clamp01(estimatedHitChance * pawnMultiplier * GetPartMultiplier(parts[i].def));
                    int partAccuracyPct = Mathf.CeilToInt(partAccuracy * 100);

                    if (!Widgets.ButtonText(rectDivider, $"{parts[i].LabelCap} [{partAccuracyPct}%]", 
                            false, true, ButtonTextColour))
                    {
                        continue;
                    }
                    
                    // call back to the verb to actually do the VATS attack
                    verb.VATS_Selection(target, parts[i], partAccuracy, shotReport);
                    Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
                    Close();
                }
            }
        }
    }

    protected virtual void Start()
    {
        Close();
    }

    protected virtual void Cancel()
    {
        Close();
    }

    public override void DoWindowContents(Rect inRect)
    {
        using (TextBlock.Default())
        {
            RectDivider layout1 = new(inRect, 145235235);
            layout1.NewRow(0.0f, VerticalJustification.Bottom, 1f);
            layout1.NewRow(0.0f);
            DoVATS(ref layout1);
            DoButtonRow(ref layout1);
            layout1.NewRow(0.0f, marginOverride: 20f);
            layout1.NewCol(20f, marginOverride: 0.0f);
        }
    }
}
