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
using SKStudios.Common.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace SKStudios.Portals {
    /// <summary>
    ///     Class that controls early depth fragment rejection for VR rendering. This can save up to 33% rendertime on OpenVR
    ///     devices... in theory.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [Obsolete("This method of optimization was never able to actually function; Don't expect to get higher" +
              "perf by employing this component.")]
    public class EarlyDepthRejectionOptimization : MonoBehaviour {
        private Camera _camera;

        private Material _depthFillMat;

        private CommandBuffer _depthRejBuffer;

        private Camera Camera {
            get {
                if (!_camera)
                    _camera = GetComponent<Camera>();
                return _camera;
            }
        }

        private Material DepthFillMat {
            get {
                if (!_depthFillMat)
                    _depthFillMat = new Material(Shader.Find("Custom/HiddenArea"));
                return _depthFillMat;
            }
        }

        /// <summary>
        ///     The Camera Event to call the cmdbuffer on
        /// </summary>
        private CameraEvent CamEvent {
            get {
                return Camera.actualRenderingPath == RenderingPath.Forward
                    ? CameraEvent.BeforeForwardOpaque
                    : CameraEvent.BeforeGBuffer;
            }
        }

        /// <summary>
        ///     CommandBuffer that rejects
        /// </summary>
        private CommandBuffer DepthRejBuffer {
            get {
                if (_depthRejBuffer == null) {
                    _depthRejBuffer = new CommandBuffer();
                    /*For debug purposes, fill the entire screen with a quad. Note that positioning does not matter that much, as 
                    the shader completely lacks a vertex shader whatsoever.*/
                    _depthRejBuffer.DrawMesh(PrimitiveHelper.GetPrimitiveMesh(PrimitiveType.Quad),
                        Matrix4x4.identity * Matrix4x4.TRS(Vector3.forward * 0.3f, Quaternion.identity, Vector3.one),
                        DepthFillMat);
                }
                return _depthRejBuffer;
            }
        }

        
        private void OnEnable()
        {
            Camera.opaqueSortMode = OpaqueSortMode.FrontToBack;
            Camera.AddCommandBuffer(CamEvent, DepthRejBuffer);
        }

        private void OnDisable()
        {
            Camera.RemoveCommandBuffer(CamEvent, DepthRejBuffer);
        }
    }
}