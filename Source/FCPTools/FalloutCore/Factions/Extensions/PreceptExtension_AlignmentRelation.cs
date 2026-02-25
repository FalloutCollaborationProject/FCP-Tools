using System.Xml;
using FCP.Core;

namespace FCP.Factions;

public enum AlignmentRelation
{
    None,
    Evil,
    Hostile,
    Astray,
    Righteous
}

public class PreceptAlignmentRelation
{
    // ReSharper disable once UnassignedField.Global
    public PreceptDef precept;
    public AlignmentRelation alignment;
}

public class PreceptExtension_AlignmentRelation : DefModExtension
{
    public List<PreceptAlignmentRelation> alignments;

    [UsedImplicitly]
    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        alignments = [];

        foreach (XmlNode child in xmlRoot.ChildNodes)
        {
            if (child.NodeType != XmlNodeType.Element)
                continue;

            if (!Enum.TryParse(child.Name, out AlignmentRelation level))
            {
                FCPLog.Error($"PreceptExtension_AlignmentRelation: Unknown alignment relation '{child.Name}'");
                continue;
            }

            foreach (XmlNode li in child.ChildNodes)
            {
                if (li.NodeType != XmlNodeType.Element)
                    continue;

                string mayRequire = li.Attributes?["MayRequire"]?.Value;
                string mayRequireAnyOf = li.Attributes?["MayRequireAnyOf"]?.Value;
                if (!DirectXmlToObjectNew.ValidateMayRequires(mayRequire, mayRequireAnyOf))
                    continue;

                var entry = new PreceptAlignmentRelation { alignment = level };
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(entry, 
                    fieldName: "precept", 
                    targetDefName: li.InnerText.Trim(), 
                    mayRequireMod: mayRequire, 
                    mayRequireAnyMod: mayRequireAnyOf);
                
                alignments.Add(entry);
            }
        }
    }
}