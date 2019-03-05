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
    public class ObjectSpawner : MonoBehaviour {
        private Rigidbody body;
        private GameObject cube;
        public GameObject spawnedObject;

        // Use this for initialization
        private void Start()
        {
            cube = Instantiate(spawnedObject);
            body = cube.GetComponent<Rigidbody>();
            StartCoroutine(DropCube());
        }

        private IEnumerator DropCube()
        {
            while (true) {
                cube.transform.position = transform.position;
                cube.transform.rotation = Quaternion.identity;
                body.velocity = Vector3.zero;
                yield return new WaitForSeconds(2);
            }
        }
    }
}