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

using System.Collections;
using SKStudios.Common.Utils;
using UnityEngine;

namespace SKStudios.Portals.Demos {
    /// <summary>
    ///     Quick and dirty tbh but also it's 2AM and I don't really care
    /// </summary>
    public class DungeonKey : MonoBehaviour {
        private GameObject _attachmentRef;
        public Collider[] BufferColliders;
        private bool done;

        public Rigidbody[] DoorBodies;

        public GameObject[] EndActivated;
        public GameObject[] EndDeActivated;
        public float FollowDistance = 1;
        public float FollowSpeed = 1;
        private Vector3 initialPosition;
        public GameObject insertRef;
        public GameObject KeyObject;
        public Transform KeySpawnPoint;
        private Vector3 lastPos;

        private Vector3 lastPositionWorld;

        private bool ovr;

        private Transform parent;
        public GameObject plungeRef;
        public GameObject pullbackRef;

        public Spinning spinning;

        private void Start()
        {
            foreach (var go in EndActivated)
                go.SetActive(false);
            parent = transform.parent;
            initialPosition = KeySpawnPoint.transform.position;
            StartCoroutine(Init());
        }

        private IEnumerator Init()
        {
            yield return WaitCache.OneTenthS;
            yield return WaitCache.OneTenthS;
            //transform.SetParent(null);
            //KeyObject.transform.SetParent(null);
            KeyObject.transform.position = initialPosition;
            yield return WaitCache.OneTenthS;
            //KeyObject.transform.localScale = Vector3.one * 0.17f;
            GetComponent<SphereCollider>().enabled = true;
            KeyObject.GetComponentInChildren<Collider>().enabled = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.name.Equals("Simple FPS controller") && _attachmentRef == null) {
                _attachmentRef = other.gameObject;
                Destroy(GetComponent<SphereCollider>());
                transform.SetParent(parent);
                other.GetComponent<Teleportable>().OnTeleport += TeleportKey;
                lastPos = KeyObject.transform.position;
            }
            else if (other.name.Equals("Lock") && _attachmentRef != null && !ovr) {
                ovr = true;
                KeyObject.transform.SetParent(null, true);
                transform.SetParent(other.transform);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                //KeyObject.transform.SetParent(transform, true);
                _attachmentRef = other.gameObject;
                FollowDistance = 0.001f;
                FollowSpeed = 0.5f;
                StartCoroutine(KeyInsert());
            }
        }

        private IEnumerator KeyInsert()
        {
            foreach (var c in BufferColliders)
                c.enabled = true;

            yield return new WaitForSeconds(1.5f);
            while (spinning.speed > 0) {
                spinning.speed -= 0.1f;
                yield return WaitCache.Frame;
            }

            spinning.speed = 0;
            var lineupTime = 1f;
            var currentTime = lineupTime;
            var InitialRotation = KeyObject.transform.rotation;
            _attachmentRef = insertRef;
            FollowSpeed = 0.3f;
            while (currentTime > 0) {
                var scalar = 1 - currentTime / lineupTime;
                KeyObject.transform.rotation = Quaternion.Slerp(InitialRotation, insertRef.transform.rotation, scalar);
                currentTime -= Time.deltaTime;
                yield return WaitCache.Frame;
            }

            FollowSpeed = 7f;

            _attachmentRef = pullbackRef.gameObject;
            yield return new WaitForSeconds(0.4f);

            FollowSpeed = 9f;
            _attachmentRef = plungeRef.gameObject;
            yield return new WaitForSeconds(0.2f);
            Unlock();
        }

        private void TeleportKey(Portal source, Portal destination)
        {
            /*KeyObject.transform.SetParent(destination.Origin, true);
            KeyObject.transform.SetParent(source.Origin, false);
            KeyObject.transform.SetParent(parent, true);*/
            lastPos = KeyObject.transform.position;
        }

        private void LateUpdate()
        {
#if SKS_DEV
            if (Input.GetKeyDown(KeyCode.Return))
                Unlock();
#endif
            if (done) return;
            if (_attachmentRef == null) {
                transform.position = initialPosition;
                transform.localScale =
                    new Vector3(1 / parent.lossyScale.x, 1 / parent.lossyScale.y, 1 / parent.lossyScale.z) * 3f;
                return;
            }

            transform.localScale = Vector3.one * 30f;

            //transform.position = Vector3.zero;
            var heading = (_attachmentRef.transform.position - KeyObject.transform.position).normalized;
            heading = Vector3.Scale(heading, _attachmentRef.transform.localScale);
            var destinationPoint = _attachmentRef.transform.position - heading * FollowDistance;
            KeyObject.transform.position = Vector3.Lerp(lastPos, destinationPoint, Time.deltaTime * FollowSpeed);
            lastPos = KeyObject.transform.position;
        }

        private void Unlock()
        {
            KeyObject.AddComponent<Rigidbody>();
            foreach (var r in DoorBodies)
                r.isKinematic = false;
            done = true;
            foreach (var go in EndActivated)
                go.SetActive(true);
            foreach (var go in EndDeActivated)
                go.SetActive(false);
            transform.SetParent(null);
        }
    }
}