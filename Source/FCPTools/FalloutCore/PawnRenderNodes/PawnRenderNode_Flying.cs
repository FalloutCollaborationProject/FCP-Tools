namespace FCP.Core;

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
            if (flyingGraphic != null) return flyingGraphic;
            if (!pawn.IsFlyingPawn(out CompFlyingPawn comp)) return flyingGraphic;

            PawnKindLifeStage curKindLifeStage = pawn.ageTracker.CurKindLifeStage;
            int curKindLifeStageInd = pawn.ageTracker.CurLifeStageIndex;
            GraphicData flyingGraphicData;
            
            if (pawn.gender != Gender.Female || curKindLifeStage.femaleGraphicData == null)
            {
                flyingGraphicData = comp.Props.flyingBodyGraphicData[curKindLifeStageInd];
            }
            else
            {
                flyingGraphicData = comp.Props.flyingFemaleBodyGraphicData[curKindLifeStageInd];
            }

            flyingGraphic = GraphicDatabase.Get(flyingGraphicData.graphicClass, flyingGraphicData.texPath,
                flyingGraphicData.shaderType?.Shader ?? ShaderDatabase.CutoutComplex, flyingGraphicData.drawSize,
                flyingGraphicData.color, flyingGraphicData.colorTwo);

            return flyingGraphic;
        }
    }
}