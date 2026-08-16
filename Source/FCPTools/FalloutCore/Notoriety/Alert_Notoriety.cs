namespace FCP.Core;

public class Alert_Notoriety : Alert
{
    public Alert_Notoriety()
    {
        defaultLabel = "FCP_Alert_Notoriety_Label".Translate();
        defaultExplanation = "FCP_Alert_Notoriety_Desc".Translate();
        defaultPriority = AlertPriority.Medium;
    }

    public override AlertReport GetReport() => NotorietyUtility.IsNotorious();
}
