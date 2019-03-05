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
using System.Collections;
using System.Collections.Generic;
using SKStudios.Portals.Demos;
using UnityEngine;
using VRTK;

namespace SKStudios.Portals
{
    public class VRTKDelayedTeleportable : MonoBehaviour
    {
        private VRTK_BodyPhysics bodyPhysics;

        // Use this for initialization
        void Start()
        {
            if(gameObject.activeInHierarchy)
                StartCoroutine(DelayedSetup());
        }

        IEnumerator DelayedSetup() {
           
            GameObject headObj = null;
            while (headObj == null) {
                Transform bodytransform = VRTK_DeviceFinder.HeadsetCamera();
                if (bodytransform)
                    headObj = bodytransform.gameObject;
                yield return new WaitForEndOfFrame();
            }

            AutomaticCameraLayerSet layerSet = headObj.AddComponent<AutomaticCameraLayerSet>();
            layerSet.ExcludedLayers = new List<string>();
            layerSet.ExcludedLayers.Add("PortalOnly");
            layerSet.ExcludedLayers.Add("PortalPlaceholder");
            layerSet.ExcludedLayers.Add("RenderExclude");


            GameObject bodyObj = null;
            while (bodyObj == null)
            {
                Transform bodytransform = VRTK_DeviceFinder.PlayAreaTransform();
                if (bodytransform)
                    bodyObj = bodytransform.gameObject;
                yield return new WaitForEndOfFrame();
            }


            bodyObj.transform.position = transform.position;

            yield return new WaitForSeconds(2f);

            Teleportable teleportable = headObj.AddComponent<Teleportable>();
            teleportable.enabled = false;
            teleportable.Root = transform;
            teleportable.IsActive = true;
            teleportable.VisOnly = false;
            teleportable.enabled = true;
            //teleportable.SpecialRoot = true;

            
            Collider col;
            if (!(col = headObj.GetComponent<Collider>()))
            {
                col = headObj.AddComponent<BoxCollider>();
                ((BoxCollider)col).size = new Vector3(0.25f, 0.25f, 0.25f);
                col.isTrigger = true;
            }

            Rigidbody headRB = null;
            if (!(headRB = headObj.GetComponent<Rigidbody>()))
                headRB = headObj.AddComponent<Rigidbody>();
            headRB.isKinematic = true;


            PlayerTeleportable playerTeleportable = headObj.AddComponent<PlayerTeleportable>();

            SelfRightingPlayer selfRight = bodyObj.AddComponent<SelfRightingPlayer>();
            selfRight.MoveSpeed = 4;
        }
    }
}

#endif