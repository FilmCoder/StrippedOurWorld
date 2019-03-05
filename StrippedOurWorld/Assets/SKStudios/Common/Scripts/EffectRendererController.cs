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

#if SKS_PORTALS
using SKStudios.Portals;
#else
#if SKS_MIRRORS
#endif
#endif
using SKStudios.Common.Utils;
using SKStudios.Rendering;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SKStudios.Common {
    public class EffectRendererController : MonoBehaviour {
        [SerializeField] protected EffectRenderer _EffectScript;

        /// <summary>
        ///     ScriptableObject containing settings for the <see cref="EffectRenderer" /> that this controller will spawn.
        /// </summary>
        [HideInInspector] public EffectRendererSettings Settings;

        /// <summary>
        ///     Target controller for this <see cref="EffectRendererController" />
        /// </summary>
        public virtual EffectRendererController TargetController { get; set; }

        public EffectRenderer EffectScript {
            get {
                if (!_EffectScript)
                    _EffectScript = GetComponentInChildren<EffectRenderer>(true);
                return _EffectScript;
            }
        }

        /// <summary>
        ///     Draw the Portal Controller Gizmos.
        /// </summary>
        public void OnDrawGizmos()
        {
#if UNITY_EDITOR
            var style = new GUIStyle();
            style.normal.textColor = Color.red;

            if (!Settings) {
                Handles.Label(transform.position, "No Settings Set", style);
                return;
            }

            if (!TargetController) {
                Handles.Label(transform.position, "No Target Set", style);
                return;
            }

            if (!Settings.OriginalMaterial) {
                Handles.Label(transform.position, "No Material Set", style);
                return;
            }

            if (!Settings.RendererPrefab) {
                Handles.Label(transform.position, "No Prefab Set", style);
                return;
            }

            if (!Settings.Mask && !SKSGlobalRenderSettings.ShouldOverrideMask ||
                SKSGlobalRenderSettings.ShouldOverrideMask && !SKSGlobalRenderSettings.Mask) {
                Handles.Label(transform.position, "No Mask Set", style);
                return;
            }


            Gizmos.color = Color.clear;

            if (EffectScript) {
                Gizmos.matrix = EffectScript.transform.localToWorldMatrix;
                Gizmos.matrix *= Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, 0.1f));
                Gizmos.DrawMesh(PrimitiveHelper.GetPrimitiveMesh(PrimitiveType.Cube));
            }

#endif
        }
    }
}