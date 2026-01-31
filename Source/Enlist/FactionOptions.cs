using System.Collections.Generic;
using Verse;

namespace FCP.Enlist;

public class FactionOptions : IExposable
{
    public Dictionary<FactionEnlistOptionsDef, bool> factionsRecruiters = new Dictionary<FactionEnlistOptionsDef, bool>();
    public Dictionary<FactionEnlistOptionsDef, SalaryInfo> factionsSalaries = new Dictionary<FactionEnlistOptionsDef, SalaryInfo>();
    public Dictionary<FactionEnlistOptionsDef, FactionStorage> factionsStorages = new Dictionary<FactionEnlistOptionsDef, FactionStorage>();
    public Dictionary<FactionEnlistOptionsDef, int> factionsReinforcementsLastTick = new Dictionary<FactionEnlistOptionsDef, int>();
    public Dictionary<FactionEnlistOptionsDef, QuestContainer> factionsWithQuests = new Dictionary<FactionEnlistOptionsDef, QuestContainer>();
    public Dictionary<FactionEnlistOptionsDef, FactionState> factionsBought = new Dictionary<FactionEnlistOptionsDef, FactionState>();

    public void ExposeData()
    {
        Scribe_Collections.Look(ref factionsRecruiters, "factionsRecruiters", LookMode.Def, LookMode.Value, ref factionKeys, ref boolValues);
        Scribe_Collections.Look(ref factionsSalaries, "factionsSalaries", LookMode.Def, LookMode.Deep, ref factionKeys2, ref salaryValues);
        Scribe_Collections.Look(ref factionsStorages, "factionsStorages", LookMode.Def, LookMode.Deep, ref factionKeys3, ref storageValues);
        Scribe_Collections.Look(ref factionsReinforcementsLastTick, "factionsReinforcementsLastTick", LookMode.Def, LookMode.Value, ref factionKeys4, ref intValues);
        Scribe_Collections.Look(ref factionsWithQuests, "factionsWithQuests", LookMode.Def, LookMode.Deep, ref factionKeys5, ref questContainerValues);
        Scribe_Collections.Look(ref factionsBought, "factionsBought", LookMode.Def, LookMode.Deep, 
            ref factionKeys6, ref boughtValues);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            factionsRecruiters ??= new Dictionary<FactionEnlistOptionsDef, bool>();
            factionsSalaries ??= new Dictionary<FactionEnlistOptionsDef, SalaryInfo>();
            factionsStorages ??= new Dictionary<FactionEnlistOptionsDef, FactionStorage>();
            factionsReinforcementsLastTick ??= new Dictionary<FactionEnlistOptionsDef, int>();
            factionsWithQuests ??= new Dictionary<FactionEnlistOptionsDef, QuestContainer>();
            factionsBought = new Dictionary<FactionEnlistOptionsDef, FactionState>();
        }
    }

    private List<FactionEnlistOptionsDef> factionKeys;
    private List<bool> boolValues;

    private List<FactionEnlistOptionsDef> factionKeys2;
    private List<SalaryInfo> salaryValues;

    private List<FactionEnlistOptionsDef> factionKeys3;
    private List<FactionStorage> storageValues;

    private List<FactionEnlistOptionsDef> factionKeys4;
    private List<int> intValues;

    private List<FactionEnlistOptionsDef> factionKeys5;
    private List<QuestContainer> questContainerValues;

    private List<FactionEnlistOptionsDef> factionKeys6;
    private List<FactionState> boughtValues;
}