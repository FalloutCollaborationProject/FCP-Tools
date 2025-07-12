using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace  FCP.Enlist
{
	public class FactionEnlistWorker
	{
		public FactionEnlistOptionsDef def;
		public FactionEnlistWorker(FactionEnlistOptionsDef def)
		{
			this.def = def;
		}

		public virtual bool CanEnlistTo(Faction toEnlist, out string cannotReason)
		{
			cannotReason = "";
			return true;
		}
		public virtual void EnlistTo(Faction toEnlist)
		{
			WorldEnlistTracker tracker = WorldEnlistTracker.Instance;
			if (!tracker.factionOptionsContainer.TryGetValue(toEnlist, out FactionOptions factionOptions))
			{
				factionOptions = new FactionOptions();
				tracker.factionOptionsContainer[toEnlist] = factionOptions;
			}
			factionOptions.factionsRecruiters[def] = true;
			if (def.enlistGoodwillGain != 0)
			{
				var baseGoodwill = toEnlist.GoodwillWith(Faction.OfPlayer);
				toEnlist.ChangeRelation(Mathf.Max(def.minGoodwillUponEnlist, baseGoodwill + def.enlistGoodwillGain));
            }
            if (!def.enlistLetterTitleKey.NullOrEmpty())
			{
				Find.LetterStack.ReceiveLetter(def.enlistLetterTitleKey.Translate(toEnlist.Named("FACTION")),
					def.enlistLetterLabelKey.Translate(toEnlist.Named("FACTION"), toEnlist.leader.Named("LEADER")), LetterDefOf.PositiveEvent);
			}

			SalaryInfo salaryInfo = new SalaryInfo
			{
				lastPaidTick = Find.TickManager.TicksGame
			};
			factionOptions.factionsSalaries[def] = salaryInfo;
			if (!factionOptions.factionsStorages.ContainsKey(def))
			{
				FactionStorage factionStorage = new FactionStorage
				{
					storedThings = new List<Thing>()
				};
				factionOptions.factionsStorages[def] = factionStorage;
			}
			def.enlistedSoundDef?.PlayOneShotOnCamera();
		}


        public virtual bool CanBuy(Faction toBuy, Caravan caravan, out string cannotReason)
        {
            cannotReason = "";
            return true;
        }

        public virtual void Buy(Faction toBuy, Caravan caravan)
        {
            WorldEnlistTracker tracker = WorldEnlistTracker.Instance;
            if (!tracker.factionOptionsContainer.TryGetValue(toBuy, out FactionOptions factionOptions))
            {
                factionOptions = new FactionOptions();
                tracker.factionOptionsContainer[toBuy] = factionOptions;
            }

			var savedRelationships = new List<FactionRelation>();
            foreach (var rel in toBuy.relations)
            {
                savedRelationships.Add(new FactionRelation 
				{ 
					baseGoodwill = rel.baseGoodwill, 
					kind = rel.kind, 
					other = rel.other 
				});
            }

            factionOptions.factionsBought[def] = new FactionState
			{
				factionRelationships = savedRelationships
            };
        }

        public virtual IEnumerable<Gizmo> GetGizmos(Faction faction, int order) 
		{
			yield break;
		}
	}
}
