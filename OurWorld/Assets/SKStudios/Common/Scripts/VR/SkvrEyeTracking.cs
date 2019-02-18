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

using System.Collections.Generic;
using UnityEngine;

namespace SKStudios.Common.Utils.VR {
    /// <summary>
    ///     Class that exposes eye offsets for custom stereoscopic rendering
    /// </summary>
    public class SkvrEyeTracking {
        private static readonly Dictionary<Camera, SkvrEyeTrackingComponent> CameraDict;

        static SkvrEyeTracking()
        {
            CameraDict = new Dictionary<Camera, SkvrEyeTrackingComponent>();
        }

        /// <summary>
        ///     Gets the eye offset of a given camera, scaled and rotated properly, relative to the head.
        ///     Outputs the value for the left eye. For right eye offset, multiply by negative 1.
        /// </summary>
        /// <param name="cam">Camera to get the eye offsets of</param>
        /// <returns></returns>
        public static Vector3 EyeOffset(Camera cam)
        {
            SkvrEyeTrackingComponent skEyeTracking;
            if (CameraDict.ContainsKey(cam)) {
                skEyeTracking = CameraDict[cam];
            }
            else {
                skEyeTracking = cam.gameObject.AddComponent<SkvrEyeTrackingComponent>();
                CameraDict.Add(cam, skEyeTracking);
            }

            return skEyeTracking.EyeOffset;
        }
    }

    public class SkvrEyeTrackingComponent : MonoBehaviour {
        private static Camera _camera;

        public static Vector3 TempOffset = Vector3.zero;
        private bool _computedThisFrame;

        private Vector3 _eyeOffset;

        private Camera _mCam;

        public SkvrEyeTrackingComponent(Vector3 eyeOffset)
        {
            _eyeOffset = eyeOffset;
        }

        private static Camera CamComp {
            get {
                if (!_camera)
                    _camera = Camera.main;
                return _camera;
            }
        }

        private Camera Cam {
            get {
                if (!_mCam)
                    _mCam = GetComponent<Camera>();
                return _mCam;
            }
        }

        /// <summary>
        ///     Return the offset of the eyes from the head, in global scale, unrotated.
        /// </summary>
        public Vector3 EyeOffset {
            get {
                Vector3 output;
                if (_computedThisFrame) return _eyeOffset;
#if false
                var left =
 /*Quaternion.Inverse(UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.RightEye)) */
                           UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.LeftEye);

                //Eyes not even being tracked through this interface, time for plan B
                if (left != Vector3.zero)
                {
                    var right =
 /*Quaternion.Inverse(UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.LeftEye)) */
                                UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.RightEye);

                    var offset = (left - right) / 2f;
                    /*var m = Cam.cameraToWorldMatrix;

                    var leftWorld = m.MultiplyPoint(-offset);
                    var rightWorld = m.MultiplyPoint(offset);
                    return Quaternion.Inverse(transform.rotation) * ((leftWorld - rightWorld) / 2f);*/

                    output = offset;
                }
                else
                {
#endif
                var offset = new Vector3(-Cam.stereoSeparation, 0, 0);
                offset /= 2;

                //UnityEngine.Debug.Log(Cam.transform.rotation);
                // if (CamComp.transform.parent != null)
                //   offset = Vector3.Scale(offset, CamComp.transform.parent.lossyScale);
                output = offset;
#if false
            }
#endif
                _eyeOffset = output;
                _computedThisFrame = true;
                return output;
            }
        }

        public void Update()
        {
            _computedThisFrame = false;
        }
    }
}