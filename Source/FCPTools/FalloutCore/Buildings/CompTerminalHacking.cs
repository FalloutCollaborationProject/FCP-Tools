namespace FCP.Core.Buildings;

public class CompTerminalHacking : ThingComp
{
    private static readonly Dictionary<int, List<string>> wordPools = new Dictionary<int, List<string>>();

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
            
        correctPassword = GenerateRandomWord(Props.wordLength, usedWords);
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
            string candidate = GenerateRandomWord(Props.wordLength, usedWords);
                
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
            string word = GenerateRandomWord(Props.wordLength, usedWords);
            if (!usedWords.Contains(word))
            {
                displayedWords.Add(word);
                usedWords.Add(word);
            }
        }
            
        displayedWords.Shuffle();
    }

    private static List<string> GetWordPool(int length)
    {
        if (wordPools.TryGetValue(length, out List<string> pool))
            return pool;

        if (Translator.TryGetTranslatedStringsForFile($"Words/HackingMinigame/words_{length}", out List<string> strings))
        {
            pool = strings
                .Select(word => word.Trim().ToUpperInvariant())
                .Where(word => word.Length == length)
                .Distinct()
                .ToList();
        }
        else
        {
            pool = [];
            FCPLog.Error($"Failed to get a word pool for length {length} at Words/HackingMinigame/words_{length}");
        }

        wordPools[length] = pool;
        return pool;
    }

    private string GenerateRandomWord(int length, HashSet<string> usedWords)
    {
        List<string> pool = GetWordPool(length);
        if (pool.Count > 0)
        {
            var unusedWords = pool.Where(word => !usedWords.Contains(word)).ToList();
            if (unusedWords.Count > 0)
                return unusedWords.RandomElement();
        }

        return GenerateRandomLetters(length, usedWords);
    }

    private string GenerateRandomLetters(int length, HashSet<string> usedWords)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string word;
        do
        {
            char[] result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = chars[Rand.Range(0, chars.Length)];
            }
            word = new string(result);
        }
        while (usedWords.Contains(word));

        return word;
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

        ThingDef rewardDef = Props.rewardPool.RandomElement();
        Thing reward = ThingMaker.MakeThing(rewardDef);
            
        var terminal = parent as Building_Terminal;
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