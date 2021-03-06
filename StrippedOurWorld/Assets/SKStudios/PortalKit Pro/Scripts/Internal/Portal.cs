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

using System;
using System.Collections.Generic;
using System.Linq;
using SKStudios.Common;
using SKStudios.Common.Utils;
using SKStudios.Rendering;
using SK_Common.Extensions;
using UnityEngine;
using UnityEngine.Rendering;

namespace SKStudios.Portals {
    /// <summary>
    ///     Class handling the majority of Portal logic. All public fields are automatically handled by a PortalController if
    ///     you wish to control portals via the Editor.
    /// </summary>
    public partial class Portal : EffectRenderer {
        //Keeps the Portal effect 100% seamless even at higher head speeds
        private const float FudgeFactor = 0.001f;

        private readonly Vector3 _defaultScale = new Vector3(1, 1, FudgeFactor);

        private Transform _detectionZone;

        private bool _enterable;

        private Collider _floorSegment;

        private bool _headInPortalTrigger;

        private Collider _portalCollider;

        private bool _updatedDopplegangersThisFrame;

        /// <summary>
        ///     Colliders that constitute wall around this <see cref="Portal"/>
        /// </summary>
        public List<Collider> BufferWall;

        /// <summary>
        ///     Parent of the <see cref="BufferWall"/> objects
        /// </summary>
        public GameObject BufferWallObj;

        /// <summary>
        ///     Should the portal make teleportables within its bounds ignore colliders behind the portal?
        /// </summary>
        public bool IgnoreRearCollisions = true;

        /// <summary>
        ///     Nearby <see cref="Teleportable" />s.
        /// </summary>
        public HashSet<Teleportable> NearTeleportables = new HashSet<Teleportable>();

        /// <summary>
        ///     <see cref="Collider" />s near enough to trigger passthrough (controlled by their respective scripts)
        /// </summary>
        public Dictionary<Collider, Collider> PassthroughColliders;

        /// <summary>
        ///     Ignored <see cref="Collider" />s behind this <see cref="Portal" />
        /// </summary>
        public Collider[] RearColliders;

        /// <summary>
        ///     Is this <see cref="Portal" /> rendering with depth enabled, or will it always pass the Z-test?
        /// </summary>
        public bool TestDepthEnabled = true;

        /// <summary>
        ///     <inheritdoc cref="EffectRenderer.DefaultMaterialLocation" />
        /// </summary>
        public override string DefaultMaterialLocation {
            get { return "Materials/Visible effects/Portal Materials/PortalMat"; }
        }

        /// <summary>
        ///     <inheritdoc cref="EffectRenderer.IsMirror"/>
        /// </summary>
        public override bool IsMirror { get { return false; } }

        /// <summary>
        ///     Is this <see cref="Portal" /> enterable?
        /// </summary>
        public bool Enterable {
            get { return _enterable; }
            set {
                _enterable = value;
                if (!value) {
                    while (NearTeleportables.Count > 0) RemoveTeleportable(NearTeleportables.First());
                    NearTeleportables.Clear();
                    transform.parent.Find("PortalTrigger").GetComponent<Collider>().enabled = false;
                }
                else {
                    transform.parent.Find("PortalTrigger").GetComponent<Collider>().enabled = true;
                }
            }
        }

        private Collider PortalCollider {
            get {
                if (!_portalCollider)
                    _portalCollider = GetComponent<Collider>();
                return _portalCollider;
            }
        }

        /// <summary>
        ///     Origin of the Portal, facing inward
        /// </summary>
        public Collider FloorSegment {
            get {
                if (!_floorSegment)
                    _floorSegment = Root.Find("PortalRenderer/FloorSegment").GetComponent<Collider>();
                return _floorSegment;
            }
        }

        /// <summary>
        ///     Origin of the Portal, facing inward
        /// </summary>
        public Transform DetectionZone {
            get {
                if (!_detectionZone)
                    _detectionZone = Root.Find("PortalRenderer/PortalTrigger");
                return _detectionZone;
            }
        }

        /// <summary>
        ///     Should Physics Passthrough be used?
        /// </summary>
        public PhysicsPassthrough PhysicsPassthrough { get; set; }

