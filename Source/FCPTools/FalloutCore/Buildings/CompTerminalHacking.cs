using System.Collections.Generic;
using Verse;
using RimWorld;

namespace FCP.Core.Buildings
{
    public class CompTerminalHacking : ThingComp
    {
        private static readonly Dictionary<int, List<string>> wordPools = new Dictionary<int, List<string>>
        {
            { 4, new List<string> { "TALK", "HELP", "CODE", "DOOR", "PASS", "LOCK", "DARK", "COLD", "WARM", "SAFE", "ARMS", "GUNS", "FIRE", "LOUD", "BASE", "ROCK", "SAND", "WIND", "WAVE", "SHIP", "RUST", "VOLT", "ACID", "BOMB", "NUKE", "BURN", "MINE", "CAVE", "SITE", "FALL" } },
            { 5, new List<string> { "POWER", "ENTER", "RADIO", "VAULT", "ROBOT", "STEEL", "ARMOR", "LASER", "WORLD", "WASTE", "BLOOD", "METAL", "GUARD", "ELITE", "READY", "SIGHT", "PRIME", "BUILD", "ORDER", "RIFLE", "HEAVY", "SHELL", "MARCH", "BREAK", "GRACE", "PEACE", "RIGHT", "MIGHT", "FIGHT", "LIGHT" } },
            { 6, new List<string> { "SYSTEM", "ACCESS", "SECURE", "BREACH", "LOCKED", "PLAYER", "ATTACK", "DEFEND", "RESCUE", "SHELTER", "STATION", "MISSION", "SUPPLY", "COMBAT", "SIGNAL", "TARGET", "STRIKE", "PATROL", "THREAT", "ENERGY", "MATTER", "DANGER", "ROCKET", "UPLOAD", "REPORT", "BACKUP", "DEVICE", "SCREEN", "REMOTE", "SENSOR" } },
            { 7, new List<string> { "COMMAND", "CONTROL", "SCIENCE", "CAPTAIN", "NUCLEAR", "REACTOR", "WEAPONS", "DEFENSE", "PROTECT", "PROGRAM", "NETWORK", "INTEGER", "CIRCUIT", "ARCHIVE", "STORAGE", "MONITOR", "COUNTER", "WARNING", "CHAMBER", "CONDUCT", "RELEASE", "GENERAL", "SCANNER", "BATTERY", "ARMORED", "SOLDIER", "HOSTILE", "UNKNOWN", "PROJECT", "EXTRACT" } },
            { 8, new List<string> { "TERMINAL", "PASSWORD", "SECURITY", "PROTOCOL", "OVERRIDE", "DATABASE", "SHUTDOWN", "ACTIVATE", "MILITARY", "COMPOUND", "FLAGSHIP", "SENTINEL", "RESPONSE", "DISASTER", "PRIORITY", "ROBOTICS", "RESEARCH", "ENGINEER", "OBSTACLE", "LOCATION", "GUIDANCE", "COMPUTER", "DOWNLOAD", "TRANSFER", "RESTRICT", "SCRAMBLE", "DETECTOR", "PRESSURE", "EQUIPPED", "INFINITE" } },
            { 9, new List<string> { "MACHINERY", "COMMANDER", "OPERATION", "SAFEGUARD", "EMERGENCY", "GENERATOR", "APPARATUS", "PROCEDURE", "CONTAINER", "EXPLOSIVE", "STRUCTURE", "ASSEMBLED", "RESISTANT", "TECHNICAL", "SCIENTIST", "CONSTRUCT", "SATELLITE", "RADIATION", "COMPONENT", "TRANSPORT", "PERIMETER", "RECOGNIZE", "BROADCAST", "ENCRYPTED", "INTERFACE", "ESTABLISH", "MECHANISM", "CALIBRATE", "FIREPOWER", "OFFENSIVE" } },
            { 10, new List<string> { "CLASSIFIED", "ENCRYPTION", "AUTHORIZED", "OPERATIONS", "LABORATORY", "CHECKPOINT", "RESTRICTED", "AMMUNITION", "DEMOLITION", "INSTRUMENT", "TECHNOLOGY", "FILTRATION", "CONNECTION", "ALLOCATION", "UNDERSTAND", "CONDITIONS", "DEPLOYMENT", "QUARANTINE", "INSPECTION", "MANAGEMENT", "FOUNDATION", "REPOSITORY", "INITIALIZE", "STABILIZED", "ORDINANCES", "ENGAGEMENT", "PROTECTION", "SUCCESSION", "SYSTEMATIC", "ILLUMINATE" } }
        };

        private bool isLocked = true;
        private int lockoutTicksRemaining;
        private string correctPassword;
        private List<string> displayedWords;
        private int attemptsRemaining;

        public CompProperties_TerminalHacking Props => (CompProperties_TerminalHacking)props;

