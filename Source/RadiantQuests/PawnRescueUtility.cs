using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FCP_RadiantQuests
{
    public static class PawnRescueUtility
    {
        public static bool pawnWillJoin = false;
        public static Pawn GeneratePrisoner(int tile, PawnKindDef pawnKindDef, Faction hostFaction)
        {
            PawnGenerationRequest request = new PawnGenerationRequest(pawnKindDef, hostFaction, PawnGenerationContext.NonPlayer, tile, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 75f, forceAddFreeWarmLayerIfNeeded: true, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: true, worldPawnFactionDoesntMatter: true);
            if (Find.Storyteller.difficulty.ChildrenAllowed)
            {
                request.AllowedDevelopmentalStages |= DevelopmentalStage.Child;
            }
            Pawn pawn = PawnGenerator.GeneratePawn(request);
            pawn.guest.SetGuestStatus(hostFaction, GuestStatus.Prisoner);
            return pawn;
        }
    }
}
