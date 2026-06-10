using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace FCP.Core.RadiantQuests;

public class QuestPart_SpawnCrashSurvivors : QuestPart
{
    public string inSignal;
    public List<Pawn> pawns;
    public MapParent mapParent;

    public override void Notify_QuestSignalReceived(Signal signal)
    {
        base.Notify_QuestSignalReceived(signal);
        if (signal.tag != inSignal || mapParent.Map == null) return;

        Map map = mapParent.Map;
        MapComponent_VertibirdCrash crash = map.GetComponent<MapComponent_VertibirdCrash>();
        IntVec3 center = crash?.crashLocation.IsValid == true ? crash.crashLocation : map.Center;

        int deadCount = Rand.RangeInclusive(1, 3);
        int injuredCount = Rand.RangeInclusive(2, 4);
        if (8 - deadCount - injuredCount < 1) deadCount--;

        pawns.Shuffle();
        List<Pawn> livingPawns = new List<Pawn>();

        for (int i = 0; i < pawns.Count; i++)
        {
            Pawn pawn = pawns[i];
            if (pawn == null) continue;

            if (CellFinder.TryFindRandomSpawnCellForPawnNear(center, map, out IntVec3 c))
            {
                GenSpawn.Spawn(pawn, c, map);

                if (i < deadCount)
                {
                    pawn.Kill(null);
                }
                else
                {
                    livingPawns.Add(pawn);
                    if (i < deadCount + injuredCount)
                    {
                        pawn.TakeDamage(new DamageInfo(DamageDefOf.Crush, Rand.Range(15f, 35f)));
                        if (Rand.Bool && pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Outside).TryRandomElement(out BodyPartRecord part))
                        {
                            pawn.TakeDamage(new DamageInfo(DamageDefOf.Cut, Rand.Range(10f, 20f), 0f, -1f, null, part));
                        }
                    }
                }
            }
        }

        if (livingPawns.Count > 0)
        {
            LordMaker.MakeNewLord(pawns[0].Faction, new LordJob_DefendBase(pawns[0].Faction, center, 0, false), map, livingPawns);
        }
        
        int survivorCount = livingPawns.Count;
        string text = survivorCount > 0 
            ? string.Format("{0} survivors located at the crash site. Enemy forces are closing in.", survivorCount)
            : "No survivors detected. Enemy forces are closing in.";
        
        Find.LetterStack.ReceiveLetter("Crash Site", text, LetterDefOf.NeutralEvent, new LookTargets(center, map));
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignal, "inSignal");
        Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
        Scribe_References.Look(ref mapParent, "mapParent");
    }
}
