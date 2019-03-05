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
using System.Collections;
using System.Collections.Generic;
using SKStudios.Common;
using SKStudios.Common.Debug;
using SKStudios.Common.Rendering;
using SKStudios.Common.Utils;
using SKStudios.ProtectedLibs.Rendering;
using SKStudios.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace SKStudios.Portals
{
    /// <summary>
    ///     Class designed to spawn and control <see cref="Portal" /> instances.
    /// </summary>
#if UNITY_EDITOR
    [ExecuteInEditMode]
    [InitializeOnLoad]
#endif
    public class PortalController : EffectRendererController
    {
        [SerializeField] private float _detectionScale;

        [SerializeField] private bool _enterable = true;

        [SerializeField] private bool _is3D;

        [SerializeField] private Texture2D _mask;

        [SerializeField] private SkEffectCamera _portalCameraScript;

        [SerializeField] private Material _portalMaterial;

        private Coroutine _portalOpeningAsyncOpRoutine;


        private Vector3 _portalOpeningSize = Vector3.one;

        private GameObject _portalRenderer;

        //The scale of this Portal, for resizing
        private float _portalScale = 1;

        /// <summary>
        ///     The scale of the controller before <see cref="OnEnable"/> is called.
        /// </summary>
        private Vector3 _oldControllerScale;

        private Coroutine _portalScaleAsyncOpRoutine;


        [SerializeField] private Portal _portalScript;

        private PortalTrigger _portalTrigger;

        private Camera _previewCamera;

        private GameObject _previewRoot;

        private bool _setup;

        [SerializeField] private PortalController _targetController;

        private Color _color = Color.white;

#if UNITY_EDITOR
        static PortalController()
        {
            EditorApplication.hierarchyWindowChanged += HierarchyWindowChanged;
        }

        private static Scene _currentScene;
        static void HierarchyWindowChanged()
        {
            if (_currentScene != SceneManager.GetActiveScene())
                UpdateAllPropertyBlocks();

        }

        /// <summary>
        ///     Update the color of the material on all <see cref="PortalController"/>s
        /// </summary>
        public static void UpdateAllPropertyBlocks()
        {
            foreach (var controller in FindObjectsOfType<PortalController>())
                controller.UpdatePropertyBlock(controller.Color);
            _currentScene = SceneManager.GetActiveScene();
        }

        /// <summary>
        ///     Update the color of the material on this <see cref="PortalController"/>
        /// </summary>
        /// <param name="color">Color to change to</param>
        public void UpdatePropertyBlock(Color color)
        {
            Block.SetColor("_Color", color);
            if (!Application.isPlaying)
            {
                var rend = GetComponent<Renderer>();
                if (rend)
                    rend.SetPropertyBlock(Block);
            }

        }
#endif
        /// <summary>
        ///     What is the color of the <see cref="PortalController"/> in the editor?
        /// </summary>
        public Color Color {
            get {
#if UNITY_EDITOR
                if (_color != Color.white) return _color;
                var seed = UnityEngine.Random.state;
                UnityEngine.Random.InitState(GetInstanceID());
                _color = UnityEngine.Random.ColorHSV(0, 1, 0.48f, 0.48f, 0.81f, 0.81f);
                UnityEngine.Random.state = seed;
                return _color;
#else
                return Color.white;
#endif
            }
        }

        [SerializeField] public bool IgnoreRearCollisions = true;


        [HideInInspector] public bool NonObliqueOverride;

        private Material _originalMaterial;
        /// <summary>
        ///     <see cref="Material"/> that this <see cref="PortalController"/> uses while in the Editor.
        /// </summary>
        public Material OriginalMaterial {
            get {
                if (_originalMaterial == null)
                    _originalMaterial = GetComponent<Renderer>().sharedMaterial;
                return _originalMaterial;
            }
        }

        /// <summary>
        ///     The Instantiated copy of the <see cref="Portal"/> prefab.
        /// </summary>
        private GameObject _portalObject;

        /// <summary>
        ///     The root for preview camera targeting
        /// </summary>
        public GameObject PreviewRoot {
            get {
                if (!_previewRoot)
                {
#if UNITY_EDITOR
                    _previewRoot = EditorUtility.CreateGameObjectWithHideFlags("Preview Root",
                        HideFlags.HideAndDontSave | HideFlags.NotEditable);
#else
                    _previewRoot = new GameObject();
#endif
#if !SKS_DEV
                    _previewRoot.hideFlags = HideFlags.HideAndDontSave;
#endif
                    _previewRoot.transform.localPosition = transform.position;
                    _previewRoot.transform.localRotation = Quaternion.AngleAxis(180, transform.up) * transform.rotation;
                    _previewRoot.transform.localScale = transform.lossyScale;
                    _previewRoot.transform.SetParent(transform, true);

                    _previewRoot.AddComponent<MeshRenderer>();
                    var filter = _previewRoot.AddComponent<MeshFilter>();
                    filter.mesh = Resources.Load<Mesh>("Meshes/PortalPreview");
                    if (_previewRoot)
                        _previewRoot.SetActive(false);
                }
                else
                {
                    if (!gameObject.activeInHierarchy)
                    {
                        DestroyImmediate(_previewRoot, true);
                        return null;
                    }
                }

                _previewRoot.tag = Keywords.Tags.SKEditorTemp;
                return _previewRoot;
            }
        }

        /// <summary>
        ///     The Camera for Preview Rendering
        /// </summary>
        public Camera PreviewCamera {
            get {
                if (!_previewCamera)
                {
#if UNITY_EDITOR
                    var previewCamera = EditorUtility.CreateGameObjectWithHideFlags("Preview Camera",
                        HideFlags.HideAndDontSave | HideFlags.NotEditable);
#if !SKS_DEV
                    previewCamera.hideFlags = HideFlags.HideAndDontSave;
#endif
                    previewCamera.transform.SetParent(transform);
                    previewCamera.transform.localPosition = Vector3.zero;
                    previewCamera.transform.localScale = Vector3.one;

                    var cam = previewCamera.AddComponent<Camera>();

                    cam.cullingMask |= 1 << Keywords.Layers.CustomRenderer;
                    cam.enabled = false;


                    _previewCamera = previewCamera.GetComponent<Camera>();
                    _previewCamera.useOcclusionCulling = false;

                    var lib = previewCamera.AddComponent<SKSRenderLib>();

                    lib.Initialize(PreviewRoot.transform, TargetController.transform);
#endif
                }
                else
                {
                    if (!gameObject.activeInHierarchy)
                    {
                        DestroyImmediate(_previewCamera, true);
                        return null;
                    }
                }

                _previewCamera.tag = Keywords.Tags.SKEditorTemp;
                return _previewCamera;
            }
        }

        /// <summary>
        ///     The <see cref="PortalController"/> that this is connected to.
        /// </summary>
        public PortalController TargetController {
            get { return _targetController; }
            set {
                _targetController = value;
                if (_targetController && _targetController.TargetController == null)
                {
#if UNITY_EDITOR
                    Undo.RecordObject(_targetController, "Automatic Portal Linking");
#endif
                    _targetController.TargetController = this;
                }

                if (PortalScript)
                    _portalScript.Target = TargetController.GetComponentInChildren<Portal>(true);
            }
        }

        public float PortalScale {
            get { return _portalScale; }
            set {
                if (!_setup && _portalScaleAsyncOpRoutine == null)
                {
                    _portalScaleAsyncOpRoutine = StartCoroutine(PortalScaleAsyncOp(value));
                    return;
                }

                _portalScale = value;
                _portalScript.Origin.localScale = Vector3.one * _portalScale;
                if (_portalScript.Target.ArrivalTarget)
                    _portalScript.Target.ArrivalTarget.localScale = Vector3.one * _portalScale;
            }
        }

        //The actual unit size of the Portal opening
        [HideInInspector]
        public Vector3 PortalOpeningSize {
            get { return _portalOpeningSize; }
            set {
                if (_portalOpeningSize == value) return;
                if (!_setup && _portalOpeningAsyncOpRoutine == null)
                {
                    _portalOpeningAsyncOpRoutine = StartCoroutine(PortalOpeningAsyncOp(value));
                    return;
                }

                _portalOpeningSize = value;
                PortalRendererObject.transform.localScale = _portalOpeningSize;
            }
        }


        /// <summary>
        ///     Is the Portal enterable?
        /// </summary>
        public bool Enterable {
            get { return _enterable; }
            set {
                _enterable = value;
                if (PortalScript)
                    PortalScript.Enterable = _enterable;
            }
        }


        /// <summary>
        ///     The scale of the detection zone for this portal
        /// </summary>
        public float DetectionScale {
            get { return _detectionScale; }
            set {
                _detectionScale = value;
                if (PortalTrigger) PortalTrigger.transform.localScale = new Vector3(1, 1, _detectionScale);
            }
        }

        public Portal PortalScript {
            get {
                if (!_portalScript)
                    _portalScript = GetComponentInChildren<Portal>(true);
                return _portalScript;
            }
        }

        private SkEffectCamera PortalCameraScript {
            get {
                if (!_portalCameraScript) _portalCameraScript = GetComponentInChildren<SkEffectCamera>(true);
                return _portalCameraScript;
            }
        }

        private PortalTrigger PortalTrigger {
            get {
                if (!_portalTrigger)
                    _portalTrigger = GetComponentInChildren<PortalTrigger>(true);
                return _portalTrigger;
            }
        }

        private GameObject PortalRendererObject {
            get {
                if (_portalRenderer == null)
                    _portalRenderer = PortalScript.transform.parent.gameObject;
                return _portalRenderer;
            }
        }

        private IEnumerator PortalScaleAsyncOp(float value)
        {
            while (!_setup) yield return WaitCache.Frame;
            PortalScale = value;
        }

        private IEnumerator PortalOpeningAsyncOp(Vector3 value)
        {
            while (!_setup) yield return WaitCache.Frame;
            PortalOpeningSize = value;
        }


        private void OnEnable()
        {
            ConsoleCallbackHandler.AddCallback(() => {
                StartCoroutine(ConsoleTheConsole());
            }, LogType.Error, "A PreviewRenderUtility");

            CleanupTemp();
            if (TargetController)
                TargetController.CleanupTemp();
            var rend = GetComponent<Renderer>();
            if (rend)
                rend.enabled = true;
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            SKSGlobalRenderSettings.Instance.OnEnable();

            if(!_setup)
                _setupCoroutine = StartCoroutine(Setup());
        }

        //I am a master of wit
        private IEnumerator ConsoleTheConsole()
        {
            yield return WaitCache.Frame;
            Debug.ClearDeveloperConsole();
        }


        private void OnDisable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;

#endif
            //Old disable logic; no longer necessary
            /*_setup = false;
            Destroy(_portalObject);
            if (_oldControllerScale != Vector3.zero)
                transform.localScale = _oldControllerScale;*/
        }

        private Coroutine _setupCoroutine = null;

        private IEnumerator Setup()
        {
            //Move all children to portal for accurate scaling
            var childTransforms = new List<Transform>();
            foreach (Transform t in transform)
                childTransforms.Add(t);


            _portalObject = Instantiate(Settings.RendererPrefab, transform);


            _portalObject.transform.localPosition = Vector3.zero;
            _portalObject.transform.localRotation = Quaternion.Euler(0, 180, 0);
            _portalObject.transform.localScale = Vector3.one;
            _portalObject.name = "Portal";

            Destroy(gameObject.GetComponent<MeshRenderer>());

            PortalScript.Settings = Settings;
            PortalScript.MeshRenderer.sharedMaterial.SetFloat(Keywords.ShaderKeys.ZTest, (int)CompareFunction.Less);

            while (!PortalScript || !TargetController.PortalScript) yield return WaitCache.Frame;
            TargetController = TargetController;

            Enterable = Enterable;

            foreach (var t in childTransforms)
                t.SetParent(PortalScript.Origin);

            yield return WaitCache.Frame;
            //PortalScript.Settings = Settings;
            _portalObject.SetActive(true);

            PortalCameraScript.RenderingCameraParent = PortalCameraScript.transform.parent;

            PortalScript.NonObliqueOverride = NonObliqueOverride;
            PortalScript.PhysicsPassthrough = GetComponentInChildren<PhysicsPassthrough>();

            DetectionScale = DetectionScale;

            PortalScript.IgnoreRearCollisions = IgnoreRearCollisions;

            PortalTrigger.Portal = PortalScript;

            //Enable scripts
            PortalScript.enabled = true;

            //todo: Update this for the new material editor
            PortalScript.MeshRenderer.lightProbeUsage = LightProbeUsage.Off;

            PortalCameraScript.enabled = true;
            PortalTrigger.enabled = true;
            _setup = true;
            //Transfer transform values to modifiable var
            PortalOpeningSize = transform.localScale;

            _oldControllerScale = transform.localScale;
            transform.localScale = Vector3.one;

#if !SKS_DEV
            _portalObject.ApplyHideFlagsRecursive(HideFlags.HideAndDontSave);
#endif
            UpdateScale();
            if (gameObject.isStatic)
            {
                PortalScript.gameObject.isStatic = true;
                PortalTrigger.gameObject.isStatic = true;
            }
        }

        private void Update()
        {
            if (!TargetController) return;
            if (!Application.isPlaying) return;
            Debug.DrawLine(transform.position, TargetController.transform.position, Color);
            if (_setup && !gameObject.isStatic)
                UpdateScale();
        }

        private void UpdateScale()
        {
            PortalScale = PortalRendererObject.transform.lossyScale.x;
            transform.localScale = Vector3.one;
        }

        private MaterialPropertyBlock _block;
        private MaterialPropertyBlock Block {
            get { return _block ?? (_block = new MaterialPropertyBlock()); }
        }

        /// <summary>
        ///     Draw the Portal Controller Gizmos.
        /// </summary>
        public void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (!SKSGlobalRenderSettings.Gizmos)
                return;

            var style = new GUIStyle();
            style.normal.textColor = Color.red;

            if (!Settings)
            {
                Handles.Label(transform.position, "No Settings Set", style);
                return;
            }

            if (!TargetController)
            {
                Handles.Label(transform.position, "No Target Set", style);
                return;
            }

            if (!Settings.OriginalMaterial)
            {
                Handles.Label(transform.position, "No Portal Material Set", style);
                return;
            }

            if (!Settings.RendererPrefab)
            {
                Handles.Label(transform.position, "No Portal Prefab Set", style);
                return;
            }

            if (!Settings.Mask && !SKSGlobalRenderSettings.ShouldOverrideMask ||
                SKSGlobalRenderSettings.ShouldOverrideMask && !SKSGlobalRenderSettings.Mask)
            {
                Handles.Label(transform.position, "No Mask Set", style);
                return;
            }


            Gizmos.color = Color.clear;
            if (PortalScript)
            {
                Gizmos.matrix = PortalScript.transform.localToWorldMatrix;
                Gizmos.matrix *= Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, 0.1f));
                Gizmos.DrawMesh(PrimitiveHelper.GetPrimitiveMesh(PrimitiveType.Cube));
            }

