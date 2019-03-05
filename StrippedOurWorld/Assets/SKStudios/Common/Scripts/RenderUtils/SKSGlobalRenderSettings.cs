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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SKStudios.Common;
using SKStudios.Common.Utils;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Callbacks;
using UnityEditor;
using UnityEditorInternal;

#endif
#if SKS_VR
using UnityEngine.VR;
#endif

namespace SKStudios.Rendering {
    /// <summary>
    ///     Stores settings for Mirror that are used globally
    /// </summary>
    [Serializable]
    [ExecuteInEditMode]
    [CreateAssetMenu(menuName = "ScriptableObjects/SKGlobalRenderSettings")]
    public class SKSGlobalRenderSettings : ScriptableObject {
        [SerializeField] [HideInInspector] private bool _shouldOverrideMaskCached;
        [SerializeField] [HideInInspector] private Texture2D _maskCached;
        [SerializeField] [HideInInspector] private bool _physicsPassthroughCached;
        [SerializeField] [HideInInspector] private int _importedCached;
        [SerializeField] [HideInInspector] private int _recursionNumberCached;
        [SerializeField] [HideInInspector] private int _placeholderResolutionCached = 64;
        [SerializeField] [HideInInspector] private int _previewResolutionCached = 64;
        [SerializeField] [HideInInspector] private bool _invertedCached;
        [SerializeField] [HideInInspector] private bool _uvFlipCached;
        [SerializeField] [HideInInspector] private bool _clippingCached;
        [SerializeField] [HideInInspector] private bool _physStyleBCached;
        [SerializeField] [HideInInspector] private bool _minimizedCached;
        [SerializeField] [HideInInspector] private bool _closedCached;
        [SerializeField] [HideInInspector] private bool _mirrorVisualizationCached;
        [SerializeField] [HideInInspector] private bool _mirrorGizmosCached;
        [SerializeField] [HideInInspector] private bool _mirrorPreviewCached;
        [SerializeField] [HideInInspector] private bool _nonscaledMirrorsCached;
        [SerializeField] [HideInInspector] private bool _adaptiveQualityCached;
        [SerializeField] [HideInInspector] private bool _aggressiveRecursionOptimizationCached;
        [SerializeField] [HideInInspector] private bool _customSkyboxCached;
        [SerializeField] [HideInInspector] private List<int> _ignoredNotifications = new List<int>();
#if SKS_VR
        [SerializeField] [HideInInspector] public bool SinglePassStereoCached;
#endif


        private static SKSGlobalRenderSettings _instance;

        private static void RegisterUndo<T>(T newVal, T oldVal)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                if (!EqualityComparer<T>.Default.Equals(newVal, oldVal)) {
                    EditorUtility.SetDirty(Instance);
                    Undo.RegisterCompleteObjectUndo(Instance, "Global Settings");
                }
#endif
        }

        /// <summary>
        ///     Get the singleton instance of this object
        /// </summary>
        public static SKSGlobalRenderSettings Instance {
            get {
                var loaded = false;
                if (!_instance) {
#if UNITY_EDITOR
                    AssetDatabase.Refresh();
#endif
                    loaded = true;
                    _instance = Resources.Load<SKSGlobalRenderSettings>("SK Global Render Settings");

                    if (_instance)
                        _instance.Initialize();
                }

                if (!_instance) loaded = false;

                if (loaded) {
#if UNITY_EDITOR
                    EditorApplication.playmodeStateChanged -= RecompileCleanup;
                    EditorApplication.playmodeStateChanged += RecompileCleanup;
#endif
                }

                return _instance;
            }
        }

        /// <summary>
        ///     Is Physic style B enabled?
        /// </summary>
        public static bool PhysStyleB {
            get { return Instance._physStyleBCached; }
            set {
#if UNITY_EDITOR
                RegisterUndo(Instance._physStyleBCached, value);
#endif
                Instance._physStyleBCached = value;
            }
        }


        /// <summary>
        ///     Should the mask for all Mirrors be overridden?
        /// </summary>
        public static bool ShouldOverrideMask {
            get { return Instance._shouldOverrideMaskCached; }
            set {
#if UNITY_EDITOR
                RegisterUndo(Instance._shouldOverrideMaskCached, value);
#endif
                Instance._shouldOverrideMaskCached = value;
            }
        }

        /// <summary>
        ///     Texture2D with which the masks on all Mirrors are overwritten if set
        /// </summary>
        public static Texture2D Mask {
            get { return Instance._maskCached; }
            set {
#if UNITY_EDITOR
                RegisterUndo(Instance._maskCached, value);
#endif
                Instance._maskCached = value;
            }
        }

        /// <summary>
        ///     Is the UV inverted? Changing this value changes the global "_InvertOverride" value.
        /// </summary>
        public static bool Inverted {
            get { return Instance._invertedCached; }
            set {
#if UNITY_EDITOR
                RegisterUndo(Instance._invertedCached, value);
#endif
                Shader.SetGlobalFloat(Keywords.ShaderKeys.InvertOverride, value ? 1 : 0);
                Instance._invertedCached = value;
            }
        }

