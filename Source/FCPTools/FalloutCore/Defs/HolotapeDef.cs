using System.Collections.Generic;
using Verse;

namespace FCP.Core.Holotapes
{
    public class HolotapeDef : Def
    {
        public List<HolotapeEntry> entries;
        public HolotapeCategory category;
        public string author;
        public SkillDef skillToTeach;
        public float xpAmount;
        
        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var error in base.ConfigErrors())
                yield return error;

            if (entries.NullOrEmpty())
                yield return "HolotapeDef has no entries";
        }
    }

    public class HolotapeEntry
    {
        public string title;
        public string content;
        public int entryNumber;
    }

    public enum HolotapeCategory
    {
        PersonalLog,
        MilitaryReport,
        PreWarStory,
        TechnicalManual,
        BroadcastRecording,
        CorporateMemo,
        ScientificData,
        SurvivalGuide
    }
}
