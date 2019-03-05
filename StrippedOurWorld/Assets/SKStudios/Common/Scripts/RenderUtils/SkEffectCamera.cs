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

using System;
using System.Collections;
using System.Collections.Generic;
using SKStudios.Common.Debug;
using SKStudios.Common.Rendering;
using SKStudios.Common.Utils;
using SKStudios.Portals;
using SKStudios.ProtectedLibs.Rendering;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SKStudios.Rendering {
    /// <summary>
    ///     Class handling Effects best described as "rendering through an object", i.e. Portals, Mirrors, and similar.
    /// </summary>
    public class SkEffectCamera : MonoBehaviour {
        public static int CurrentDepth;

        //Keeps track of cameras actively rendering during this frame for image processing
        public static List<Camera> RenderingCameras = new List<Camera>();
        public static Rect LastRect;

        private static Camera _mainCamera;

        //Render lib
        private SKSRenderLib _cameraLib;

        private int _recursionNumber;

        private RenderData _renderDataTemplate;
        public bool Initialized;
        private Camera[] RecursionCams;

        private static Camera MainCamera {
            get {
                if (!_mainCamera)
                    _mainCamera = Camera.main;
                return _mainCamera;
            }
        }

        private SKSRenderLib CameraLib {
            get {
                if (!_cameraLib)
                    _cameraLib = gameObject.AddComponent<SKSRenderLib>();
                return _cameraLib;
            }
        }


        public Transform RenderingCameraParent { get; set; }

        public Transform OriginTransform { get; private set; }
        public Transform DestinationTransform { get; private set; }

        public Camera PrimaryCam { get; set; }
        public Mesh BlitMesh { get; set; }


        private SkEffectCamera OtherCamera { get; set; }

        private RenderData RenderDataTemplate {
            get {
                if (_renderDataTemplate == null) {
                    if (MainCamera == null)
                        return null;

                    _renderDataTemplate = new RenderData(MainCamera.pixelRect.size,
                        SKSGlobalRenderSettings.RecursionNumber,
                        SKSGlobalRenderSettings.AdaptiveQuality, 0);
                    _renderDataTemplate.InitCache();
                }

                return _renderDataTemplate;
            }
        }


        /// <summary>
        ///     Initialize this SKEffectCamera
        /// </summary>
        /// <param name="other">The Sister SKSEffectCamera</param>
        /// <param name="material">The Material to be rendered to</param>
        /// <param name="sourceRenderer">Source renderer</param>
        /// <param name="targetRenderer">Target Renderer</param>
        /// <param name="mesh">Mesh to be rendered to</param>
        public void Initialize(SkEffectCamera other, Material material, MeshRenderer sourceRenderer,
            MeshRenderer targetRenderer, Mesh mesh, Transform originTransform, Transform destinationTransform)
        {
            if (Initialized) {
                return;
#if SKS_DEV
                SKLogger.LogWarning("SKEffect Camera " + name + "initialized twice; Stacktrace follows" + StackTraceUtility.ExtractStackTrace(), SKOrgType.SKS);
#endif
            }
            StartCoroutine(InitializeRoutine(other, material, sourceRenderer, targetRenderer, mesh, originTransform,
                destinationTransform));
        }

        private IEnumerator InitializeRoutine(SkEffectCamera other, Material material, MeshRenderer sourceRenderer,
            MeshRenderer targetRenderer, Mesh mesh, Transform originTransform, Transform destinationTransform)
        {
            while (RenderDataTemplate == null) yield return WaitCache.Frame;
            OtherCamera = other;
            PrimaryCam = GetComponent<Camera>();

            PrimaryCam.enabled = false;
            //todo: This got moved down, it might need to say here.
            CameraLib.Initialize(originTransform, destinationTransform);

            OriginTransform = originTransform;
            DestinationTransform = destinationTransform;
            RenderDataTemplate.Material = material;
            RenderDataTemplate.SourceRenderer = sourceRenderer;
            RenderDataTemplate.TargetRenderer = targetRenderer;
            RenderDataTemplate.SourceCollider = sourceRenderer.GetComponent<Collider>();
            RenderDataTemplate.TargetCollider = targetRenderer.GetComponent<Collider>();
            RenderDataTemplate.Mesh = mesh;

            //Init cache
            for (var i = 0; i < (int) TextureTargetEye.Count; i++)
                RenderDataTemplate.RenderDataCache[i] = RenderDataTemplate.Clone();

            RenderDataTemplate.InitCache();
            UpdateMaterial(material);

            InstantiateRecursion(SKSGlobalRenderSettings.RecursionNumber);
            //CameraLib.Initialize(originTransform, destinationTransform, RecursionCams);
            Initialized = true;
        }

        /// <summary>
        ///     Updates the material to be rendered to
        /// </summary>
        /// <param name="m"></param>
        public void UpdateMaterial(Material m)
        {
            StartCoroutine(UpdateMaterialRoutine(m));
        }

        private IEnumerator UpdateMaterialRoutine(Material m)
        {
            while (RenderDataTemplate == null)
                yield return WaitCache.Frame;
            RenderDataTemplate.Material = m;
            for (var i = 0; i < (int) TextureTargetEye.Count; i++)
                RenderDataTemplate.RenderDataCache[i].Material = m;
        }

        /// <summary>
        ///     Updates the target renderer
        /// </summary>
        /// <param name="targetRenderer"></param>
        public void UpdateTargetRenderer(MeshRenderer targetRenderer)
        {
            RenderDataTemplate.TargetRenderer = targetRenderer;
            for (var i = 0; i < (int) TextureTargetEye.Count; i++)
                RenderDataTemplate.RenderDataCache[i].TargetRenderer = targetRenderer;
        }

        /// <summary>
        ///     Updates the mesh to be rendered to
        /// </summary>
        /// <param name="mesh"></param>
        public void UpdateMesh(Mesh mesh)
        {
            RenderDataTemplate.Mesh = mesh;
            for (var i = 0; i < (int) TextureTargetEye.Count; i++)
                RenderDataTemplate.RenderDataCache[i].Mesh = mesh;
        }

        /// <summary>
        ///     Instantiation is done on awake
        /// </summary>
        private void Awake()
        {
            enabled = false;
        }

        private void LateUpdate()
        {
            _recursionNumber = 0;
        }

        /// <summary>
        ///     Instantiate recursion cameras
        /// </summary>
        /// <param name="count"></param>
        private void InstantiateRecursion(int count)
        {
            StartCoroutine(InstantiateRecursionEnumerator(count));
        }

        private IEnumerator InstantiateRecursionEnumerator(int count)
        {
            while (RenderDataTemplate == null) yield return WaitCache.Frame;
            count++;

            if (RecursionCams != null)
                for (var i = 0; i < RecursionCams.Length; i++) {
                    if (i <= 0)
                        continue;
                    var cam = RecursionCams[i];
                    Destroy(cam);
                }

            RecursionCams = new Camera[count + 1];
            var camName = this.name + Random.value;
            var mainMarker = gameObject.AddComponent<CameraMarker>();
            mainMarker.Initialize(CameraLib, RenderDataTemplate.SourceRenderer);
            PrimaryCam.stereoTargetEye = StereoTargetEyeMask.None;
            for (var i = 1; i < count; i++) {
                var cam = InstantiateCamera(i);
                cam.name = camName + "Recursor " + i;
            }

            RecursionCams[0] = PrimaryCam;
        }

        /// <summary>
        ///     Sets up and returns a recursion camera at index i
        /// </summary>
        /// <param name="index">index</param>
        /// <returns></returns>
        private Camera InstantiateCamera(int index)
        {
            var cameraRecursor = new GameObject();
#if !SKS_DEV
            cameraRecursor.hideFlags = HideFlags.HideAndDontSave;
#endif
            cameraRecursor.transform.SetParent(RenderingCameraParent);
            cameraRecursor.transform.localPosition = Vector3.zero;
            cameraRecursor.transform.localRotation = Quaternion.identity;

            var marker = cameraRecursor.AddComponent<CameraMarker>();
            marker.Initialize(CameraLib, RenderDataTemplate.SourceRenderer);
            var newCam = cameraRecursor.AddComponent<Camera>();

            newCam.cullingMask = PrimaryCam.cullingMask;
            newCam.renderingPath = PrimaryCam.renderingPath;

            newCam.useOcclusionCulling = PrimaryCam.useOcclusionCulling;
            newCam.depthTextureMode = PrimaryCam.depthTextureMode;
            newCam.enabled = false;

            newCam.ResetProjectionMatrix();
            newCam.ResetWorldToCameraMatrix();
            newCam.ResetCullingMatrix();
            newCam.stereoTargetEye = PrimaryCam.stereoTargetEye;

            RecursionCams[index] = newCam;
            return newCam;
        }


        public void ForceResetRender()
        {
            CurrentDepth = 0;
            _recursionNumber = 0;
        }

        /// <summary>
        ///     Renders the view of a given Renderer as if it were through another renderer. Returns true if successful.
        /// </summary>
        /// <param name="headCam">The origin camera</param>
        /// <param name="material">The Material to modify</param>
        /// <param name="sourceRenderer">The Source renderer</param>
        /// <param name="targetRenderer">The Target renderer</param>
        /// <param name="mesh">The Mesh of the source Renderer</param>
        /// <param name="obliquePlane">Will the projection matrix be clipped at the near plane?</param>
        /// <param name="is3D">Is the renderer not being treated as two-dimenstional?</param>
        /// <param name="isMirror">Is the renderer rendering through itself?</param>
        /// <param name="isSsr">Is the renderer a low quality effect renderer, similar to ssr?</param>
        public bool RenderIntoMaterial(Camera headCam, Material material, MeshRenderer sourceRenderer,
            MeshRenderer targetRenderer, Mesh mesh,
            bool obliquePlane = true, bool is3D = false, bool isMirror = false,
            bool isSsr = false, int pixelPadding = 0, bool recursionEnabled = true)
        {
            if (!Initialized) return false;

            _renderDataTemplate = RenderDataTemplate;
            RenderDataTemplate.ScreenSize = new Vector2(Screen.width, Screen.height);
            //todo: Deprecated; old patch for VR. Remove in next version.
            //RenderDataTemplate.ScreenSize = new Vector2(Screen.width / 2, Screen.height);
#if !SKS_MIRRORS
//if (camera.transform.parent == transform.parent)
//    return;
#endif

            var firstRender = false;
            var renderingCamera = RecursionCams[CurrentDepth];

            //Render Placeholder if max depth hit
            if (CurrentDepth > SKSGlobalRenderSettings.RecursionNumber) return false;

            var renderTarget = headCam.targetTexture;
            var marker = CameraMarker.GetMarker(headCam);

            if (marker)
                if (marker.Owner == OtherCamera)
                    return false;

            Graphics.SetRenderTarget(renderTarget);

            //Sets up the Render Properties for this render
            var renderProps = new RenderProperties();

            //Is this the first time that the IsMirror is being rendered this frame?
            if (headCam == MainCamera)
                firstRender = true;

            renderProps |= firstRender ? RenderProperties.FirstRender : 0;

            renderProps |= RenderProperties.Optimize;
            renderProps |= CurrentDepth < 1
                ? obliquePlane ? RenderProperties.ObliquePlane : 0
                : RenderProperties.ObliquePlane;
            renderProps |= isMirror ? RenderProperties.Mirror : 0;
            renderProps |= isSsr ? RenderProperties.LowQuality : 0;
            renderProps |= SKSGlobalRenderSettings.CustomSkybox ? RenderProperties.RipCustomSkybox : 0;
            renderProps |= SKSGlobalRenderSettings.AggressiveRecursionOptimization
                ? RenderProperties.AggressiveOptimization
                : 0;
            renderProps |= recursionEnabled ? 0 : RenderProperties.RecursionDisabled;
            if (firstRender) {
                renderProps |= SKSGlobalRenderSettings.Inverted ? RenderProperties.InvertedCached : 0;
                renderProps |= SKSGlobalRenderSettings.UvFlip ? RenderProperties.UvFlipCached : 0;
            }

#if SKS_VR
            renderProps |= RenderProperties.Vr;
            renderProps |= SKSGlobalRenderSettings.SinglePassStereo ? RenderProperties.SinglePass : 0;
#endif
            //renderProps &= ~RenderProperties.Optimize;
            _recursionNumber++;

            //Renders the IsMirror itself to the rendertexture
            transform.SetParent(RenderingCameraParent);

            CurrentDepth++;

            renderingCamera.renderingPath = headCam.renderingPath;
            renderingCamera.allowHDR = headCam.allowHDR;
            renderingCamera.cullingMask = headCam.cullingMask;
            renderingCamera.cullingMask |= 1 << Keywords.Layers.CustomRendererOnly;

            renderingCamera.enabled = false;
            RenderingCameras.Add(headCam);

            //Set up the RenderData for the current frame
            RenderDataTemplate.OriginCamera = headCam;
            RenderDataTemplate.RenderingCamera = renderingCamera;
            RenderDataTemplate.CurrentDepth = CurrentDepth;
            RenderDataTemplate.PixelPadding = pixelPadding;

            //Copy per-frame values
            for (var i = 0; i < (int) TextureTargetEye.Count; i++)
                RenderDataTemplate.RenderDataCache[i].CopyFrameData(RenderDataTemplate);




            //In order to prevent overriding the depth texture, this must be enabled at the beginning of rendering and then disabled afterwards
#if TRANSFER_DEPTH
            renderingCamera.depthTextureMode = DepthTextureMode.DepthNormals;
#endif

#if SKS_VR
            var tempDataLeft = RenderDataTemplate.Clone();
            var tempDataRight = RenderDataTemplate.Clone();
//Stereo Rendering
            if (headCam.stereoTargetEye == StereoTargetEyeMask.Both) {
                //Todo: This got replaced. Does the new version help?
                /*RenderingCameraParent.rotation = DestinationTransform.rotation *
                     (Quaternion.Inverse(OriginTransform.rotation) *
                     (headCam.transform.rotation));*/
                renderingCamera.transform.rotation = DestinationTransform.rotation *
                                                     (Quaternion.Inverse(OriginTransform.rotation) *
                                                      (headCam.transform.rotation));


                //Todo: Figure out why this optimization doesn't work in VR mode
                //var tempDataLeft = RenderDataTemplate.RenderDataCache[(int)TextureTargetEye.Left];
                //var tempDataRight = RenderDataTemplate.RenderDataCache[(int)TextureTargetEye.Right];



                //Left eye

                
                //renderingCamera.stereoTargetEye = StereoTargetEyeMask.Left;
                tempDataLeft.ProjectionMatrix = headCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
                //tempDataLeft.Position = headCam.transform.position;
                tempDataLeft.ViewMatrix = headCam.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
                tempDataLeft.TargetEye = TextureTargetEye.Left;
                tempDataLeft.RenderProperties = renderProps;
                _cameraLib.RenderCamera(tempDataLeft);
                
                //RenderEye(StereoTargetEyeMask.Left, renderingCamera, headCam, tempDataLeft);

                //Right eye
                
                //renderingCamera.stereoTargetEye = StereoTargetEyeMask.Right;
                tempDataRight.ProjectionMatrix = headCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
                //tempDataRight.Position = headCam.transform.position;
                tempDataRight.ViewMatrix = headCam.worldToCameraMatrix;

                tempDataRight.TargetEye = TextureTargetEye.Right;
                tempDataRight.RenderProperties = renderProps;

                _cameraLib.RenderCamera(tempDataRight);
                
                //RenderEye(StereoTargetEyeMask.Right, renderingCamera, headCam, tempDataRight);
            }
            else
            {
#endif
            //Non-VR rendering with VR enabled
            var tempData = RenderDataTemplate.Clone();

            renderProps &= ~RenderProperties.SinglePass;
            renderProps &= ~RenderProperties.Vr;

            tempData.RenderProperties = renderProps;
            renderingCamera.transform.rotation = DestinationTransform.rotation *
                                                 (Quaternion.Inverse(OriginTransform.rotation) *
                                                  headCam.transform.rotation);

            tempData.ProjectionMatrix = headCam.projectionMatrix;
            //tempData.Position = headCam.transform.position;
            tempData.ViewMatrix = headCam.worldToCameraMatrix;
            //tempData.ScreenSize = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
#if DEBUG_EFFECT_DEPTH
            Debug.Log("About to Render camera " + tempData.RenderingCamera + " at depth " + CurrentDepth);
#endif
            if (renderingCamera.stereoTargetEye == StereoTargetEyeMask.Left) {
                tempData.TargetEye = TextureTargetEye.Left;
                _cameraLib.RenderCamera(tempData);
            }
            else {
                tempData.TargetEye = TextureTargetEye.Right;
                _cameraLib.RenderCamera(tempData);
            }

            //RenderEye(headCam.stereoTargetEye, renderingCamera, headCam, renderProps);

#if SKS_VR
            }
#endif


#if false
//Non-stereo rendering
//RenderData.Position = camera.transform.position;
            var tempData = RenderDataTemplate.RenderDataCache[(int) TextureTargetEye.Right];
            tempData.ProjectionMatrix = headCam.projectionMatrix;
            tempData.ViewMatrix = headCam.worldToCameraMatrix;
            tempData.RenderProperties = renderProps;
            renderingCamera.transform.rotation = DestinationTransform.rotation *
                                                 (Quaternion.Inverse(OriginTransform.rotation) *
                                                  headCam.transform.rotation);
            tempData.Position = headCam.transform.position;
            CameraLib.RenderCamera(tempData);
#endif
            CurrentDepth--;
            renderingCamera.depthTextureMode = DepthTextureMode.None;
            RenderingCameras.Remove(headCam);
            if (RenderingCameras.Count == 0) { 
                try
                {
                    //_cameraLib.TerminateRender();
                    //SKSRenderLib.ClearUnwinder();
                }
                catch (NullReferenceException)
                {
                    SKLogger.LogWarning("Attempted to render without proper setup");
                }
            }
                

            return true;
        }

        /// <summary>
        ///     Render a given eye
        /// </summary>
        /// <param name="eye"><see cref="StereoTargetEyeMask" /> to render</param>
        /// <param name="renderingCamera">Camera to render with</param>
        /// <param name="headCam">Head camera</param>
        /// <param name="tempData">Temporary render data to use</param>
        private void RenderEye(StereoTargetEyeMask eye, Camera renderingCamera, Camera headCam,
            RenderData tempData)
        {
            var left = eye == StereoTargetEyeMask.Left;
            //renderingCamera.stereoTargetEye = left ? StereoTargetEyeMask.Left : StereoTargetEyeMask.Right;
            var stereoEye = left
                ? Camera.StereoscopicEye.Left
                : Camera.StereoscopicEye.Right;

            tempData.ProjectionMatrix = eye == StereoTargetEyeMask.None
                ? headCam.projectionMatrix
                : headCam.GetStereoProjectionMatrix(
                    stereoEye);

            //tempData.Position = headCam.transform.position;

            tempData.ViewMatrix =
                eye == StereoTargetEyeMask.None
                    ? headCam.worldToCameraMatrix
                    : headCam.GetStereoViewMatrix(stereoEye);

            TextureTargetEye textureTargetEye;

            switch (eye) {
                case StereoTargetEyeMask.Left:
                    textureTargetEye = TextureTargetEye.Left;
                    break;
                case StereoTargetEyeMask.Right:
                    textureTargetEye = TextureTargetEye.Right;
                    break;
                default:
                    textureTargetEye = TextureTargetEye.Unset;
                    break;
            }

            tempData.TargetEye = textureTargetEye;
            tempData.RenderProperties = tempData.RenderProperties;
            _cameraLib.RenderCamera(tempData);
        }

        private void OnDisable()
        {
            //if (_cameraLib)
            //    Destroy(_cameraLib);
            CameraMarker marker;
            if (marker = gameObject.GetComponent<CameraMarker>()) Destroy(marker);

            if (RecursionCams != null)
                foreach (var c in RecursionCams)
                    if (c && c.gameObject)
                        if (c != PrimaryCam)
                            Destroy(c.gameObject);
        }
    }
}