        /// <summary>
        ///     Is the UV flipped? Changing this value changes the global "_YFlipOverride" value.
        /// </summary>
        public static bool UvFlip {
            get { return Instance._uvFlipCached; }
            set {
#if UNITY_EDITOR
                RegisterUndo(Instance._uvFlipCached, value);
#endif
                Shader.SetGlobalFloat(Keywords.ShaderKeys.YFlipOverride, value ? 1 : 0);
                Instance._uvFlipCached = value;
            }
        }

        /// <summary>
        ///     Is Object clipping enabled to make objects disappear as they enter applicable products?
        /// </summary>
        public static bool Clipping {
            get { return Instance._clippingCached; }
            set {
#if UNITY_EDITOR
                RegisterUndo(Instance._clippingCached, value);
#endif
                //Keywords.ShaderKeys.ClipOverride
                Shader.SetGlobalFloat(Keywords.ShaderKeys.ClipOverride, value ? 0 : 1);
                Instance._clippingCached = value;
            }
        }

        /// <summary>
        ///     Are passthrough physics simulated in applicable products?
        /// </summary>
        public static bool PhysicsPassthrough {
            get { return Instance._physicsPassthroughCached; }
            set {
#if UNITY_EDITOR
                RegisterUndo(Instance._physicsPassthroughCached, value);
#endif

                Instance._physicsPassthroughCached = value;
            }
        }

        /// <summary>
        ///     Has the asset been imported?
        /// </summary>
        public static int Imported {
            get { return Instance._importedCached; }
            set {
#if UNITY_EDITOR
                RegisterUndo(Instance._importedCached, value);
#endif
                Instance._importedCached = value;
            }
        }

        /// <summary>
        ///     how many times should <see cref="EffectRenderer" /> recurse while rendering?
        /// </summary>
        public static int RecursionNumber {
            get { return Instance._recursionNumberCached; }
            set {
#if UNITY_EDITOR
                RegisterUndo(Instance._recursionNumberCached, value);
#endif
                Instance._recursionNumberCached = value;
            }
        }

        /// <summary>
        ///     Is the sceneview menu minimized?
        /// </summary>
        public static bool Minimized {
            get { return Instance._minimizedCached; }
            set {
#if UNITY_EDITOR
                RegisterUndo(Instance._minimizedCached, value);
#endif
                Instance._minimizedCached = value;
            }
        }

        /// <summary>
        ///     Should differently-sized renderers (and portals) scale?
        /// </summary>
        public static bool NonScaledRenderers {
            get { return Instance._nonscaledMirrorsCached; }
            set {
#if UNITY_EDITOR
                RegisterUndo(Instance._nonscaledMirrorsCached, value);
#endif
                Instance._nonscaledMirrorsCached = value;
            }
        }

        /// <summary>
        ///     Is Aggressive Recursion optimization enabled?
        /// </summary>
        public static bool AggressiveRecursionOptimization {
            get { return Instance._aggressiveRecursionOptimizationCached; }
            set {
#if UNITY_EDITOR
                RegisterUndo(Instance._aggressiveRecursionOptimizationCached, value);
#endif
                Instance._aggressiveRecursionOptimizationCached = value;
            }
        }

        /// <summary>
        ///     Is the MirrorMenu closed?
        /// </summary>
        public static bool MenuClosed {
            get { return Instance._closedCached; }
            set {
#if UNITY_EDITOR
                RegisterUndo(Instance._closedCached, value);
#endif
                Instance._closedCached = value;
            }
        }

        /// <summary>
        ///     Enable visualization of custom renderer connections
        /// </summary>
        public static bool Visualization {
            get { return Instance._mirrorVisualizationCached; }
            set {
#if UNITY_EDITOR
                RegisterUndo(Instance._mirrorVisualizationCached, value);
#endif
                Instance._mirrorVisualizationCached = value;
            }
        }


        /// <summary>
        ///     Enable Custom Skybox rendering, to fix broken skybox issues induced by optimizations.
        /// </summary>
        public static bool CustomSkybox {
            get { return Instance._customSkyboxCached; }
            set {
#if UNITY_EDITOR
                RegisterUndo(Instance._customSkyboxCached, value);
#endif
                Instance._customSkyboxCached = value;
            }
        }

        /// <summary>
        ///     Disable showing Mirror gizmos even while selected
        /// </summary>
        public static bool Gizmos {
            get { return Instance._mirrorGizmosCached; }
            set {
#if UNITY_EDITOR
                RegisterUndo(Instance._mirrorGizmosCached, value);
#endif
                Instance._mirrorGizmosCached = value;
            }
        }


