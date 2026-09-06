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
            List<GraphicData> femaleGraphicDataList = comp.Props.flyingFemaleBodyGraphicData;
            List<GraphicData> bodyGraphicDataList = comp.Props.flyingBodyGraphicData;

            List<GraphicData> graphicDataList = pawn.gender == Gender.Female
                && curKindLifeStage.femaleGraphicData != null
                && !femaleGraphicDataList.NullOrEmpty()
                ? femaleGraphicDataList
                : bodyGraphicDataList;

            if (graphicDataList.NullOrEmpty() || curKindLifeStageInd >= graphicDataList.Count)
                return flyingGraphic;

            GraphicData flyingGraphicData = graphicDataList[curKindLifeStageInd];

            flyingGraphic = GraphicDatabase.Get(flyingGraphicData.graphicClass, flyingGraphicData.texPath,
                flyingGraphicData.shaderType?.Shader ?? ShaderDatabase.CutoutComplex, flyingGraphicData.drawSize,
                flyingGraphicData.color, flyingGraphicData.colorTwo);

            return flyingGraphic;
        }
    }
}