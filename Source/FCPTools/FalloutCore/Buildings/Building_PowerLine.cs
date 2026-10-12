using RimWorld;
using Verse;

namespace FCP.Core.Buildings;

public class Building_PowerLine : Building
{
    private Pawn containedPawn;
    private Corpse containedCorpse;
    private Faction storedHostFaction;
    private GuestStatus? storedGuestStatus;
    
    public Pawn ContainedPawn => containedPawn;
    public Corpse ContainedCorpse => containedCorpse;
    
    public bool HasPawn
    {
        get
        {
            if (containedPawn != null && !containedPawn.Destroyed)
            {
                if (containedPawn.Dead)
                {
                    ConvertToCorpse(containedPawn.Corpse);
                    return false;
                }
                return true;
            }
            return false;
        }
    }
    
    public bool HasCorpse => containedCorpse != null && !containedCorpse.Destroyed
        && containedCorpse.Spawned && containedCorpse.Position.InHorDistOf(Position, 3f);

    private void ConvertToCorpse(Corpse corpse)
    {
        containedCorpse = corpse;
        containedPawn = null;
        storedHostFaction = null;
        storedGuestStatus = null;
        if (corpse != null)
            corpse.Rotation = Rotation.Opposite;
    }
    
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref containedPawn, "containedPawn");
        Scribe_References.Look(ref containedCorpse, "containedCorpse");
        Scribe_References.Look(ref storedHostFaction, "storedHostFaction");
        Scribe_Values.Look(ref storedGuestStatus, "storedGuestStatus");
    }
    
    public bool TryAcceptPawn(Pawn p)
    {
        if (HasPawn || HasCorpse || p == null)
            return false;
        containedPawn = p;
        if (p.guest != null && !p.Dead)
        {
            storedHostFaction = p.HostFaction;
            storedGuestStatus = p.guest.GuestStatus;
        }
        return true;
    }
    
    public bool TryAcceptCorpse(Corpse corpse)
    {
        if (HasPawn || HasCorpse || corpse == null)
            return false;
        containedCorpse = corpse;
        return true;
    }
    
    public Pawn EjectContents()
    {
        if (!HasPawn)
            return null;
        Pawn p = containedPawn;
        containedPawn = null;
        if (p.guest != null && !p.Dead && storedGuestStatus != null)
            p.guest.SetGuestStatus(storedHostFaction, storedGuestStatus.Value);
        storedHostFaction = null;
        storedGuestStatus = null;
        return p;
    }
    
    public Corpse EjectCorpse()
    {
        if (!HasCorpse)
            return null;
        Corpse c = containedCorpse;
        containedCorpse = null;
        return c;
    }
    
    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        if (HasPawn)
        {
            Pawn p = EjectContents();
            if (!p.Spawned)
                GenSpawn.Spawn(p, Position, Map);
            if (!p.Dead)
            {
                Hediff crucified = p.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.FCP_Crucified);
                if (crucified != null)
                    p.health.RemoveHediff(crucified);
                Hediff exhaustion = p.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.FCP_CrucifixionExhaustion);
                if (exhaustion != null)
                    p.health.RemoveHediff(exhaustion);
                p.health.AddHediff(HediffDefOf.FCP_WasCrucified);
            }
        }
        if (HasCorpse)
        {
            Corpse c = EjectCorpse();
            if (c != null && !c.Spawned)
                GenSpawn.Spawn(c, Position, Map);
        }
        base.Destroy(mode);
    }
    
    public override string GetInspectString()
    {
        string text = base.GetInspectString();
        if (HasPawn)
        {
            if (!text.NullOrEmpty())
                text += "\n";
            text += "FCP_ContainedPawn".Translate(containedPawn.LabelShort);
        }
        else if (HasCorpse)
        {
            if (!text.NullOrEmpty())
                text += "\n";
            text += "FCP_ContainedCorpse".Translate(containedCorpse.InnerPawn.LabelShort);
        }
        return text;
    }
}