#endif
        }

        /// <summary>
        ///     Removes preview objects from the scene
        /// </summary>
        public void CleanupTemp()
        {
            if (this)
                CleanupTempRecursive(transform);
        }

        private void CleanupTempRecursive(Transform targetTransform)
        {
            foreach (Transform t in targetTransform)
            {
                CleanupTempRecursive(t);

                if (t.gameObject.CompareTag(Keywords.Tags.SKEditorTemp))
                    DestroyImmediate(t.gameObject, true);
            }
        }

#if UNITY_EDITOR
        public void OnDrawGizmosSelected()
        {
            if (!this ||
                !SKSGlobalRenderSettings.Gizmos ||
                !gameObject || !transform || !TargetController)
                return;

            var defaultSize = 1f;
            //Dir vector to second portal
            var diffVector = (TargetController.transform.position - transform.position).normalized;

            if (TargetController && SKSGlobalRenderSettings.Visualization)
            {
                var iterations =
                    Mathf.RoundToInt(Vector3.Distance(transform.position, TargetController.transform.position) / 1f);
                for (var i = 0; i < iterations; i++)
                {
                    var timeScalar = ((float)i / iterations + Time.time / 5f) % 1f;


                    //Set the color to be between the two portal's colors
                    var arrowColor = Color.Lerp(Color, TargetController.Color, timeScalar);
                    arrowColor.a = 0.5f;
                    Handles.color = arrowColor;

                    var arrowSize = defaultSize;

                    //Place arrows and move them accordingly
                    var arrowPosition = Vector3.Lerp(transform.position - diffVector * arrowSize,
                        TargetController.transform.position, timeScalar);


                    //Scale arrows down as they approach destination
                    var distanceToTarget = Vector3.Distance(arrowPosition, TargetController.transform.position);
                    float distanceToOrigin;
                    if (distanceToTarget <= 1)
                        arrowSize *= distanceToTarget;
                    //Scale arrows up as they leave origin
                    else if ((distanceToOrigin =
                                 Vector3.Distance(arrowPosition + diffVector * arrowSize, transform.position)) <=
                             1) arrowSize *= distanceToOrigin;

                    //Scale arrows up as they spawn from origin

                    Handles.ArrowCap(0, arrowPosition,
                        Quaternion.LookRotation(diffVector, Vector3.up),
                        arrowSize);
                }

                //Draw children's gizmos
                foreach (Transform t in transform)
                    if (t != transform)
                        RecursiveTryDrawGizmos(t);

                //Draw detection zone preview

                //Gizmos.DrawMesh(PrimitiveHelper.GetPrimitiveMesh(PrimitiveType.Cube));
                if (!Application.isPlaying)
                {
                    Gizmos.color = new Color(1, 1, 1, 0.2f);
                    var detectionCenter = transform.position -
                                          transform.forward * (DetectionScale / 2f) * transform.lossyScale.z / 2f;
                    Gizmos.DrawWireMesh(PrimitiveHelper.GetPrimitiveMesh(PrimitiveType.Cube), detectionCenter,
                        transform.rotation,
                        Vector3.Scale(transform.lossyScale, new Vector3(1, 1, DetectionScale / 2f)));
                    var style = new GUIStyle(new GUIStyle { alignment = TextAnchor.MiddleCenter });
                    style.normal.textColor = Color.white;
                    Handles.Label(detectionCenter, "Portal Detection zone", style);
                }
            }
        }

        private RenderData _renderData;

        /// <summary>
        ///     Handles scenveview rendering of portal previews and related editor utilities
        /// </summary>
        private void OnWillRenderObject()
        {
#if UNITY_EDITOR
            if (!(Selection.activeGameObject == gameObject)) return;

            if (!TargetController || !Settings || !Settings.OriginalMaterial ||
                !Settings.Mask || !SKSGlobalRenderSettings.Preview ||
                !this || Application.isPlaying)
                return;


            var previewRenderer = PreviewRoot.GetComponent<MeshRenderer>();
            previewRenderer.sharedMaterial = Settings.OriginalMaterial;
            //previewRenderer.enabled = true;

            var lib = PreviewCamera.GetComponent<SKSRenderLib>();
            PreviewCamera.transform.localPosition = Vector3.zero;

            var sceneCam = SceneView.GetAllSceneCameras()[0];

            var cam = PreviewCamera;


            GL.Clear(true, true, Color.black);
            Graphics.SetRenderTarget(null);

            var renderProps = new RenderProperties();

            //renderState |= RenderState.FirstRender;
            renderProps |= RenderProperties.Optimize;
            renderProps |= SKSGlobalRenderSettings.Inverted ? RenderProperties.InvertedCached : 0;
            renderProps |= !SKSGlobalRenderSettings.UvFlip ? RenderProperties.UvFlipCached : 0;
            renderProps |= RenderProperties.ObliquePlane;
            renderProps |= RenderProperties.FirstRender;
            renderProps |= RenderProperties.RipCustomSkybox;

            var rend2 = TargetController.GetComponent<MeshRenderer>();
            var mesh = PreviewRoot.GetComponent<MeshFilter>().sharedMesh;
            //TargetController.PreviewRoot.GetComponent<MeshRenderer>().enabled = false;
            //TargetController.GetComponent<MeshRenderer>().enabled = false;
            cam.transform.localPosition = Vector3.zero;

            TargetController.PreviewRoot.transform.localPosition = Vector3.zero;

            cam.transform.rotation = TargetController.PreviewRoot.transform.rotation *
                                     (Quaternion.Inverse(transform.rotation) *
                                      sceneCam.transform.rotation);

            TargetController.PreviewRoot.transform.localScale = Vector3.one;

            if (_renderData == null)
            {
                _renderData = new RenderData(renderProps, cam, sceneCam,
                    sceneCam.projectionMatrix, TextureTargetEye.Left,
                    Settings.OriginalMaterial, new Vector2(Screen.currentResolution.width,
                        Screen.currentResolution.height), previewRenderer, rend2, null, null, mesh, 1, 0, false, 0, Matrix4x4.identity);
            }
            else
            {
                //_renderData.Position = sceneCam.transform.position;
                _renderData.ProjectionMatrix = sceneCam.projectionMatrix;
                _renderData.ScreenSize = new Vector2(Screen.currentResolution.width,
                    Screen.currentResolution.height);
                _renderData.RenderingCamera = PreviewCamera;
                _renderData.SourceRenderer = previewRenderer;
            }

            try
            {
                lib.RenderCamera(_renderData);
            }
            catch (Exception)
            {
                lib.Initialize(PreviewRoot.transform, TargetController.transform);

                //Doesn't really matter what happened here, unity editor strangeness sometimes hucks issues
                Graphics.SetRenderTarget(null);
                lib.TerminateRender();
                return;
            }

            Graphics.SetRenderTarget(null);
            lib.TerminateRender();
#endif
        }

        /// <summary>
        ///     Draw all gizmos for an object and its children, even if the children
        ///     are hidden in the inspector
        /// </summary>
        /// <param name="target">Transform to target</param>
        private void RecursiveTryDrawGizmos(Transform target)
        {
            foreach (Transform t in target)
            {
                var behaviour = t.GetComponents<MonoBehaviour>();
                if (behaviour.Length > 0)
                    behaviour[0].SendMessage("OnDrawGizmosSelected", SendMessageOptions.DontRequireReceiver);
                RecursiveTryDrawGizmos(t);
            }
        }
#endif
    }
}