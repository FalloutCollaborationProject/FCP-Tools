// ReSharper disable UnassignedField.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace FCP.Factions;

public class FactionExtension_FixedIdeo : DefModExtension
{
    public IdeoIconDef ideoIconDef;
    public IdeoColorDef ideoColorDef;
    public string memberName;
    public string adjective;
    public string ritualRoomName;
    public List<RoleOverride> roleOverrides;

    public void CopyToIdeo(Ideo ideo)
    {
        ideo.memberName = memberName ?? ideo.memberName;
        ideo.adjective = adjective ?? ideo.adjective;
        ideo.WorshipRoomLabel = ritualRoomName ?? ideo.WorshipRoomLabel;

        if (ideoIconDef != null)
        {
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                ideo.SetIcon(ideoIconDef,
                    ideoColorDef.colorDef ?? ideo.colorDef ?? IdeoFoundation.GetRandomColorDef(ideo));
            });
        }
    }

    public class RoleOverride
    {
        public PreceptDef preceptDef;
        public string newName;
        public bool disableApparelRequirements = false;
        public List<PreceptApparelRequirement> apparelRequirementsOverride;
    }
}