        /// <summary>
        ///     Event raised when this portal teleports a <see cref="Teleportable" /> away from itself.
        ///     Second arg is a ref to the target <see cref="Portal" />.
        /// </summary>
        public event Action<Teleportable, Portal> OnTeleportObject;

        /// <summary>
        ///     Event raised when this portal recieves a <see cref="Teleportable" /> sent to it from a <see cref="Portal" /> that
        ///     targets this one.
        ///     Second arg is the <see cref="Portal" /> that sent the object.
        /// </summary>
        public event Action<Teleportable, Portal> OnObjectArrived;


        protected override void RenderObjectBehavior()
        {
            UpdateDopplegangers();
            ((Portal) Target).UpdateDopplegangers(true);
        }

#region UnityMethods
        private void Awake()
        {
            enabled = false;
            var portalLayer = Keywords.Layers.CustomRenderer;
            Physics.IgnoreLayerCollision(portalLayer, portalLayer, true);
        }

        /// <summary>
        ///     Setup for the Portal
        /// </summary>
        private new void OnEnable()
        {
            base.OnEnable();

            NearTeleportables = new HashSet<Teleportable>();
            PassthroughColliders = new Dictionary<Collider, Collider>();
            //Add the buffer colliders to the collection
            BufferWall = new List<Collider>();
            foreach (Transform tran in BufferWallObj.transform)
            {
                var c = tran.GetComponent<Collider>();
                BufferWall.Add(c);
                c.enabled = false;
            }

            BufferWall.Add(FloorSegment);
            PhysicsPassthrough.Initialize(this);
        }


        private void Update()
        {
            _updatedDopplegangersThisFrame = false;
            MeshRenderer.sharedMaterial.SetFloat(Keywords.ShaderKeys.ZTest, (int) CompareFunction.Less);
        }

        private new void LateUpdate()
        {
            UpdateBackPosition();
            base.LateUpdate();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (PassthroughColliders != null)
                foreach (var c in PassthroughColliders.Keys)
                    if (PassthroughColliders[c])
                        Destroy(PassthroughColliders[c]);
        }

#endregion

        /// <summary>
        ///     Updates the "dopplegangers", the visual counterparts, to all teleportable objects near portals.
        /// </summary>
        public void UpdateDopplegangers(bool force = false)
        {
            if (!ArrivalTarget || !force && _updatedDopplegangersThisFrame)
                return;
            _updatedDopplegangersThisFrame = true;
            //Updates dopplegangers
            foreach (var tp in NearTeleportables) {
                if (!tp || !tp.Initialized)
                    continue;
                tp.UpdateDoppleganger();
                if (tp.Doppleganger)
                    PortalUtils.TeleportObject(tp.Doppleganger, Origin, ArrivalTarget, tp.Doppleganger.transform, null,
                        false);
                else
                    RemoveTeleportable(tp);
                //PortalUtils.TeleportObject(tp.Doppleganger, Origin, ArrivalTarget, tp.Doppleganger.transform);
                //StartCoroutine(tp.UpdateDoppleganger());
            }
        }