        public bool IsLocked => isLocked;
        public bool IsLockedOut => lockoutTicksRemaining > 0;
        public int LockoutHoursRemaining => lockoutTicksRemaining / 2500;
        public int AttemptsRemaining => attemptsRemaining;
        public string CorrectPassword => correctPassword;
        public List<string> DisplayedWords => displayedWords;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad && displayedWords == null)
            {
                GeneratePasswords();
                attemptsRemaining = Props.maxAttempts;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (parent.IsHashIntervalTick(60) && lockoutTicksRemaining > 0)
            {
                lockoutTicksRemaining--;
            }
        }

        public void GeneratePasswords()
        {
            displayedWords = new List<string>(Props.wordCount);
            var usedWords = new HashSet<string>();
            
            correctPassword = GenerateRandomWord(Props.wordLength);
            displayedWords.Add(correctPassword);
            usedWords.Add(correctPassword);
            
            int[] desiredLikeness = new int[Props.wordCount - 1];
            for (int i = 0; i < desiredLikeness.Length; i++)
            {
                desiredLikeness[i] = Rand.Range(0, Props.wordLength);
            }
            
            int attempts = 0;
            int maxAttempts = Props.wordCount * 50;
            
            while (displayedWords.Count < Props.wordCount && attempts < maxAttempts)
            {
                attempts++;
                string candidate = GenerateRandomWord(Props.wordLength);
                
                if (usedWords.Contains(candidate))
                    continue;
                
                int likeness = CalculateLikeness(candidate, correctPassword);
                int targetIndex = displayedWords.Count - 1;
                
                if (targetIndex < desiredLikeness.Length && 
                    (likeness == desiredLikeness[targetIndex] || attempts > maxAttempts / 2))
                {
                    displayedWords.Add(candidate);
                    usedWords.Add(candidate);
                }
            }
            
            while (displayedWords.Count < Props.wordCount)
            {
                string word = GenerateRandomWord(Props.wordLength);
                if (!usedWords.Contains(word))
                {
                    displayedWords.Add(word);
                    usedWords.Add(word);
                }
            }
            
            displayedWords.Shuffle();
        }

        private string GenerateRandomWord(int length)
        {
            if (wordPools.TryGetValue(length, out var pool) && pool.Count > 0)
                return pool.RandomElement();
            
            return GenText.RandomSeedString(length).ToUpper();
        }

        private int CalculateLikeness(string word1, string word2)
        {
            int matches = 0;
            int minLength = word1.Length < word2.Length ? word1.Length : word2.Length;
            for (int i = 0; i < minLength; i++)
            {
                if (word1[i] == word2[i])
                    matches++;
            }
            return matches;
        }

        public int CheckLikeness(string guess)
        {
            int matches = 0;
            int minLength = guess.Length < correctPassword.Length ? guess.Length : correctPassword.Length;
            for (int i = 0; i < minLength; i++)
            {
                if (guess[i] == correctPassword[i])
                    matches++;
            }
            return matches;
        }

        public bool TryHack(string guess)
        {
            if (guess == correctPassword)
            {
                isLocked = false;
                GiveReward();
                return true;
            }

            attemptsRemaining--;
            if (attemptsRemaining <= 0)
            {
                ApplyLockout();
            }
            return false;
        }

        public void RemoveDud(string word)
        {
            int index = displayedWords.IndexOf(word);
            if (index >= 0 && word != correctPassword)
            {
                displayedWords[index] = new string('.', word.Length);
            }
        }

        public void RestoreAttempt()
        {
            attemptsRemaining = Props.maxAttempts;
        }

        private void ApplyLockout()
        {
            lockoutTicksRemaining = Props.lockoutDurationHours * 2500;
            GeneratePasswords();
            attemptsRemaining = Props.maxAttempts;
        }

        private void GiveReward()
        {
            if (Props.rewardPool == null || Props.rewardPool.Count == 0)
                return;

            var rewardDef = Props.rewardPool.RandomElement();
            var reward = ThingMaker.MakeThing(rewardDef);
            
            Building_Terminal terminal = parent as Building_Terminal;
            IntVec3 dropPos = terminal?.InteractionCell ?? parent.Position;
            
            GenPlace.TryPlaceThing(reward, dropPos, parent.Map, ThingPlaceMode.Near);
            Messages.Message("FCP_TerminalHackSuccess".Translate(reward.Label), reward, MessageTypeDefOf.PositiveEvent);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isLocked, "isLocked", true);
            Scribe_Values.Look(ref lockoutTicksRemaining, "lockoutTicksRemaining", 0);
            Scribe_Values.Look(ref correctPassword, "correctPassword");
            Scribe_Values.Look(ref attemptsRemaining, "attemptsRemaining", Props.maxAttempts);
            Scribe_Collections.Look(ref displayedWords, "displayedWords", LookMode.Value);
        }
    }
}
