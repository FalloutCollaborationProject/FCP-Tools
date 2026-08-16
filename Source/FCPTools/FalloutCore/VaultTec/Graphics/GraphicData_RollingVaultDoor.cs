namespace FCPVT;

public class GraphicData_RollingVaultDoor : GraphicData
{
    public bool isLeftSideGraphic = false;
    public bool shouldFade = false;

    public float xMoveAmount = 1f;
    public float rotationFactor = 0f;
    public float fadeFactor = 0f;
    public float maxAngle = 0f;

    public GraphicData_RollingVaultDoor() : base()
    {

    }
}