        /// <summary>
        ///     Disable showing Mirror gizmos even while selected
        /// </summary>
        public static bool Preview {
            get { return Instance._mirrorPreviewCached; }
            set {
#if UNITY_EDITOR
                RegisterUndo(Instance._mirrorPreviewCached, value);
#endif
                Instance._mirrorPreviewCached = value;
            }
        }


        /// <summary>
        ///     Is the MirrorMenu minimized?
        /// </summary>
        public static bool AdaptiveQuality {
            get { return Instance._adaptiveQualityCached; }
            set {
#if UNITY_EDITOR
                RegisterUndo(Instance._adaptiveQualityCached, value);
#endif
                Instance._adaptiveQualityCached = value;
            }
        }

        /// <summary>
        ///     Resolution of placeholder <see cref="Cubemap" />
        /// </summary>
        public static int PlaceholderResolution {
            get { return Instance._placeholderResolutionCached; }
            set {
                var v = (uint) value;
                v--;
                v |= v >> 1;
                v |= v >> 2;
                v |= v >> 4;
                v |= v >> 8;
                v |= v >> 16;
                v++;
                value = (int) v;
#if UNITY_EDITOR
                RegisterUndo(Instance._placeholderResolutionCached, value);
#endif
                Instance._placeholderResolutionCached = value;
            }
        }


        /// <summary>
        ///     Resolution of preview <see cref="Cubemap" /> in th editor
        /// </summary>
        public static int PreviewResolution {
            get { return Instance._previewResolutionCached; }
            set {
                var v = (uint) value;
                v--;
                v |= v >> 1;
                v |= v >> 2;
                v |= v >> 4;
                v |= v >> 8;
                v |= v >> 16;
                v++;
                value = (int) v;
#if UNITY_EDITOR
                RegisterUndo(Instance._previewResolutionCached, value);
#endif
                Instance._previewResolutionCached = value;
            }
        }


        /// <summary>
        ///     Is the MirrorMenu minimized?
        /// </summary>
        public static List<int> IgnoredNotifications {
            get { return Instance._ignoredNotifications; }
            set {
#if UNITY_EDITOR
                RegisterUndo(Instance._ignoredNotifications, value);
#endif
                Instance._ignoredNotifications = value;
            }
        }


        public static bool LightPassthrough;
#if SKS_VR
/// <summary>
/// Is SinglePassStereo rendering enabled?
/// </summary> 
        public static bool SinglePassStereo{
            get { return Instance.SinglePassStereoCached; }
            set { 
#if UNITY_EDITOR
     RegisterUndo(Instance.SinglePassStereoCached, value);
#endif
    Instance.SinglePassStereoCached = value; 
}
        }
#endif
        /// <summary>
        ///     Initialize
        /// </summary>
        public void OnEnable()
        {
            Initialize();
        }

        // Use this for initialization
        private void Initialize()
        {
#if SKS_VR
            SinglePassStereo = SinglePassStereoCached;
            //SinglePassStereo = SinglePassStereoCached;
            //Debug.Log("Single Pass Stereo Mode: " + SinglePassStereo);
           // Shader.EnableKeyword("VR");   
              Shader.SetGlobalFloat("_VR", 1);
#else
            Shader.SetGlobalFloat("_VR", 0);
            //Shader.DisableKeyword("VR");
#endif
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("SKSRenderSettings:{");
            builder.Append(_shouldOverrideMaskCached ? 0 : 1);
            builder.Append(_maskCached ? 0 : 1);
            builder.Append(_physicsPassthroughCached ? 0 : 1);
            builder.Append(_invertedCached ? 0 : 1);
            builder.Append(_uvFlipCached ? 0 : 1);
            builder.Append(_clippingCached ? 0 : 1);
            builder.Append(_physStyleBCached ? 0 : 1);
            builder.Append(_minimizedCached ? 0 : 1);
            builder.Append(_closedCached ? 0 : 1);
            builder.Append(_mirrorVisualizationCached ? 0 : 1);
            builder.Append(_mirrorGizmosCached ? 0 : 1);
            builder.Append(_mirrorPreviewCached ? 0 : 1);
            builder.Append(_nonscaledMirrorsCached ? 0 : 1);
            builder.Append(_adaptiveQualityCached ? 0 : 1);
            builder.Append(_aggressiveRecursionOptimizationCached ? 0 : 1);
            builder.Append(_customSkyboxCached ? 0 : 1).Append('|');
            builder.Append(_importedCached).Append('|');
            builder.Append(_recursionNumberCached);
            builder.Append("}");
            return builder.ToString();
        }

#if UNITY_EDITOR
        [DidReloadScripts]
#endif
        private static void RecompileCleanup()
        {
#if UNITY_EDITOR
            if (Application.isPlaying) return;
            if (!InternalEditorUtility.tags.Contains("SKSEditorTemp")) return;
            var tempObjects = GameObject.FindGameObjectsWithTag("SKSEditorTemp");
            for (var i = 0; i < tempObjects.Length; i++) DestroyImmediate(tempObjects[i], true);

#endif
        }
    }
}