// © 2018, SKStudios LLC. All Rights Reserved.
// 
// The software, artwork and data, both as individual files and as a complete software package known as 'PortalKit Pro', or 'MirrorKit Pro'
// without regard to source or channel of acquisition, are bound by the terms and conditions set forth in the Unity Asset 
// Store license agreement in addition to the following terms;
// 
// One license per seat is required for Companies, teams, studios or collaborations using PortalKit Pro and/or MirrorKit Pro that have over 
// 10 members or that make more than $10,000 USD per year. 
// 
// Addendum;
// If PortalKit Pro or MirrorKit pro constitute a major portion of your game's mechanics, please consider crediting the software and/or SKStudios.
// You are in no way obligated to do so, but it would be sincerely appreciated.

using UnityEngine;

namespace SKStudios.Common.Demos {
    /// <summary>
    ///     Basic FPS controller example
    /// </summary>
    public class BasicFPSExample : MonoBehaviour {
        //private Vector2 _mouseAbsolute;
        private Rigidbody _rigidbody;
        private Vector2 _smoothMouse;
        public Transform body;

        // Assign this if there's a parent object controlling motion, such as a Character Controller.
        // Yaw rotation will affect this object instead of the camera if set.
        public GameObject characterBody;

        public Vector2 clampInDegrees = new Vector2(360, 180);
        private float distanceMoved;

        public GameObject eyeParent;
        public bool ForcePause;
        public Camera headCam;
        public GameObject headset;

        public bool KillInput;
        private Quaternion lastBodyRot;
        public bool lockCursor;
        public float moveSpeed = 100f;
        private Vector3 moveStartPosition;

        private bool moving;
        private bool paused;
        public Collider playerPhysBounds;
        public Vector2 sensitivity = new Vector2(2, 2);
        public Vector2 smoothing = new Vector2(3, 3);
        public Vector2 targetCharacterDirection;
        public Vector2 targetDirection;
        private float totalTime;

        private void Start()
        {
            GetComponentInChildren<Camera>().depthTextureMode = DepthTextureMode.None;
            // Set target direction to the camera's initial orientation.
            targetDirection = transform.localRotation.eulerAngles;
            // Set target direction for the character body to its inital state.
            if (characterBody) targetCharacterDirection = characterBody.transform.localRotation.eulerAngles;
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            playerPhysBounds.enabled = true;
            playerPhysBounds.transform.localPosition +=
                new Vector3(0, playerPhysBounds.GetComponent<Collider>().bounds.extents.y, 0);
        }


        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                paused = !paused;

            if (ForcePause)
                paused = true;

            Cursor.lockState = !paused ? CursorLockMode.Locked : CursorLockMode.None;
            if (paused) return;

            // Get raw mouse input for a cleaner reading on more sensitive mice.
            var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            var euler = headset.transform.parent.localEulerAngles;
            var phi = Mathf.Abs((int) euler.x - 0) % 360; // This is either the distance or 360 - distance
            var distance = phi > 180 ? 360 - phi : phi;

            // Scale input against the sensitivity setting and multiply that against the smoothing value.
            mouseDelta = Vector2.Scale(mouseDelta,
                new Vector2(sensitivity.y * smoothing.x, sensitivity.x * smoothing.y));

            // Interpolate mouse movement over time to apply smoothing delta.
            _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
            _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);

            var rotation = Quaternion.AngleAxis(-_smoothMouse.y, Vector3.right);
            var halfclamp = clampInDegrees.y / 2;
            rotation *= Quaternion.AngleAxis(_smoothMouse.x * ((halfclamp - distance + 25) / (halfclamp + 25)),
                Vector3.up);

            headset.transform.parent.localRotation *= rotation;

            var newEuler = headset.transform.parent.localEulerAngles;
            phi = Mathf.Abs((int) newEuler.x - 0) % 360; // This is either the distance or 360 - distance
            var newDistance = phi > 180 ? 360 - phi : phi;

            if (distance > halfclamp && newDistance >= distance)
                newEuler.x = euler.x;

            headset.transform.parent.localEulerAngles = new Vector3(newEuler.x, newEuler.y, 0);
            var targetBodyRot = Quaternion.Euler(0, newEuler.y, 0);
            body.transform.localRotation =
                Quaternion.Lerp(body.transform.localRotation,
                    targetBodyRot,
                    Time.deltaTime * 3);
        }

        private void FixedUpdate()
        {
            var dirVector = headCam.transform.forward;
            dirVector.y = 0;
            dirVector = dirVector.normalized * moveSpeed * transform.localScale.x * 35f;
            //Keyboard input
            if (Input.GetKey(KeyCode.LeftShift)) dirVector *= 2f;

            if (!KillInput) {
                var inputDir = Vector3.zero;
                if (Input.GetKey(KeyCode.W))
                    inputDir += dirVector;

                if (Input.GetKey(KeyCode.A))
                    inputDir += Quaternion.AngleAxis(-90, Vector3.up) * dirVector;

                if (Input.GetKey(KeyCode.S))
                    inputDir += Quaternion.AngleAxis(180, Vector3.up) * dirVector;

                if (Input.GetKey(KeyCode.D))
                    inputDir += Quaternion.AngleAxis(90, Vector3.up) * dirVector;

                var newVelocity = inputDir * Time.deltaTime;

                //_rigidbody.velocity -= _lastVelocity;
                //_lastVelocity = newVelocity;
                _rigidbody.velocity = new Vector3(newVelocity.x, _rigidbody.velocity.y, newVelocity.z);
            }
        }
    }
}