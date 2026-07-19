using System;
using System.Linq;
using RimWorld;
using Verse;

namespace FCP.Core.Robotics
{
    public class CompProperties_SecuritronFace : CompProperties
    {
        public CompProperties_SecuritronFace()
        {
            compClass = typeof(CompSecuritronFace);
        }
    }

    public class CompSecuritronFace : ThingComp
    {
        private HediffDef chosenFace = HediffDefOf_Securitron.FCP_Hediff_Securitron_Face_Army;
        private bool didInitialSetup;

        public CompProperties_SecuritronFace Props => (CompProperties_SecuritronFace)props;

        public static HediffDef[] AllFaces => new[]
        {
            HediffDefOf_Securitron.FCP_Hediff_Securitron_Face_Empty,
            HediffDefOf_Securitron.FCP_Hediff_Securitron_Face_Corrupt,
            HediffDefOf_Securitron.FCP_Hediff_Securitron_Face_Army,
            HediffDefOf_Securitron.FCP_Hediff_Securitron_Face_Cop,
            HediffDefOf_Securitron.FCP_Hediff_Securitron_Face_Female,
            HediffDefOf_Securitron.FCP_Hediff_Securitron_Face_Male,
            HediffDefOf_Securitron.FCP_Hediff_Securitron_Face_Smily,
            HediffDefOf_Securitron.FCP_Hediff_Securitron_Face_Victor,
        };

        public static HediffDef[] PlayerSelectableFaces => new[]
        {
            HediffDefOf_Securitron.FCP_Hediff_Securitron_Face_Army,
            HediffDefOf_Securitron.FCP_Hediff_Securitron_Face_Cop,
            HediffDefOf_Securitron.FCP_Hediff_Securitron_Face_Female,
            HediffDefOf_Securitron.FCP_Hediff_Securitron_Face_Male,
        };

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (respawningAfterLoad)
            {
                didInitialSetup = true;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (!didInitialSetup)
            {
                didInitialSetup = true;

                SecuritronPresetExtension preset = (parent as Pawn)?.kindDef.GetModExtension<SecuritronPresetExtension>();
                if (preset?.face != null)
                {
                    chosenFace = preset.face;
                }
                else
                {
                    chosenFace = Rand.Bool
                        ? HediffDefOf_Securitron.FCP_Hediff_Securitron_Face_Male
                        : HediffDefOf_Securitron.FCP_Hediff_Securitron_Face_Female;
                }

                RefreshFace();
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref chosenFace, "securitronChosenFace");
            Scribe_Values.Look(ref didInitialSetup, "securitronFaceInitialSetupDone");
        }

        public override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);
            if (signal == "RanOutOfFuel" || signal == "Refueled")
            {
                RefreshFace();
            }
        }

        public HediffDef CurrentDisplayedFace(Pawn pawn)
        {
            BodyPartRecord part = FindScreenPart(pawn);
            if (part == null || pawn.health?.hediffSet == null)
            {
                return null;
            }

            Hediff hediff = pawn.health.hediffSet.hediffs.FirstOrDefault(h => h.Part == part && Array.IndexOf(AllFaces, h.def) >= 0);
            return hediff?.def;
        }

        public void SetFace(HediffDef faceDef)
        {
            chosenFace = faceDef;
            RefreshFace();
        }

        public void Notify_FactionChanged()
        {
            RefreshFace();
        }

        public void RefreshFace()
        {
            Pawn pawn = parent as Pawn;
            if (pawn?.health == null)
            {
                return;
            }

            HediffDef display;
            if (pawn.Faction != null && pawn.Faction != Faction.OfPlayer)
            {
                display = HediffDefOf_Securitron.FCP_Hediff_Securitron_Face_Corrupt;
            }
            else
            {
                CompRefuelable fuel = pawn.GetComp<CompRefuelable>();
                bool poweredOff = !RobotUtility.IsPoweredOn(pawn) || (fuel != null && !fuel.HasFuel);
                display = poweredOff
                    ? HediffDefOf_Securitron.FCP_Hediff_Securitron_Face_Empty
                    : chosenFace;
            }

            if (CurrentDisplayedFace(pawn) == display)
            {
                return;
            }

            BodyPartRecord part = FindScreenPart(pawn);
            if (part == null)
            {
                return;
            }

            Hediff existing = pawn.health.hediffSet.hediffs.FirstOrDefault(h => h.Part == part && Array.IndexOf(AllFaces, h.def) >= 0);
            if (existing != null)
            {
                pawn.health.RemoveHediff(existing);
            }
            pawn.health.AddHediff(display, part);
            pawn.Drawer.renderer.SetAllGraphicsDirty();
        }

        private static BodyPartRecord FindScreenPart(Pawn pawn)
        {
            return pawn.RaceProps.body.AllParts.FirstOrDefault(p => p.groups.Contains(BodyPartGroupDefOf_Securitron.SecuritronScreen));
        }
    }
}
