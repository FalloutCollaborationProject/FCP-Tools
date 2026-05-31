using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace FCP.Core.Holotapes
{
    public class CompProperties_Holotape : CompProperties
    {
        public HolotapeDef contentDef;

        public CompProperties_Holotape()
        {
            compClass = typeof(CompHolotape);
        }
    }

    public class CompHolotape : ThingComp
    {
        private bool hasBeenRead;
        private bool skillAlreadyGranted;

        public CompProperties_Holotape Props => (CompProperties_Holotape)props;
        public HolotapeDef ContentDef => Props.contentDef;
        public bool HasBeenRead => hasBeenRead;
        public bool SkillAlreadyGranted => skillAlreadyGranted;

        public void MarkAsRead()
        {
            hasBeenRead = true;
        }

        public void GrantSkill()
        {
            skillAlreadyGranted = true;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref hasBeenRead, "hasBeenRead", false);
            Scribe_Values.Look(ref skillAlreadyGranted, "skillAlreadyGranted", false);
        }

        public override string CompInspectStringExtra()
        {
            HolotapeDef def = ContentDef;
            if (def == null)
                return null;

            TaggedString str = "Category: " + def.category;
            if (!def.author.NullOrEmpty())
                str += "\nAuthor: " + def.author;
            if (hasBeenRead)
                str += "\n(Read)";
            
            return str;
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (selPawn.apparel == null)
                yield break;
            
            bool hasPipboy = false;
            List<Apparel> worn = selPawn.apparel.WornApparel;
            for (int i = 0; i < worn.Count; i++)
            {
                if (worn[i].TryGetComp<CompPipboyHolotapeStorage>() != null)
                {
                    hasPipboy = true;
                    break;
                }
            }
            if (!hasPipboy)
                yield break;
            
            if (!selPawn.CanReach(parent, PathEndMode.Touch, Danger.Deadly))
                yield return new FloatMenuOption("CannotUseNoPath".Translate(), null);
            else if (!selPawn.CanReserve(parent))
                yield return new FloatMenuOption("CannotUseReserved".Translate(), null);
            else
                yield return new FloatMenuOption("Load into Pip-Boy", () => selPawn.jobs.TryTakeOrderedJob(
                    JobMaker.MakeJob(Buildings.JobDefOf_Terminal.FCP_LoadHolotapeIntoPipboy, parent)));
        }
    }
}
