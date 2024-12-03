using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.Grammar;
using Verse;
using UnityEngine;


namespace FCP_RadiantQuests
{
    public class SitePartWorker_PawnRescue : SitePartWorker
    {
        public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
        {
            base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
            int enemiesCount = GetEnemiesCount(part.site, part.parms);
            outExtraDescriptionRules.Add(new Rule_String("enemiesCount", enemiesCount.ToString()));
            outExtraDescriptionRules.Add(new Rule_String("enemiesLabel", GetEnemiesLabel(part.site, enemiesCount)));
            Pawn pawn = PawnRescueUtility.GeneratePrisoner(part.site.Tile, slate.Get<PawnKindDef>("prisonerPawnKind", PawnKindDefOf.Slave), slate.Get<Faction>("prisonerFaction"));
            Log.Message(slate.Get<float>("chanceToJoin"));
            Log.Message(slate.Get<float>("chanceToJoinVal"));
            if (slate.Get<float>("chanceToJoin") >= slate.Get<float>("chanceToJoinVal"))
            {
                Log.Message("Pawn will join");
                PawnRescueUtility.pawnWillJoin = true;
            }
            part.things = new ThingOwner<Pawn>(part, oneStackOnly: true);
            part.things.TryAdd(pawn);
            PawnRelationUtility.Notify_PawnsSeenByPlayer(Gen.YieldSingle(pawn), out var pawnRelationsInfo, informEvenIfSeenBefore: true, writeSeenPawnsNames: false);
            string output = (pawnRelationsInfo.NullOrEmpty() ? "" : ((string)("\n\n" + "PawnHasTheseRelationshipsWithColonists".Translate(pawn.LabelShort, pawn) + "\n\n" + pawnRelationsInfo)));
            slate.Set("prisoner", pawn);
            outExtraDescriptionRules.Add(new Rule_String("prisonerFullRelationInfo", output));
        }
        public static readonly SimpleCurve ThreatPointsLootMarketValue = new SimpleCurve
    {
        new CurvePoint(100f, 200f),
        new CurvePoint(250f, 450f),
        new CurvePoint(800f, 1000f),
        new CurvePoint(10000f, 2000f)
    };

        public override string GetArrivedLetterPart(Map map, out LetterDef preferredLetterDef, out LookTargets lookTargets)
        {
            string arrivedLetterPart = base.GetArrivedLetterPart(map, out preferredLetterDef, out lookTargets);
            lookTargets = new LookTargets(map.Parent);
            return arrivedLetterPart;
        }
        public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
        {
            string text = base.GetPostProcessedThreatLabel(site, sitePart) + ": " + "KnownSiteThreatEnemyCountAppend".Translate(GetEnemiesCount(site, sitePart.parms), "Enemies".Translate());
            if (sitePart.things != null && sitePart.things.Any)
            {
                text = text + "\nPrisoners: " + sitePart.things[0].LabelShortCap;
            }
            if (site.HasWorldObjectTimeout)
            {
                text += " (" + "DurationLeft".Translate(site.WorldObjectTimeoutTicksLeft.ToStringTicksToPeriod()) + ")";
            }
            return text;
        }

        public override SitePartParams GenerateDefaultParams(float myThreatPoints, int tile, Faction faction)
        {
            SitePartParams sitePartParams = base.GenerateDefaultParams(myThreatPoints, tile, faction);
            sitePartParams.threatPoints = Mathf.Max(sitePartParams.threatPoints, faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Settlement));
            sitePartParams.lootMarketValue = ThreatPointsLootMarketValue.Evaluate(sitePartParams.threatPoints);
            return sitePartParams;
        }

        protected int GetEnemiesCount(Site site, SitePartParams parms)
        {
            return PawnGroupMakerUtility.GeneratePawnKindsExample(new PawnGroupMakerParms
            {
                tile = site.Tile,
                faction = site.Faction,
                groupKind = PawnGroupKindDefOf.Settlement,
                points = parms.threatPoints,
                inhabitants = true,
                seed = OutpostSitePartUtility.GetPawnGroupMakerSeed(parms)
            }).Count();
        }

        protected string GetEnemiesLabel(Site site, int enemiesCount)
        {
            if (site.Faction == null)
            {
                return (enemiesCount == 1) ? "Enemy".Translate() : "Enemies".Translate();
            }
            if (enemiesCount != 1)
            {
                return site.Faction.def.pawnsPlural;
            }
            return site.Faction.def.pawnSingular;
        }
    }
}
