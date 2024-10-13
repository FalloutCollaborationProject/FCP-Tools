namespace FCP.Core.Birds;

public class PawnRenderNode_Flying : PawnRenderNode_AnimalPart
{
    private readonly Pawn pawn;
    private Graphic flyingGraphic;

    public PawnRenderNode_Flying(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props,
        tree)
    {
        this.pawn = pawn;
        _ = FlyingGraphic;
    }

    public Graphic FlyingGraphic
    {
        get
        {
            if (flyingGraphic != null)
            {
                return flyingGraphic;
            }

            if (!pawn.IsFlyingPawn(out var comp))
            {
                return flyingGraphic;
            }

            var curKindLifeStage = pawn.ageTracker.CurKindLifeStage;
            var curKindLifeStageInd = pawn.ageTracker.CurLifeStageIndex;
            GraphicData flyingGaraphicData;
            if (pawn.gender != Gender.Female || curKindLifeStage.femaleGraphicData == null)
            {
                flyingGaraphicData = comp.Props.flyingBodyGraphicData[curKindLifeStageInd];
            }
            else
            {
                flyingGaraphicData = comp.Props.flyingFemaleBodyGraphicData[curKindLifeStageInd];
            }

            flyingGraphic = GraphicDatabase.Get(flyingGaraphicData.graphicClass, flyingGaraphicData.texPath,
                flyingGaraphicData.shaderType?.Shader ?? ShaderDatabase.CutoutComplex, flyingGaraphicData.drawSize,
                flyingGaraphicData.color, flyingGaraphicData.colorTwo);

            return flyingGraphic;
        }
    }
}