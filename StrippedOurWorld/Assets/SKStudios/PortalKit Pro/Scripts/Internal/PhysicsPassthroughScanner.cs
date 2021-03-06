﻿// © 2018, SKStudios LLC. All Rights Reserved.
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

using SKStudios.Common.Utils;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace SKStudios.Portals {
    /// <summary>
    ///     Used to prevent non teleportable objects from interacting with physics passthrough duplicates
    ///     spawnwed by the <see cref="PhysicsPassthrough" /> component.
    /// </summary>
    public class PhysicsPassthroughScanner : MonoBehaviour {
        private const float Bufferfactor = 0.4f;
        private Vector3 _currentSize;

        private PhysicsPassthrough _passthrough;
        public BoxCollider Collider;

        private void OnEnable()
        {
            IncrementalUpdater.RegisterObject<PhysicsPassthroughScanner>(this, UpdateBounds, UpdateStep.FixedUpdate);
        }

        private void OnDisable()
        {
            IncrementalUpdater.RemoveObject<PhysicsPassthroughScanner>(this, UpdateStep.FixedUpdate);
        }

        /// <summary>
        ///     Initialize this passthrough scanner
        /// </summary>
        /// <param name="passthrough">the "parent" scanner</param>
        public void Initialize(PhysicsPassthrough passthrough)
        {
            _passthrough = passthrough;
            Collider = gameObject.GetComponent<BoxCollider>();
            Collider.enabled = true;
            Collider.attachedRigidbody.WakeUp();
            UpdateBounds();
        }

        /// <summary>
        ///     Mantains constant size on the boundaries of the scanner. Grows the bounds to include new duplicates.
        /// </summary>
        private void UpdateBounds()
        {
            if (!_passthrough || !Collider || !Collider.enabled)
                return;

            transform.position = _passthrough.Portal.Origin.position;
            var b = new Bounds {center = transform.position};

            if (b.size.magnitude > 0)
                b.size -= b.size / 10f;

            foreach (var c in _passthrough.Portal.BufferWall)
                if (c)
                    b.Encapsulate(c.bounds);


            foreach (var c in _passthrough.Portal.PassthroughColliders.Values)
                if (c)
                    b.Encapsulate(c.bounds);

            Collider.center = transform.InverseTransformPoint(b.center);
            var size = transform.InverseTransformVector(b.size);
            size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
            _currentSize = size;
            Collider.size = _currentSize + _currentSize * Bufferfactor;
            transform.rotation = Quaternion.identity;
        }

        /// <summary>
        ///     Adds a new collider to be ignored when it enters the detection bounds
        /// </summary>
        /// <param name="col">collider to be ignored</param>
        public void OnTriggerEnter(Collider col)
        {
            if (!col || col.gameObject.isStatic || col.CompareTag(Keywords.Tags.PhysicDupe))
                return;

            Teleportable teleportable;

            if (col.attachedRigidbody != null && (teleportable = col.attachedRigidbody.GetComponent<Teleportable>()))
                if (_passthrough.Portal.NearTeleportables.Contains(teleportable))
                    return;

            foreach (var c in _passthrough.Portal.PassthroughColliders.Values) Physics.IgnoreCollision(c, col, true);

            if (_passthrough.Portal.Target)
                foreach (var c in ((Portal) _passthrough.Portal.Target).BufferWall)
                    if (c)
                        Physics.IgnoreCollision(c, col, true);

            _passthrough.IgnoredColliders.Add(col);
        }

        /// <summary>
        ///     Detect colliders leaving the volume
        /// </summary>
        /// <param name="col"></param>
        public void OnTriggerExit(Collider col)
        {
            _passthrough.IgnoredColliders.Remove(col);
        }

        /// <summary>
        ///     Draw the gizmos for the passthrough scanner
        /// </summary>
        public void OnDrawGizmosSelected()
        {
            if (!Collider) return;

            var style = new GUIStyle(new GUIStyle {alignment = TextAnchor.MiddleCenter});
            style.normal.textColor = Color.white;
#if UNITY_EDITOR
            Handles.Label(Collider.bounds.center, "Physics Passthrough Scanner volume", style);
#endif

            Gizmos.color = new Color(0, 0, 1, 0.2f);
            Gizmos.DrawCube(Collider.bounds.center, Collider.bounds.extents * 2f);
        }
    }
}