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


using System;
using System.Reflection;
using SKStudios.Common.Utils;
using SKStudios.Common.Utils.Reflection;
using UnityEngine;

#if SK_UFPS
namespace SKStudios.Portals.Compatibility {
    /// <summary>
    /// Fix for UFPS behavior
    /// </summary>
    public class PortalKitUfpsFix : MonoBehaviour {

        public Teleportable Teleportable;
        public vp_FPController FpController;
        private vp_PlayerEventHandler _player;

        void Start()
        {
            Teleportable.OnTeleport += Teleport;
            foreach (var p in GetVarInfoFast.GetVariableInfos(typeof(vp_Controller)).Item1) {
                if (p.Name == "Player") {
                    _player = (vp_PlayerEventHandler)p.GetValue(FpController, null);
                   
                }
                    
            }

            var ignoredLayers = 1 << Keywords.Layers.CustomRenderer | 
                                1 << Keywords.Layers.CustomRendererOnly |
                                1 << Keywords.Layers.CustomRendererPlaceholder;

            vp_Layer.Mask.ExternalBlockers &= ~ignoredLayers;
            vp_Layer.Mask.IgnoreWalkThru |= ignoredLayers;
            vp_Layer.Mask.BulletBlockers &= ~ignoredLayers;

        }

        void Teleport(Portal origin, Portal destination)
        {
            var rotDiff = Quaternion.Inverse(origin.Origin.rotation) * origin.ArrivalTarget.rotation;
            var rotation = _player.Rotation.Get();
            rotation = (Quaternion.Euler(new Vector3(rotation.x, rotation.y, 0)) * 
                       (rotDiff)).eulerAngles;

            var velocity = _player.Velocity.Get();
            velocity = rotDiff * velocity;

            var motor = _player.MotorThrottle.Get();
            motor = rotDiff * motor;

            //_player.Stop.Send();

            _player.Rotation.Set(rotation);
            _player.MotorThrottle.Set(motor);
            _player.Velocity.Set(velocity);
        }

        private void Update()
        {
            var rotation = _player.Rotation.Get();

            _player.Rotation.Set(rotation);
        }
    }
}
#endif