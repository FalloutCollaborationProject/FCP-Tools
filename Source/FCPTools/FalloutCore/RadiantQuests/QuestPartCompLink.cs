namespace FCP.Core.RadiantQuests;

public static class QuestPartCompLink
{
    public static int PartIndex(QuestPart part)
    {
        return part?.quest == null ? -1 : part.quest.PartsListForReading.IndexOf(part);
    }

    public static T Resolve<T>(int questId, int partIndex, Predicate<T> unlinked) where T : QuestPart
    {
        List<Quest> quests = Find.QuestManager.QuestsListForReading;
        if (questId >= 0)
        {
            for (int i = 0; i < quests.Count; i++)
            {
                if (quests[i].id != questId)
                    continue;

                List<QuestPart> parts = quests[i].PartsListForReading;
                return partIndex >= 0 && partIndex < parts.Count ? parts[partIndex] as T : null;
            }
            return null;
        }

        for (int i = 0; i < quests.Count; i++)
        {
            List<QuestPart> parts = quests[i].PartsListForReading;
            for (int j = 0; j < parts.Count; j++)
            {
                if (parts[j] is T part && unlinked(part))
                    return part;
            }
        }
        return null;
    }
}
