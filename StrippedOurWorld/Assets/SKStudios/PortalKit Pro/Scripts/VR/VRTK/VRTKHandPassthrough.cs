// © 2018, SKStudios LLC. All Rights Reserved.
// 
// The software, artwork and data, both as individual files and as a complete software package known as 'PortalKit Pro', 
// without regard to source or channel of acquisition, are bound by the terms and conditions set forth in the Unity Asset 
// Store license agreement in addition to the following terms;
// 
// One license per seat is required for Companies, teams, studios or collaborations using PortalKit Pro that have over 
// 10 members or that make more than $50,000 USD per year. 
// 
// Addendum;
// If PortalKitPro constitutes a major portion of your game's mechanics, please consider crediting the software and/or SKStudios.
// You are in no way obligated to do so, but it would be sincerely appreciated.

#if SKS_VR
using SKStudios.Portals;
using System.Collections;
using SKStudios.Portals;
using UnityEngine;
using VRTK;

[RequireComponent(typeof(VRTK_ControllerEvents))]
public class VRTKHandPassthrough : TeleportableScript {
    VRTK_InteractGrab grab;
    // Use this for initialization
    void Start() {
        grab = gameObject.GetComponent<VRTK_InteractGrab>();
    }

    // Update is called once per frame
    public override void CustomUpdate() {
        base.CustomUpdate();
    }

    public override void OnTeleport() {
        
        if (!this.isActiveAndEnabled)
            return;

        Teleportable grabTele;

        if (grab.GetGrabbedObject() && 
            (grabTele = grab.GetGrabbedObject().GetComponent<Teleportable>())) {
            grabTele.UpdateDoppleganger();

            if(grab && grab.GetGrabbedObject() && Teleportable.Doppleganger && CurrentPortal)
            PortalUtils.TeleportObject(grab.GetGrabbedObject(), CurrentPortal.Origin, CurrentPortal.ArrivalTarget, Teleportable.Doppleganger.transform, null, false);
        }
        if (grab) {
            grab.ForceRelease();
            StartCoroutine(OnPostTeleport());
            //grab.AttemptGrab();
        }

       
       
    }

    private IEnumerator OnPostTeleport() {
        
        yield return new WaitForFixedUpdate();
        if (grab) {
            grab.AttemptGrab();
        }
    }
   
}
#endif