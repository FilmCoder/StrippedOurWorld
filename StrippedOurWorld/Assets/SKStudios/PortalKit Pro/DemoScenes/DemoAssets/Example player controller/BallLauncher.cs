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

namespace SKStudios.Common.Demos {
    public class BallLauncher : MonoBehaviour {
        public GameObject ball;
        public float speed = 200;

        public Transform tip;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0)) {
                var newBall = Instantiate(ball);
                newBall.transform.SetParent(tip, false);
                newBall.transform.SetParent(null, true);
                var rigid = newBall.GetComponent<Rigidbody>();
                rigid.isKinematic = false;
                rigid.velocity = Vector3.zero;
                rigid.AddForce(tip.forward * speed);
                StartCoroutine(DestroyBall(newBall));
            }
        }

        private IEnumerator DestroyBall(GameObject go)
        {
            yield return new WaitForSeconds(5f);
            var shrinkTotalTime = 1f;
            var time = shrinkTotalTime;
            var initialScale = go.transform.localScale;
            while (time > 0) {
                go.transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, 1 - time / shrinkTotalTime);
                time -= Time.deltaTime;
                yield return WaitCache.Frame;
            }

            Destroy(go);
        }
    }
}