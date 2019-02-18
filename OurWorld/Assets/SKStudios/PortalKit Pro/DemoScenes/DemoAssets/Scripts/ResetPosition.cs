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
using UnityEngine;

namespace SKStudios.Portals.Demos {
    public class ResetPosition : MonoBehaviour {
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private Vector3 initialScale;
        private IEnumerator ResetPos;
        public float resetTime;

        private Rigidbody rigid;

        // Use this for initialization
        private void Start()
        {
            rigid = gameObject.GetComponent<Rigidbody>();
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            initialScale = transform.localScale;
            ResetPos = ResetPositionandVelocity(resetTime);
            StartCoroutine(ResetPos);
        }

        public IEnumerator ResetPositionandVelocity(float time)
        {
            while (true) {
                yield return new WaitForSecondsRealtime(time);
                rigid.isKinematic = true;
                transform.position = initialPosition;
                transform.rotation = initialRotation;
                transform.localScale = initialScale;
                rigid.isKinematic = false;
                rigid.velocity = Vector3.zero;
            }
        }
    }
}