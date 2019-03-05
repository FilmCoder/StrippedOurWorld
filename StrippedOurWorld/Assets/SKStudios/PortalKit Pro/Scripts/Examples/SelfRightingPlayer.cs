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

using UnityEngine;

namespace SKStudios.Portals.Demos {
    public class SelfRightingPlayer : MonoBehaviour {
        private Rigidbody _body;
        public float MoveSpeed = 2f;

        // Use this for initialization
        private void Start()
        {
            _body = gameObject.GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        private void Update()
        {
            //Get the forward direction of the capsule solely on the XZ plane for rotational calculation
            var forward = transform.forward;
            forward = new Vector3(forward.x, 0, forward.z).normalized;

            //rotate the rotation closer to true up based on MoveSpeed
            var newRot = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(
                    forward,
                    Vector3.up), Time.deltaTime * MoveSpeed);

            //Apply rotation to player
            if (!_body)
                transform.rotation = newRot;
            else
                _body.MoveRotation(newRot);
        }
    }
}