        /// <summary>
        ///     Updates the back position of the Portal wall for visual seamlessness
        /// </summary>
        private void UpdateBackPosition()
        {
            _CheeseActivated = -1;
            if (!Settings.Is3D) transform.localScale = _defaultScale;
            if (SeamlessRecursionFix != null) SeamlessRecursionFix.gameObject.SetActive(false);
            if (_headInPortalTrigger == false || HeadCamera == null ||
                PlayerTeleportable.PlayerTeleportableScript.MidTeleport)
                return;

            _NearClipVertsLocal = HeadCamera.EyeNearPlaneDimensions();
            //Gets the near clipping plane verts in global space
            if (_NearClipVertsGlobal == null)
                return;

            var originScale = Origin.transform.lossyScale.x;
            var destinationScale = ArrivalTarget.transform.lossyScale.x;
            var scale = originScale > destinationScale ? originScale / destinationScale : 1;
            for (var i = 0; i < _NearClipVertsGlobal.Length; i++)
                _NearClipVertsGlobal[i] = HeadCamera.transform.position +
                                         HeadCamera.transform.rotation * (_NearClipVertsLocal[i] * scale);


            //Moves the drawn "plane" back if the camera gets too close
            float deepestVert = 0;
            var deepestVertIndex = 0;
            Vector3 deepestVertVector;
            var portalPlane = new Plane(-Origin.forward, Origin.position);
            for (var index = 0; index < _NearClipVertsGlobal.Length; index++) {
                var currentVert = _NearClipVertsGlobal[index];
                var currentDepth = portalPlane.GetDistanceToPoint(currentVert);
                if (currentDepth < 0) {
                    Debug.DrawLine(HeadCamera.transform.position, currentVert, Color.red);
                    continue;
                }

                Debug.DrawLine(HeadCamera.transform.position, currentVert, Color.green);
                _CheeseActivated = SkEffectCamera.CurrentDepth;
                if (currentDepth > deepestVert) {
                    deepestVert = currentDepth;
                    deepestVertIndex = index;
                    deepestVertVector = currentVert;
                }
            }

            deepestVertVector = _NearClipVertsGlobal[deepestVertIndex];
            //Scale the portal for seamless passthrough
            if (_CheeseActivated != -1 && !Settings.Is3D) {
                SeamlessRecursionFix.gameObject.SetActive(true);
                //Reset scale so that InverseTransformPoint doesn't return a scaled value
                transform.localScale = Vector3.one;
                //Get the local-space distance
                var dotDist = -PortalUtils.PlanePointDistance(Vector3.forward, Vector3.zero,
                    transform.InverseTransformPoint(deepestVertVector));
                //Clamp to Fudge
                var dist = Mathf.Max(FudgeFactor, dotDist) * 1; //1.5f;
                //If the dist is too far, do not update the back position, there is a state machine offset
                if (dist >= 1) {
                    transform.localScale = _defaultScale;
                    return;
                }

                //Scale, so that the back wall updates properly.
                transform.localScale = new Vector3(1, 1, dist);
            }
        }


        /// <summary>
        ///     External class keeps track of the physics duplicates for clean, consistent physical passthrough
        /// </summary>
        public void FixedUpdate()
        {
            //if (GlobalPortalSettings.PhysicsPassthrough)
            //_headInPortalTrigger = false;
            if (NearTeleportables.Count >= 1 || ((Portal) Target).NearTeleportables.Count >= 1)
                PhysicsPassthrough.UpdatePhysics();
        }

