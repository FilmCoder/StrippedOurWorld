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
using SKStudios.Common.Debug;
using SKStudios.Common.Rendering;
using SKStudios.Common.Utils;
using SKStudios.Portals;
using SKStudios.Rendering;
using SK_Common.Extensions;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace SKStudios.Common {
    /// <summary>
    ///     Component used on an object to trigger <see cref="SkEffectCamera" /> rendering.
    /// </summary>
    public abstract class EffectRenderer : MonoBehaviour {
        /// <summary>
        ///     Small epsilon value used for various purposes
        /// </summary>
        private const float FudgeFactor = 0.001f;

        protected static Camera _HeadCamera;

        private Transform _arrivalTarget;

        /// <summary>
        ///     Marker for a number of recursion and head intersection checks. Referred to as a "cheese" since
        ///     it's a cheesy way to solve a large number of problems, but it's also monstrously effective.
        /// </summary>
        protected int _CheeseActivated = -1;

        /// <summary>
        ///     Calls the rendering of a Effect frame
        /// </summary>
        private SkEffectCamera _effectCamera;

        private MeshFilter _meshFilter;

        private MeshRenderer _meshRenderer;

        protected Vector3[] _NearClipVertsGlobal;
        protected Vector3[] _NearClipVertsLocal;

        private Transform _origin;

        private SkCustomRendererPlaceholder _placeholder;

        private MeshRenderer _placeholderRenderer;

        private Mesh _rendererMesh;

        protected MaterialPropertyBlock _SeamlessRecursionBlock;

        private Transform _seamlessRecursionFix;

        protected Renderer _SeamlessRecursionRenderer;

        /// <summary>
        ///     ScriptableObject containing settings for this <see cref="EffectRenderer" />.
        /// </summary>
        [HideInInspector] public EffectRendererSettings Settings;

        /// <summary>
        ///     The location where the default material for this EffectRenderer can be found
        /// </summary>
        public abstract string DefaultMaterialLocation { get; }

        /// <summary>
        ///     Is the EffectRenderer rendering with mirrored rendering rules?
        /// </summary>
        public abstract bool IsMirror { get; }

        protected static Camera HeadCamera {
            get {
                if (_HeadCamera == null) _HeadCamera = Camera.main;
                return _HeadCamera;
            }
        }

        /// <summary>
        ///     Should cameras use Oblique culling? (Default True)
        /// </summary>
        public bool NonObliqueOverride { get; set; }

        /// <summary>
        ///     The transform of an object used to fix recursion issues
        /// </summary>
        public Transform SeamlessRecursionFix {
            get {
                if (_seamlessRecursionFix == null)
                    _seamlessRecursionFix = transform.parent.Find("SeamlessRecursionFix");

                return _seamlessRecursionFix;
            }
        }

        protected Renderer SeamlessRecursionRenderer {
            get {
                if (!_SeamlessRecursionRenderer)
                    _SeamlessRecursionRenderer = SeamlessRecursionFix.GetComponent<Renderer>();
                return _SeamlessRecursionRenderer;
            }
        }

        /// <summary>
        ///     The Mesh Renderer used by the Renderer. Used for determining Renderer size on screen.
        /// </summary>
        public MeshRenderer MeshRenderer {
            get {
                if (!_meshRenderer) _meshRenderer = GetComponent<MeshRenderer>();

                return _meshRenderer;
            }
            set { _meshRenderer = value; }
        }

        /// <summary>
        ///     The Mesh Filter used by the Renderer. Also used for determining Renderer size on screen.
        /// </summary>
        public MeshFilter MeshFilter {
            get {
                if (!_meshFilter)
                    _meshFilter = gameObject.GetComponent<MeshFilter>();
                return _meshFilter;
            }
            set { _meshFilter = value; }
        }

        private Mesh RendererMesh {
            get {
                if (_rendererMesh == null)
                    _rendererMesh = MeshFilter.sharedMesh;
                return _rendererMesh;
            }
        }

        /// <summary>
        ///     Placeholder for when recursion bottoms out
        /// </summary>
        public SkCustomRendererPlaceholder Placeholder {
            get {
                var p = transform.parent.GetComponentInChildren<SkCustomRendererPlaceholder>();
                if (p)
                    _placeholder = p;
                return _placeholder;
            }
        }

        /// <summary>
        ///     The Mesh Renderer used by the placeholder.
        /// </summary>
        public MeshRenderer PlaceholderMeshRenderer {
            get {
                if (!_placeholderRenderer) _placeholderRenderer = Placeholder.GetComponent<MeshRenderer>();
                return _placeholderRenderer;
            }
            set { _placeholderRenderer = value; }
        }

        /// <summary>
        ///     The Material for the Renderer to use. Holds a copy of
        ///     <see cref="EffectRendererSettings.OriginalMaterial" /> in play modea after <see cref="Start" />.
        /// </summary>
        public Material RenderMaterial {
            get {
                var renderMaterial = MeshRenderer.sharedMaterial;
                if (!renderMaterial) {
                    renderMaterial = new Material(Resources.Load<Material>(DefaultMaterialLocation));
                    UpdateMaterial(renderMaterial);
                }

                return renderMaterial;
            }
            private set { UpdateMaterial(value); }
        }

        /// <summary>
        ///     The target Renderer Trigger
        /// </summary>
        public virtual EffectRenderer Target { get; set; }

        /// The
        /// <see cref="SkEffectCamera" />
        /// that this Renderer uses.
        public SkEffectCamera EffectCamera {
            get {
                if (!_effectCamera) {
                    if (!RenderMaterial)
                        return null;
                    InitCamera();
                }

                return _effectCamera;
            }
            set { _effectCamera = value; }
        }

        /// <summary>
        ///     Origin of the Renderer, facing inward
        /// </summary>
        public Transform Origin {
            get {
                if (!_origin)
                    _origin = Root.Find("Source");
                return _origin;
            }
        }

        /// <summary>
        ///     The transform attached to the target Renderer, for ref purposes. facing outward. Verbose for clarity reasons.
        /// </summary>
        public Transform ArrivalTarget {
            get {
                if (!_arrivalTarget) {
                    if (Target == null) return null;
                    _arrivalTarget = Target.Root.Find("Target");
                }

                return _arrivalTarget;
            }
        }

        public Transform Root { get { return transform.parent.parent; } }

        /// <summary>
        ///     Event raised when this Effect is disabled.
        /// </summary>
        public event Action OnDisabled;

        /// <summary>
        ///     Registered to callback on the associated <see cref="EffectRendererSettings" /> to update when
        ///     <see cref="EffectRendererSettings.OriginalMaterial" /> changes.
        /// </summary>
        /// <param name="originalMaterial">Material to make a copy of</param>
        private void UpdateMaterial(Material originalMaterial)
        {
            MeshRenderer.sharedMaterial = new Material(originalMaterial);
            if (SeamlessRecursionFix)
                SeamlessRecursionFix.GetComponent<Renderer>().sharedMaterial = MeshRenderer.sharedMaterial;
            EffectCamera.UpdateMaterial(originalMaterial);
        }

        /// <summary>
        ///     Registered to callback on the associated <see cref="EffectRendererSettings" /> to update when
        ///     <see cref="EffectRendererSettings.Mask" /> changes.
        /// </summary>
        /// <param name="mask">Mask to apply</param>
        private void UpdateMask(Texture2D mask)
        {
            if (RenderMaterial)
                RenderMaterial.SetTexture(Keywords.ShaderKeys.AlphaTexture, mask);
        }

        protected void InitCamera()
        {
            StartCoroutine(InitCameraRoutine());
        }

        private IEnumerator InitCameraRoutine()
        {
            while (!Target)
                yield return WaitCache.OneTenthS;
            _effectCamera = transform.parent.parent.GetComponentInChildren<SkEffectCamera>(true);
            _effectCamera.Initialize(Target.EffectCamera, RenderMaterial, MeshRenderer, Target.MeshRenderer,
                MeshFilter.sharedMesh, Origin, ArrivalTarget);
        }

        protected void SetupHeadCamera()
        {
            StartCoroutine(SetupHeadCameraRoutine());
        }

        protected IEnumerator SetupHeadCameraRoutine()
        {
            while (HeadCamera == null) yield return WaitCache.Frame;
            _NearClipVertsLocal = HeadCamera.EyeNearPlaneDimensions();
            _NearClipVertsGlobal = new Vector3[_NearClipVertsLocal.Length];
        }

        protected virtual void OnEnable()
        {
            //Register all delegates required for portal functionality
            Settings.OnChangeMaterial -= UpdateMaterial;
            Settings.OnChangeMask -= UpdateMask;
            Settings.OnChangeMaterial += UpdateMaterial;
            Settings.OnChangeMask += UpdateMask;

            SetupHeadCamera();

            //Set up things to keep the seamless recursion fix updated
            SeamlessRecursionRenderer.sharedMaterial = RenderMaterial;
            _SeamlessRecursionBlock = new MaterialPropertyBlock();
            UpdateMaterial(Settings.OriginalMaterial);
            UpdateMask(Settings.Mask);

            if (EffectCamera != null)
                InitCamera();
        }

        protected virtual void OnDisable()
        {
            try {
                Settings.OnChangeMaterial -= UpdateMaterial;
                Settings.OnChangeMask -= UpdateMask;
            }
            catch (NullReferenceException e) {
                SKLogger.LogWarning(e.Message, SKOrgType.PortalKit);
            }
        }

        protected virtual void OnDestroy()
        {
            OnDisable();
        }

        /// <summary>
        ///     All Renderer updates are done after everything else has updated
        /// </summary>
        protected virtual void LateUpdate()
        {
            SkEffectCamera.CurrentDepth = 0;
        }

        protected virtual void FrameUpdate() { }

        private void OnWillRenderObject()
        {
            if (!Target) return;
            var marker = CameraMarker.GetMarker(Camera.current);
            if (marker != null)
                if (marker.MeshRenderer == Target.MeshRenderer)
                    return;
            //Is the mesh renderer in the camera frustrum?
            if (MeshRenderer.isVisible) {
                RenderObjectBehavior();

                Target.FrameUpdate();

#if UNITY_EDITOR
                if (SceneView.lastActiveSceneView != null
                    && Camera.current == SceneView.lastActiveSceneView.camera)
                    return;
#endif
                TryRender(Camera.current, _NearClipVertsGlobal);
                if (Camera.current.gameObject.CompareTag(Keywords.Tags.MainCamera)) {
                    SKSGlobalRenderSettings.Inverted = SKSGlobalRenderSettings.Inverted;
                    SKSGlobalRenderSettings.UvFlip = SKSGlobalRenderSettings.UvFlip;
                }
            }

            UpdateEffects();
        }

        /// <summary>
        ///     Behavior invoked on render, if needed.
        /// </summary>
        protected abstract void RenderObjectBehavior();

        /// <summary>
        ///     Render a Renderer frame, assuming that the camera is in front of the Renderer and all conditions are met.
        /// </summary>
        /// <param name="camera">The camera rendering the Renderer</param>
        /// <param name="nearClipVerts">The vertices of the camera's near clip plane</param>
        private void TryRender(Camera camera, Vector3[] nearClipVerts)
        {
            if (!Target || !Settings.RenderingEnabled)
                return;
            //bool isVisible = false;
            var isVisible = false;
            //Check if the camera itself is behind the Renderer, even if the frustum isn't.

            if (!IsMirror) {
                if (!SKSGeneralUtils.IsBehind(camera.gameObject.transform.position, Origin.position, Origin.forward))
                    isVisible = true;
                else
                    foreach (var v in nearClipVerts)
                        if (!SKSGeneralUtils.IsBehind(v, Origin.position, Origin.forward)) {
                            isVisible = true;
                            break;
                        }
            }
            else {
                if (SKSGeneralUtils.IsBehind(camera.gameObject.transform.position, Origin.position, Origin.forward))
                    isVisible = true;
                else
                    foreach (var v in nearClipVerts)
                        if (SKSGeneralUtils.IsBehind(v, Origin.position, Origin.forward)) {
                            isVisible = true;
                            break;
                        }
            }


            if (isVisible || _CheeseActivated != -1)
                EffectCamera.RenderIntoMaterial(
                    camera, RenderMaterial,
                    MeshRenderer, Target.MeshRenderer,
                    RendererMesh, !NonObliqueOverride && _CheeseActivated == -1,
                    Settings.Is3D, IsMirror, Settings.IsLowQualityEffect, Settings.PixelPadding,
                    Settings.RecursionEnabled);

            MeshRenderer.GetPropertyBlock(_SeamlessRecursionBlock);
            SeamlessRecursionRenderer.SetPropertyBlock(_SeamlessRecursionBlock);
        }

        /// <summary>
        ///     Sets the z rendering order to make through-wall rendering seamless, as well as disabling masks while traversing
        ///     Renderer
        /// </summary>
        protected void UpdateEffects()
        {
            //MeshRenderer.GetPropertyBlock(_seamlessRecursionBlock);
            if (_CheeseActivated == SkEffectCamera.CurrentDepth) {
                //_seamlessRecursionBlock.SetFloat("_ZTest", (int)CompareFunction.Always);
                //MeshRenderer.SetPropertyBlock(_seamlessRecursionBlock);
                MeshRenderer.sharedMaterial.SetFloat(Keywords.ShaderKeys.ZTest, (int) CompareFunction.Always);
                MeshRenderer.sharedMaterial.SetFloat(Keywords.ShaderKeys.Mask, 0);
            }
            else {
                //_meshRenderer.material.SetFloat("_ZTest", (int)CompareFunction.Less);
                MeshRenderer.sharedMaterial.SetFloat(Keywords.ShaderKeys.Mask, 1);
            }
        }


        /// <summary>
        ///     Debugging naming for ~more understandable breakpoints~
        /// </summary>
        protected virtual void Start()
        {
#if SKS_DEV
            name = transform.parent.parent.parent.gameObject.name;
#endif
        }

        private void OnDrawGizmos()
        {
#if SKS_DEV
            var style = new GUIStyle(new GUIStyle {alignment = TextAnchor.MiddleCenter});
            style.normal.textColor = Color.white;
#if UNITY_EDITOR
            Handles.Label(transform.position, name, style);
#endif
#endif
        }
    }
}