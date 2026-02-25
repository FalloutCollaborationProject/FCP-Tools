// ReSharper disable UnassignedField.Global
// ReSharper disable ClassNeverInstantiated.Global

using FCP.Core;

namespace FCP.Factions;

public class FactionExtension_FixedIdeo : DefModExtension
{
    public IdeoIconDef ideoIconDef;
    public IdeoColorDef ideoColorDef;
    public string memberName;
    public string adjective;
    public string ritualRoomName;
    public List<PreceptDef> preceptDefs;
    public List<RoleOverride> roleOverrides;

    public void ApplyToIdeo(Ideo ideo)
    {
        ideo.memberName = memberName ?? ideo.memberName;
        ideo.adjective = adjective ?? ideo.adjective;
        ideo.WorshipRoomLabel = ritualRoomName ?? ideo.WorshipRoomLabel;
        
        if (!preceptDefs.NullOrEmpty()) 
            ApplyPrecepts(ideo);

        if (ideoIconDef != null)
        {
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                ideo.SetIcon(ideoIconDef,
                    ideoColorDef.colorDef ?? ideo.colorDef ?? IdeoFoundation.GetRandomColorDef(ideo));
            });
        }
    }

    private void ApplyPrecepts(Ideo ideo)
    {
        foreach (PreceptDef preceptDef in preceptDefs)
        {
            if (!ideo.CanAddPreceptAllFactions(preceptDef))
            {
                FCPLog.Warning($"Tried to add PreceptDef {preceptDef.defName} via FactionExtension_FixedIdeo, CanAddPreceptAllFactions was False");
                continue;
            }

            Precept precept = PreceptMaker.MakePrecept(preceptDef);
            ideo.AddPrecept(precept, init: true);
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