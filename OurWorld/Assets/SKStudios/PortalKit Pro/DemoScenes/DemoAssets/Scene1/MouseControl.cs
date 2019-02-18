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
using UnityEngine.EventSystems;

[AddComponentMenu("Camera-Control/Mouse drag Orbit with zoom")]
public class MouseControl : MonoBehaviour {
    public float distance = 5.0f;
    public float distanceMax = 15f;
    public float distanceMin = .5f;
    private float rotationXAxis;
    private float rotationYAxis;
    public float smoothTime = 2f;
    public Transform target;
    private float velocityX;
    private float velocityY;
    public float xSpeed = 120.0f;
    public float yMaxLimit = 80f;
    public float yMinLimit = -20f;

    public float ySpeed = 120.0f;

    // Use this for initialization
    private void Start()
    {
        var angles = transform.eulerAngles;
        rotationYAxis = angles.y;
        rotationXAxis = angles.x;
    }

    private void LateUpdate()
    {
        if (target && !EventSystem.current.IsPointerOverGameObject()) {
            if (Input.GetMouseButton(0)) {
                velocityX += xSpeed * Input.GetAxis("Mouse X") * distance * 0.02f;
                velocityY += ySpeed * Input.GetAxis("Mouse Y") * 0.02f;
            }

            rotationYAxis += velocityX;
            rotationXAxis -= velocityY;
            rotationXAxis = ClampAngle(rotationXAxis, yMinLimit, yMaxLimit);
            var toRotation = Quaternion.Euler(rotationXAxis, rotationYAxis, 0);
            var rotation = toRotation;

            distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 5, distanceMin, distanceMax);
            //RaycastHit hit;
            //if (Physics.Linecast(target.position, transform.position, out hit)) {
            //    distance -= hit.distance;
            //}
            var negDistance = new Vector3(0.0f, 0.0f, -distance);
            var position = rotation * negDistance + target.position;

            transform.rotation = rotation;
            transform.position = position;
            velocityX = Mathf.Lerp(velocityX, 0, Time.deltaTime * smoothTime);
            velocityY = Mathf.Lerp(velocityY, 0, Time.deltaTime * smoothTime);
        }
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}