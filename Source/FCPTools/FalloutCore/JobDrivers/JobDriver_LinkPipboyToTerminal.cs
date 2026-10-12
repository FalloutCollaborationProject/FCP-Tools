using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace FCP.Core.JobDrivers
{
    public class JobDriver_LinkPipboyToTerminal : JobDriver
    {
        private Buildings.Building_Terminal Terminal => job.GetTarget(TargetIndex.A).Thing as Buildings.Building_Terminal;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Terminal, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOnForbidden(TargetIndex.A);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            Toil linkToil = Toils_General.Wait(120);
            linkToil.WithProgressBarToilDelay(TargetIndex.A);
            linkToil.defaultCompleteMode = ToilCompleteMode.Delay;
            yield return linkToil;

            Toil syncToil = new Toil();
            syncToil.initAction = () =>
            {
                if (pawn.apparel == null)
                    return;

                Apparel pipboy = pawn.apparel.WornApparel.Find(a => a.TryGetComp<Holotapes.CompPipboyHolotapeStorage>() != null);
                if (pipboy == null)
                    return;

                Holotapes.CompPipboyHolotapeStorage pipboyStorage = pipboy.TryGetComp<Holotapes.CompPipboyHolotapeStorage>();
                Buildings.CompHolotapeStorage terminalStorage = Terminal?.GetComp<Buildings.CompHolotapeStorage>();
                if (pipboyStorage == null || terminalStorage == null)
                    return;

                int synced = 0;

                for (int i = 0; i < terminalStorage.StoredHolotapes.Count; i++)
                {
                    ThingDef tapeDef = terminalStorage.StoredHolotapes[i].def;
                    if (!HasTape(pipboyStorage.StoredHolotapes, tapeDef))
                    {
                        pipboyStorage.TryStoreHolotape(ThingMaker.MakeThing(tapeDef));
                        synced++;
                    }
                }

                for (int i = 0; i < pipboyStorage.StoredHolotapes.Count; i++)
                {
                    ThingDef tapeDef = pipboyStorage.StoredHolotapes[i].def;
                    if (!HasTape(terminalStorage.StoredHolotapes, tapeDef))
                    {
                        terminalStorage.TryStoreHolotape(ThingMaker.MakeThing(tapeDef));
                        synced++;
                    }
                }

                if (synced > 0)
                    Messages.Message(pawn.LabelShort + " synced " + synced + " holotape(s).", pawn, MessageTypeDefOf.PositiveEvent);
                else
                    Messages.Message(pawn.LabelShort + "'s Pip-Boy is already synced.", pawn, MessageTypeDefOf.NeutralEvent);
            };
            yield return syncToil;
        }

        private static bool HasTape(List<Thing> tapes, ThingDef tapeDef)
        {
            for (int i = 0; i < tapes.Count; i++)
            {
                if (tapes[i].def == tapeDef)
                    return true;
            }
            return false;
        }
    }
}