        /// <summary>
        ///     Attempt to add a teleportableScript to the nearteleportables group
        /// </summary>
        /// <param name="teleportable">the script to add</param>
        public bool AddTeleportable(Teleportable teleportable)
        {
            if (NearTeleportables.Contains(teleportable))
                return true;
            if (!Target ||
                !Enterable ||
                !teleportable ||
                !teleportable.Initialized ||
                teleportable.AddedLastFrame)
                return false;


            SetBufferWallActive(true);

            teleportable.AddedLastFrame = true;
            teleportable.EnableDoppleganger();

            NearTeleportables.Add(teleportable);

            //Ignores collision with rear objects
            var checkedVerts = PortalUtils.BackCheckVerts(MeshFilter.mesh);

            //Ignore collision with the Portal itself
#if !DISABLE_PHYSICS_IGNORE
            teleportable.CollisionManager.IgnoreCollision(this, PortalCollider, true, true);
            teleportable.CollisionManager.IgnoreCollision(this, ((Portal) Target).PortalCollider, true, true);
#endif
            //Ignores rear-facing colliders
            if (IgnoreRearCollisions) {
                var ray = new Ray();
                RaycastHit[] hit;

                //Enables the buffer wall if it is disabled
                foreach (Transform tran in BufferWallObj.transform) {
                    var c = tran.GetComponent<Collider>();
                    c.enabled = true;
                }

                var ignoredColliders = new HashSet<Collider>();

                //Raycast back
                foreach (var v in checkedVerts) {
                    ray.origin = transform.TransformPoint(v) + transform.forward * 0.01f;
                    ray.direction = -transform.forward;

                    hit = Physics.RaycastAll(ray, 1 * transform.parent.localScale.z, ~0,
                        QueryTriggerInteraction.Collide);
#if SKS_DEV
                    Debug.DrawRay(ray.origin, -transform.forward * transform.parent.localScale.z, Color.cyan, 3);
#endif
                    if (hit.Length <= 0) continue;
                    foreach (var h in hit) {
                        //Never ignore collisions with Physics Passthrough Duplicates
                        var t = h.collider.gameObject.GetComponent<Teleportable>();
                        if (h.collider.gameObject.CompareTag(Keywords.Tags.PhysicDupe) ||
                            t != null)
                            continue;

                        if (h.collider.transform.parent && transform.parent && h.collider.transform.parent.parent &&
                            transform.parent.parent) {
#if !DISABLE_PHYSICS_IGNORE
                            if (h.collider.transform.parent.parent != transform.parent.parent) {
                                teleportable.CollisionManager.IgnoreCollision(this, h.collider);
                                ignoredColliders.Add(h.collider);
                            }
#endif
                        }
                        else {
#if !DISABLE_PHYSICS_IGNORE
                            teleportable.CollisionManager.IgnoreCollision(this, h.collider);
                            ignoredColliders.Add(h.collider);
#endif
                        }
                    }
                }

                var downCheckVerts = this.DownCheckVerts();
                foreach (var v in downCheckVerts) {
                    ray.origin = Origin.TransformPoint(v) - transform.forward * 0.01f;
                    ray.direction = -transform.up;
                    hit = Physics.RaycastAll(ray, transform.parent.localScale.y + 0.3f, ~0,
                        QueryTriggerInteraction.Collide);
#if SKS_DEV
                    Debug.DrawRay(ray.origin, -transform.up * transform.parent.localScale.y, Color.red, 3);
#endif
                    if (hit.Length <= 0) continue;
                    foreach (var h in hit)
                        if (ignoredColliders.Contains(h.collider)) {
                            FloorSegment.gameObject.SetActive(true);
                            Debug.DrawRay(ray.origin, -transform.up * transform.parent.localScale.y, Color.green, 3);
                            goto TERRAIN_INTERSECTION;
                        }

                    FloorSegment.gameObject.SetActive(false);
                }
            }

            TERRAIN_INTERSECTION:

            teleportable.EnterPortal(this);

            UpdateDopplegangers();

            return true;
        }

