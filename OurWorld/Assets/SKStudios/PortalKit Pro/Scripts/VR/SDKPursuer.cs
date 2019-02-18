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

using UnityEngine;

namespace SKStudios.Portals {
    /// <summary>
    ///     This class fixes issues with newer versions of VRTK and SteamVR where teleportation was broken.
    /// </summary>
    public class SdkPursuer : TeleportableScript {
        public GameObject Sdk;


        public override void Initialize(Teleportable t) { }

        public override void CustomUpdate()
        {
            //We don't want to use this at all
        }

        public override void LeavePortal() { }

        public override void Teleport() { }

        public override void OnTeleport() { }

        private void Start()
        {
            TeleportScriptIndependantly = false;
        }

        private void LateUpdate()
        {
            var position = Sdk.transform.position;
            var rotation = Sdk.transform.rotation;
            var diffVector = position - transform.position;
            transform.position = position;
            transform.rotation = rotation;
            Sdk.transform.position = Sdk.transform.position - diffVector;
            Sdk.transform.rotation = rotation;
        }
    }
}