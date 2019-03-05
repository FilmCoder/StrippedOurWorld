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
using System.Collections;
using SKStudios.Common.Debug;
using SKStudios.Common.Utils;
using UnityEngine;

namespace SKStudios.Portals {
    /// <summary>
    ///     Class allowing for scripts on gameobjects to be teleported separately from the object proper.
    /// </summary>
    public abstract class TeleportableScript : MonoBehaviour {
        [HideInInspector] public Portal CurrentPortal;

        private Transform _originalParent;
        private Vector3 _originalPosition;
        private Quaternion _originalRotation;
        private Vector3 _originalScale;
        private Transform _otherTransform;
        private Transform _otherTransformParent;

        [HideInInspector] public Teleportable Teleportable;

        public bool TeleportScriptIndependantly = true;

        [HideInInspector] public bool ThroughPortal;

        public virtual void Initialize(Teleportable teleportable)
        {
            this.Teleportable = teleportable;
            _originalParent = transform.parent;
            try {
                _otherTransform = SKSGeneralUtils.FindAnalogousTransform(transform, teleportable.Root,
                    teleportable.Doppleganger.transform, true);
                _otherTransformParent = _otherTransform.parent;
            }
            catch (NullReferenceException e) {
                SKLogger.LogError("Teleportablescript on " + name + "had a problem:" + e.Message, SKOrgType.PortalKit);
            }

            _originalPosition = transform.localPosition;
            _originalRotation = transform.localRotation;
            _originalScale = transform.localScale;
        }

        // Update is called once per frame
        public virtual void CustomUpdate()
        {
            if (ThroughPortal) Teleport();

            //Check if the gameobject this script is attached to is through a Portal
            if (!ThroughPortal && CurrentPortal &&
                SKSGeneralUtils.IsBehind(transform.position, CurrentPortal.Origin.position,
                    CurrentPortal.Origin.forward * 1.01f))
                StartCoroutine(DelayedTeleport());
            else if (ThroughPortal && CurrentPortal &&
                     SKSGeneralUtils.IsBehind(transform.position, CurrentPortal.Target.Origin.position,
                         CurrentPortal.Target.Origin.forward * 1.01f))
                StartCoroutine(DelayedUnTeleport());
        }

        private IEnumerator DelayedTeleport()
        {
            //Is this script going to teleport before its primary object?
            if (TeleportScriptIndependantly) {
                _otherTransform.SetParent(transform.parent);
                ThroughPortal = true;
                ActivateInheritance(gameObject);
                Teleport();
            }

            //yield return WaitCache.Frame;
            OnPassthrough();
            yield return null;
        }

        private IEnumerator DelayedUnTeleport()
        {
            if (TeleportScriptIndependantly) {
                _otherTransform.SetParent(_otherTransformParent);
                transform.SetParent(_originalParent);
                ThroughPortal = false;
                ResetTransform();
            }

            OnPassthrough();

            yield return WaitCache.Frame;
        }

        public virtual void Teleport()
        {
            transform.SetParent(_otherTransformParent);
            ResetTransform();
        }

        //Hook for detecting script Portal passthrough
        protected virtual void OnPassthrough() { }

        //Hook for detecting parent object teleport
        public virtual void OnTeleport() { }

        public virtual void LeavePortal()
        {
            if (TeleportScriptIndependantly) {
                transform.SetParent(_originalParent);
                ThroughPortal = false;

                ResetTransform();
                CurrentPortal = null;
            }

            OnPassthrough();
        }

        private void ActivateInheritance(GameObject child)
        {
            child.SetActive(true);
            if (child.transform.parent != null)
                ActivateInheritance(child.transform.parent.gameObject);
        }

        private void ResetTransform()
        {
            transform.localPosition = _originalPosition;
            transform.localRotation = _originalRotation;
            transform.localScale = _originalScale;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (ThroughPortal)
                return;
            PortalTrigger t;
            if (t = other.GetComponent<PortalTrigger>())
                if (Teleportable)
                    t.OnTriggerEnter(Teleportable.GetComponent<Collider>());
        }


        public void OnTriggerStay(Collider other)
        {
            if (ThroughPortal)
                return;
            PortalTrigger t;
            if (t = other.GetComponent<PortalTrigger>()) {
                if (!Teleportable)
                    return;
                var c = Teleportable.GetComponent<Collider>();
                t.OnTriggerStay(c);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (ThroughPortal)
                return;
            PortalTrigger t;
            if (t = other.GetComponent<PortalTrigger>())
                if (t && Teleportable)
                    t.OnTriggerExit(Teleportable.GetComponent<Collider>());
        }

        /*
        public void OnTriggerEnter(Collider other)
        {
            PortalTrigger trigger;
            if (trigger = other.GetComponent<PortalTrigger>())
            {
                trigger.SendMessage("OnTriggerEnter", teleportable.GetComponent<Collider>());
            }
            
        }*/
    }
}