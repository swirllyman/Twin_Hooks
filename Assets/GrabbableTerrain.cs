using UnityEngine;

public class GrabbableTerrain : Grabbable
{
    public delegate void PickUpCallback();
    public event PickUpCallback onPickup;

    public override void PickUp()
    {
        //base.PickUp();
        onPickup?.Invoke();

    }


    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
