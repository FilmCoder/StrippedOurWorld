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

using SKStudios.Common.Utils;
using UnityEngine;

namespace SKStudios.Common.Rendering {
    /// <summary>
    ///     A basic behavior that renders a <see cref="Cubemap" /> with the given resolution.
    /// </summary>
    [ExecuteInEditMode]
    public class CubemapController : MonoBehaviour {
        private Camera _cam;

        private Cubemap _map;

        /// <summary>
        ///     Should the cubemap be rendered as soon as the scene loads?
        /// </summary>
        public bool AutomaticallyCapture = false;

        /// <summary>
        ///     Layers to be excluded from the render of the <see cref="Cubemap" />. Defaults to ignoring
        ///     <see cref="Keywords.Layers.CustomRenderer" /> and <see cref="Keywords.Layers.CustomRendererPlaceholder" />
        /// </summary>
        public LayerMask LayerMask = 0;

        /// <summary>
        ///     The Resolution each side of the Cubemap will render at.
        /// </summary>
        public int Resolution = 64;

        /// <summary>
        ///     Camera that will render <see cref="Map" />
        /// </summary>
        private Camera Cam {
            get {
                if (!_cam) {
                    _cam = gameObject.AddComponent<Camera>();
                    _cam.enabled = false;
                    _cam.hideFlags = HideFlags.HideAndDontSave;
                }

                return _cam;
            }
        }

        /// <summary>
        ///     The <see cref="Cubemap" /> associated with this object. If the <see cref="Cubemap" /> has not yet
        ///     been rendered, accessing it will force it to render.
        /// </summary>
        public Cubemap Map {
            get {
                if (!_map)
                    RenderMap();
                return _map;
            }
        }

        /// <summary>
        ///     Set up the default layers
        /// </summary>
        private void Awake()
        {
            if (LayerMask == 0)
                LayerMask = ~0 &
                            ~(1 << Keywords.Layers.CustomRenderer) &
                            ~(1 << Keywords.Layers.CustomRendererPlaceholder);
        }

        private void Start()
        {
            if (AutomaticallyCapture)
                RenderMap();
        }

        /// <summary>
        ///     Free <see cref="Map" /> when destroyed
        /// </summary>
        private void OnDestroy()
        {
            if (_map)
                DestroyImmediate(_map, true);
            //if(_cam)
            //    DestroyImmediate(_cam, true);
        }

        /// <summary>
        ///     Render the <see cref="Cubemap" /> associated with this object, with each side having resolution
        ///     <see cref="Resolution" />.
        /// </summary>
        public void RenderMap()
        {
            _map = new Cubemap(Resolution, TextureFormat.ARGB32, false);
            Cam.cullingMask = LayerMask;
            Cam.nearClipPlane = 0.5f;
            Cam.RenderToCubemap(Map);
        }
    }
}