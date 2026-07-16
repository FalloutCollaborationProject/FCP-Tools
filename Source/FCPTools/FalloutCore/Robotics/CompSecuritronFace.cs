using RimWorld;
using Verse;

namespace FCP.Core.Robotics
{
    [DefOf]
    public static class ThingDefOf_SecuritronFace
    {
        [MayRequire("Rick.FCP.Robotics")]
        public static ThingDef FCP_Apparel_Securitron_Face_Empty;
        [MayRequire("Rick.FCP.Robotics")]
        public static ThingDef FCP_Apparel_Securitron_Face_Corrupt;
        [MayRequire("Rick.FCP.Robotics")]
        public static ThingDef FCP_Apparel_Securitron_Face_Army;
        [MayRequire("Rick.FCP.Robotics")]
        public static ThingDef FCP_Apparel_Securitron_Face_Cop;
        [MayRequire("Rick.FCP.Robotics")]
        public static ThingDef FCP_Apparel_Securitron_Face_Female;
        [MayRequire("Rick.FCP.Robotics")]
        public static ThingDef FCP_Apparel_Securitron_Face_Male;
        [MayRequire("Rick.FCP.Robotics")]
        public static ThingDef FCP_Apparel_Securitron_Face_Smily;
        [MayRequire("Rick.FCP.Robotics")]
        public static ThingDef FCP_Apparel_Securitron_Face_Victor;

        static ThingDefOf_SecuritronFace()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf_SecuritronFace));
        }
    }

    public class CompProperties_SecuritronFace : CompProperties
    {
        public CompProperties_SecuritronFace()
        {
            compClass = typeof(CompSecuritronFace);
        }
    }

    public class CompSecuritronFace : ThingComp
    {
        private ThingDef chosenFace = ThingDefOf_SecuritronFace.FCP_Apparel_Securitron_Face_Army;
        private bool didInitialSetup;

        public CompProperties_SecuritronFace Props => (CompProperties_SecuritronFace)props;

        public static ThingDef[] AllFaces => new[]
        {
            ThingDefOf_SecuritronFace.FCP_Apparel_Securitron_Face_Empty,
            ThingDefOf_SecuritronFace.FCP_Apparel_Securitron_Face_Corrupt,
            ThingDefOf_SecuritronFace.FCP_Apparel_Securitron_Face_Army,
            ThingDefOf_SecuritronFace.FCP_Apparel_Securitron_Face_Cop,
            ThingDefOf_SecuritronFace.FCP_Apparel_Securitron_Face_Female,
            ThingDefOf_SecuritronFace.FCP_Apparel_Securitron_Face_Male,
            ThingDefOf_SecuritronFace.FCP_Apparel_Securitron_Face_Smily,
            ThingDefOf_SecuritronFace.FCP_Apparel_Securitron_Face_Victor,
        };

        public static ThingDef[] PlayerSelectableFaces => new[]
        {
            ThingDefOf_SecuritronFace.FCP_Apparel_Securitron_Face_Army,
            ThingDefOf_SecuritronFace.FCP_Apparel_Securitron_Face_Cop,
            ThingDefOf_SecuritronFace.FCP_Apparel_Securitron_Face_Female,
            ThingDefOf_SecuritronFace.FCP_Apparel_Securitron_Face_Male,
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
                        ? ThingDefOf_SecuritronFace.FCP_Apparel_Securitron_Face_Male
                        : ThingDefOf_SecuritronFace.FCP_Apparel_Securitron_Face_Female;
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

        public ThingDef CurrentDisplayedFace(Pawn pawn)
        {
            foreach (Apparel apparel in pawn.apparel.WornApparel)
            {
                if (System.Array.IndexOf(AllFaces, apparel.def) >= 0)
                {
                    return apparel.def;
                }
            }
            return null;
        }

        public void SetFace(ThingDef faceDef)
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
            if (pawn?.apparel == null)
            {
                return;
            }

            ThingDef display;
            if (pawn.Faction != null && pawn.Faction != Faction.OfPlayer)
            {
                display = ThingDefOf_SecuritronFace.FCP_Apparel_Securitron_Face_Corrupt;
            }
            else
            {
                CompRefuelable fuel = pawn.GetComp<CompRefuelable>();
                display = (fuel != null && !fuel.HasFuel)
                    ? ThingDefOf_SecuritronFace.FCP_Apparel_Securitron_Face_Empty
                    : chosenFace;
            }

            if (CurrentDisplayedFace(pawn) == display)
            {
                return;
            }

            Apparel newFace = (Apparel)ThingMaker.MakeThing(display);
            pawn.apparel.Wear(newFace, dropReplacedApparel: false);
            RobotUtility.TouchGraphic(newFace);
        }
    }
}