        /// <summary>
        ///     Teleports the given teleportable, making passthrough and image effects seamless.
        /// </summary>
        /// <param name="teleportable">Teleportable to teleport</param>
        /// <param name="col">Associated Collider</param>
        private void TryTeleportTeleporable(Teleportable teleportable, Collider col)
        {
            var targetPortal = (Portal) Target;

            if (!SKSGeneralUtils.IsBehind(teleportable.TeleportableBounds.center, Origin.position, Origin.forward) ||
                teleportable.VisOnly)
                return;
            //if (!PortalUtils.IsBehind(col.transform.position, Origin.position, Origin.forward) || teleportable.VisOnly) return;
            if (teleportable.TeleportedLastFrame)
                return;

            teleportable.TeleportedLastFrame = true;
            RemoveTeleportable(teleportable);

            //Makes objects not with invisible buffer bounds in the case of portals being too close
            foreach (var c in BufferWall) {
#if !DISABLE_PHYSICS_IGNORE
                teleportable.CollisionManager.IgnoreCollision(this, c, true);
#endif
            }


            teleportable.StartTeleport(this);

            PortalUtils.TeleportObject(teleportable.Root.gameObject, Origin, ArrivalTarget, teleportable.Root, null,
                true,
                !SKSGlobalRenderSettings.NonScaledRenderers);

            targetPortal.FixedUpdate();
            teleportable.Teleport(this, targetPortal);

            if (targetPortal.PhysicsPassthrough) {
                targetPortal.PhysicsPassthrough.UpdatePhysics();
                var teleportableColliders = teleportable.CollisionManager.Colliders.ToArray();
                targetPortal.PhysicsPassthrough.ForceRescanOnColliders(teleportableColliders);
                if (PhysicsPassthrough)
                    PhysicsPassthrough.ForceRescanOnColliders(teleportableColliders);
            }

            targetPortal.E_OnTriggerStay(col);

            teleportable.FinishTeleport(this);


            targetPortal.UpdateDopplegangers(true);
            UpdateDopplegangers(true);

            //Un-register head as being in portal trigger to prevent flash
            if (teleportable == PlayerTeleportable.PlayerTeleportableScript) {
                _headInPortalTrigger = false;
                targetPortal.UpdateDopplegangers(true);
                targetPortal.IncomingCamera();
                _CheeseActivated = -1;
                //Resets the vis depth of the Portal volume
                if (!Settings.Is3D)
                    transform.localScale = new Vector3(1f, 1f, FudgeFactor);
            }

            Rigidbody body;
            if ((body = col.attachedRigidbody) && SKSGlobalRenderSettings.PhysStyleB) {
                var colliderEnabled = PortalCollider.enabled;
                var otherColliderEnabled = targetPortal.PortalCollider.enabled;

                PortalCollider.enabled = true;
                targetPortal.PortalCollider.enabled = true;

                var portalBody = PortalCollider.attachedRigidbody;
                var targetBody = targetPortal.PortalCollider.attachedRigidbody;

                PortalCollider.enabled = colliderEnabled;
                targetPortal.PortalCollider.enabled = otherColliderEnabled;

                if (portalBody != null
                    && targetBody != null) {
                    var relativeVelocity = Quaternion.Inverse(portalBody.rotation) * portalBody.velocity;
                    relativeVelocity += Quaternion.Inverse(targetBody.rotation) * targetBody.velocity;
                    relativeVelocity = targetBody.rotation * relativeVelocity;
                    body.AddForce(relativeVelocity, ForceMode.Impulse);
                }
            }

            if (OnTeleportObject != null)
                OnTeleportObject.Invoke(teleportable, (Portal) Target);

            if (targetPortal.OnObjectArrived != null)
                targetPortal.OnObjectArrived.Invoke(teleportable, this);
        }

        /// <summary>
        ///     Removes a teleportable from the portal area
        /// </summary>
        /// <param name="teleportable">teleportable to remove</param>
        private void RemoveTeleportable(Teleportable teleportable)
        {
            if (!teleportable || teleportable.RemovedLastFrame) return;
            if (!NearTeleportables.Contains(teleportable)) return;
            teleportable.RemovedLastFrame = true;

            if (!NonObliqueOverride)
                teleportable.SetClipPlane(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero);


            NearTeleportables.Remove(teleportable);
            UpdateDopplegangers();

            if (NearTeleportables.Count < 1)
                SetBufferWallActive(false);

            teleportable.LeavePortal(this);

            teleportable.CollisionManager.ResumeCollisionsForKey(this);

            if (teleportable == PlayerTeleportable.PlayerTeleportableScript)
                _headInPortalTrigger = false;
        }


        /// <summary>
        ///     Called when another Portal is sending a camera to this Portal
        /// </summary>
        public void IncomingCamera()
        {
            //Debug.Break();
            _CheeseActivated = -1;
            _headInPortalTrigger = true;
            UpdateBackPosition();
            UpdateDopplegangers();
            UpdateEffects();
            //TargetPortal.PortalCamera.ForceResetRender();
            ((Portal) Target).EffectCamera.ForceResetRender();
            WakeBufferWall();
            //TargetPortal.TryRenderPortal(_headCamera, NearClipVertsGlobal);
            //TryRenderPortal(_headCamera, NearClipVertsGlobal);
        }


        /// <summary>
        ///     Wake the buffer wall. Walls spawn disabled to prevent fast-Moving objects from colliding with them before any
        ///     objects have ever entered the teleportable.
        /// </summary>
        private void WakeBufferWall()
        {
            if (BufferWall.Count > 0)
                if (BufferWall[0].attachedRigidbody)
                    BufferWall[0].attachedRigidbody.WakeUp();
        }

        private void SetBufferWallActive(bool active)
        {
            if (BufferWall.Count > 0)
                foreach (var c in BufferWall)
                    c.enabled = active;
        }
    }
}