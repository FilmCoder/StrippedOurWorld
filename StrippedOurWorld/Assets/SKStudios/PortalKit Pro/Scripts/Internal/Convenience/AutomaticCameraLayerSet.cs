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

using System.Collections.Generic;
using UnityEngine;

namespace SKStudios.Portals {
    [RequireComponent(typeof(Camera))]
    public class AutomaticCameraLayerSet : MonoBehaviour {
        private new Camera _camera;
        public List<string> ExcludedLayers;

        // Use this for initialization
        private void Start()
        {
            _camera = gameObject.GetComponent<Camera>();
            var mask = _camera.cullingMask;
            foreach (var l in ExcludedLayers) mask &= ~(1 << LayerMask.NameToLayer(l));
            _camera.cullingMask = mask;
        }
    }
}