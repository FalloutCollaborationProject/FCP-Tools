using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace FCP.Core.RadiantQuests;

public class QuestPart_DefendCrashSite : QuestPart
{
    public string inSignal;
    public Faction enemyFaction;
    public float points;
    public float groupDifficultyMult = 1f;
    public int waveCount = 3;
    public int ticksBetweenWaves = 15000;
    public string outSignalComplete;
    public MapParent mapParent;

    internal DefendCrashSiteComp comp;

    public override void Notify_QuestSignalReceived(Signal signal)
    {
        base.Notify_QuestSignalReceived(signal);

        if (signal.tag == inSignal)
        {
            comp = new DefendCrashSiteComp(Current.Game) { questPart = this };
            Current.Game.components.Add(comp);
        }
    }

    public void SpawnWave(int waveNum)
    {
        if (mapParent.Map == null)
        {
            return;
        }
        float basePoints = points > 0 ? points : 500f;
        float wavePoints = Mathf.Max(200f, basePoints * groupDifficultyMult * (1.2f + waveNum * 0.4f));
        
        IncidentParms parms = new IncidentParms
        {
            target = mapParent.Map,
            faction = enemyFaction,
            points = wavePoints,
            raidStrategy = RaidStrategyDefOf.ImmediateAttack,
            raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn,
            forced = true
        };
        
        IncidentDefOf.RaidEnemy.Worker.TryExecute(parms);
    }

    public void Complete()
    {
        if (!outSignalComplete.NullOrEmpty())
        {
            Find.SignalManager.SendSignal(new Signal(outSignalComplete));
        }
    }

    public override void Cleanup()
    {
        base.Cleanup();
        if (comp != null)
        {
            Current.Game.components.Remove(comp);
            comp = null;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignal, "inSignal");
        Scribe_References.Look(ref enemyFaction, "enemyFaction");
        Scribe_Values.Look(ref points, "points");
        Scribe_Values.Look(ref groupDifficultyMult, "groupDifficultyMult", 1f);
        Scribe_Values.Look(ref waveCount, "waveCount", 3);
        Scribe_Values.Look(ref ticksBetweenWaves, "ticksBetweenWaves", 15000);
        Scribe_Values.Look(ref outSignalComplete, "outSignalComplete");
        Scribe_References.Look(ref mapParent, "mapParent");
        
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            comp = Current.Game?.GetComponent<DefendCrashSiteComp>();
            if (comp != null && comp.questPart == null)
            {
                comp.questPart = this;
            }
        }
    }
}

public class DefendCrashSiteComp : GameComponent
{
    public QuestPart_DefendCrashSite questPart;
    private int wavesSpawned;
    private int ticksUntilNext;
    private bool initialized;

    public DefendCrashSiteComp()
    {
    }

    public DefendCrashSiteComp(Game game)
    {
    }

    public override void GameComponentTick()
    {
        base.GameComponentTick();

        if (questPart == null)
        {
            Current.Game.components.Remove(this);
            return;
        }

        if (!initialized)
        {
            ticksUntilNext = Rand.Range(1000, 3000);
            initialized = true;
        }

        if (wavesSpawned >= questPart.waveCount) return;

        ticksUntilNext--;

        if (ticksUntilNext <= 0)
        {
            questPart.SpawnWave(wavesSpawned);
            wavesSpawned++;

            if (wavesSpawned >= questPart.waveCount)
            {
                questPart.Complete();
                Current.Game.components.Remove(this);
            }
            else
            {
                ticksUntilNext = questPart.ticksBetweenWaves;
            }
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref wavesSpawned, "wavesSpawned");
        Scribe_Values.Look(ref ticksUntilNext, "ticksUntilNext");
        Scribe_Values.Look(ref initialized, "initialized");
        
        if (Scribe.mode == LoadSaveMode.PostLoadInit && questPart == null)
        {
            List<Quest> quests = Find.QuestManager.QuestsListForReading;
            for (int i = 0; i < quests.Count; i++)
            {
                List<QuestPart> parts = quests[i].PartsListForReading;
                for (int j = 0; j < parts.Count; j++)
                {
                    QuestPart_DefendCrashSite defendPart = parts[j] as QuestPart_DefendCrashSite;
                    if (defendPart != null && defendPart.comp == null)
                    {
                        questPart = defendPart;
                        defendPart.comp = this;
                        return;
                    }
                }
            }
        }
    }
}
