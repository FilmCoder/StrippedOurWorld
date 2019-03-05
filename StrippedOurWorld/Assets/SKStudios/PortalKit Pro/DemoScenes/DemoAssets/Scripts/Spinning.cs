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
    //[ExecuteInEditMode]
    public class Spinning : MonoBehaviour {
        public Vector3 axis = Vector3.up;
        private float destSpeed;
        public float speed = 4f;
        public bool spinning;
        public float spinupTime;
        public bool triggerRequired;

        private void Start()
        {
            if (triggerRequired) {
                destSpeed = speed;
                speed = 0;
            }
        }
        // Update is called once per frame

        private void Update()
        {
            if (spinning)
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles +
                                                      Quaternion.AngleAxis(speed * Time.deltaTime, axis).eulerAngles);
        }

        private IEnumerator smoothToSpeed(float spinupTime, float speed)
        {
            while (this.speed <= destSpeed) {
                this.speed += Mathfx.Sinerp(0, destSpeed, Time.deltaTime / spinupTime);
                yield return WaitCache.Fixed;
            }

            yield return null;
            this.speed = destSpeed;
        }
    }
}