using UnityEngine;

namespace FCP.Core;

public abstract class AirshipState : IExposable
{
    protected Airship airship;
    
    public AirshipState(Airship airship)
    {
        this.airship = airship;
    }
    
    public AirshipState() { }
    
    // Called after loading to restore the airship reference
    public virtual void PostLoadInit(Airship loadAirship)
    {
        airship = loadAirship;
    }

    public abstract Vector3 GetDrawPosition();
    public abstract void Tick(int delta);
    public abstract void OnEnter();
    public abstract void OnExit();
    
    public virtual void ExposeData()
    {
        
    }
}