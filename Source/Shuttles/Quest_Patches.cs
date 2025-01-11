using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace FCP_Shuttles
{

    [HarmonyPatch(typeof(QuestNode_SpawnSkyfaller), "RunInt")]
    public static class Patch_QuestNode_SpawnSkyfaller
    {
        public static void Prefix(QuestNode_SpawnSkyfaller __instance)
        {
            Slate slate = QuestGen.slate;
            if (WorldComponent_QuestTracker.Instance.NeedToReplaceShuttle(QuestGen.quest, out var factionModExtension))
            {
                Log.Message(QuestGen.quest + " - " + factionModExtension.customShuttleIncoming);
                slate.Set(nameof(__instance.skyfallerDef), factionModExtension.customShuttleIncoming);
            }
        }
    }

    [HarmonyPatch]
    public static class SlateSetPatch
    {
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(Slate), "Set").MakeGenericMethod(new Type[]
            {
                typeof(object)
            });
        }
        public static void Prefix(string name, object var, bool isAbsoluteName = false)
        {
            if (QuestGen.quest != null)
            {
                if (var is Pawn pawn && name == "asker" && pawn.Faction != null)
                {
                    TryRegisterCustomShuttle(pawn.Faction, QuestGen.quest);
                }
                else if (var is Faction faction && name == "askerFaction")
                {
                    TryRegisterCustomShuttle(faction, QuestGen.quest);
                }
            }
        }

        private static void TryRegisterCustomShuttle(Faction faction, Quest quest)
        {
            if (faction != null)
            {
                var options = faction.def.GetModExtension<FactionModExtension>();
                if (options != null)
                {
                    if (options.customShuttle != null)
                    {
                        var questTracker = WorldComponent_QuestTracker.Instance;
                        if (!questTracker.storedQuests.ContainsKey(quest))
                        {
                            questTracker.storedQuests[quest] = faction;
                        }
                        questTracker.ReplaceShuttle(options);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(QuestGen))]
    [HarmonyPatch("Generate")]
    public static class Patch_Generate
    {
        public static void Postfix()
        {
            WorldComponent_QuestTracker.Instance.RestoreShuttle();
        }
    }

    [HarmonyPatch(typeof(Quest))]
    [HarmonyPatch("QuestTick")]
    public static class Patch_QuestTick
    {
        public static void Prefix(Quest __instance, out bool __state)
        {
            if (WorldComponent_QuestTracker.Instance.NeedToReplaceShuttle(__instance, out var factionModExtension))
            {
                __state = true;
                WorldComponent_QuestTracker.Instance.ReplaceShuttle(factionModExtension);
            }
            else
            {
                __state = false;
            }
        }
        public static void Postfix(bool __state)
        {
            if (__state)
            {
                WorldComponent_QuestTracker.Instance.RestoreShuttle();
            }
        }
    }

    [HarmonyPatch(typeof(Quest))]
    [HarmonyPatch("Notify_SignalReceived")]
    public static class Patch_Notify_SignalReceived
    {
        public static void Prefix(Quest __instance, out bool __state)
        {
            if (WorldComponent_QuestTracker.Instance.NeedToReplaceShuttle(__instance, out var factionModExtension))
            {
                __state = true;
                WorldComponent_QuestTracker.Instance.ReplaceShuttle(factionModExtension);
            }
            else
            {
                __state = false;
            }
        }
        public static void Postfix(bool __state)
        {
            if (__state)
            {
                WorldComponent_QuestTracker.Instance.RestoreShuttle();
            }
        }
    }
}