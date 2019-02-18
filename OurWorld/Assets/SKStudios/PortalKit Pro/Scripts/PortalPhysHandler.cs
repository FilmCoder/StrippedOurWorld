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

using SKStudios.Common;
using SKStudios.Rendering;
using UnityEngine;

namespace SKStudios.Portals {
    public partial class Portal : EffectRenderer {
        private Collider _headCollider;

        private Collider HeadCollider {
            get {
                if (_headCollider == null)
                    _headCollider = Camera.main.GetComponent<Collider>();
                return _headCollider;
            }
        }

        /// <summary>
        /// Find the collider to find the <see cref="Teleportable"/> script on
        /// </summary>
        /// <param name="col">Collider to search from</param>
        /// <returns></returns>
        private Teleportable GetTeleportableFromCollider(Collider col)
        {
            var teleportableBody = col.attachedRigidbody;
            return teleportableBody != null
                ? teleportableBody.GetComponent<Teleportable>()
                : col.GetComponent<Teleportable>();
        }

        /// <summary>
        ///     All collsision methods are externed to another script
        /// </summary>
        public void E_OnTriggerEnter(Collider col)
        {
            if (!Enterable)
                return;
#if !DISABLE_PHYSICS_IGNORE
            Physics.IgnoreCollision(col, PortalCollider);
#endif          
            var teleportScript = GetTeleportableFromCollider(col);

            AddTeleportable(teleportScript);
        }


        /// <summary>
        ///     Checks if objects are in Portal, and teleports them if they are. Also handles player entry.
        /// </summary>
        /// <param name="col"></param>
        public void E_OnTriggerStay(Collider col)
        {
            if (col.gameObject.isStatic) return;
            if (!Enterable) return;
            if (!Target) return;

            var teleportScript = GetTeleportableFromCollider(col);

            if (!AddTeleportable(teleportScript)) return;

            if (teleportScript == PlayerTeleportable.PlayerTeleportableScript) _headInPortalTrigger = true;

            var globalLocalZScale =
                (Quaternion.Inverse(ArrivalTarget.rotation) *
                 Vector3.Scale(ArrivalTarget.forward, Target.transform.lossyScale)).z;
            //Updates clip planes for disappearing effect
            if (!NonObliqueOverride && SKSGlobalRenderSettings.Clipping)
                teleportScript.SetClipPlane(
                    Origin.position,
                    Origin.forward,
                    ArrivalTarget.position - -ArrivalTarget.forward * (globalLocalZScale * 0.01f),
                    -ArrivalTarget.forward);


            WakeBufferWall();

            //Makes objects collide with invisible buffer bounds
            foreach (var c in BufferWall) teleportScript.CollisionManager.AddCollision(this, c);

            //Makes objects collide with invisible buffer bounds
            foreach (var c in ((Portal) Target).BufferWall) {
#if !DISABLE_PHYSICS_IGNORE
                teleportScript.CollisionManager.IgnoreCollision(this, c, true);
#endif
            }

            if (SKSGlobalRenderSettings.PhysicsPassthrough)
                using (var c = PassthroughColliders.GetEnumerator()) {
                    while (c.MoveNext())
                        teleportScript.CollisionManager.AddCollision(this, c.Current.Value);
                }

            //Passes Portal info to teleport script
            teleportScript.SetPortalInfo(this);

            //Enables Doppleganger
            teleportScript.EnableDoppleganger();

            //Checks if object should be teleported
            TryTeleportTeleporable(teleportScript, col);
        }

        /// <summary>
        ///     Removes primed objects from the queue if they move away from the Portal
        /// </summary>
        public void E_OnTriggerExit(Collider col)
        {
            if (!Enterable)
                return;

            if (col == HeadCollider)
                _headInPortalTrigger = false;

            RemoveTeleportable(GetTeleportableFromCollider(col));
        }
    }
}