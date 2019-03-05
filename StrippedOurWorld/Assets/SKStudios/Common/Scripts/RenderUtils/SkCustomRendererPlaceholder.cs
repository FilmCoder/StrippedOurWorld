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
using SKStudios.Rendering;
using UnityEngine;

namespace SKStudios.Common.Rendering {
    /// <summary>
    ///     A behavior for generating good looking placeholders for <see cref="EffectRenderer" />s.
    ///     Automatically triggers rendering of a <see cref="CubemapController" /> and uses the result to estimate
    ///     color.
    /// </summary>
    public class SkCustomRendererPlaceholder : MonoBehaviour {
        private Material _material;

        private void Start()
        {
            Render();
        }

        private GameObject _cubeMapControllerGameObject;
        private CubemapController _cubeMapController;
        private CubemapController CubeMapController {
            get {
                if (_cubeMapControllerGameObject == null)
                {
                    _cubeMapControllerGameObject = new GameObject();
                    
                    _cubeMapControllerGameObject.transform.SetParent(transform, false);
                    _cubeMapControllerGameObject.transform.position = transform.position - (transform.forward.normalized * 2.5f);
                    _cubeMapControllerGameObject.tag = Keywords.Tags.SKEditorTemp;
                }
                if (_cubeMapController == null)
                {
                    _cubeMapController = _cubeMapControllerGameObject.AddComponent<CubemapController>();
                    _cubeMapController.Resolution = SKSGlobalRenderSettings.PlaceholderResolution;
                    _cubeMapController.RenderMap();
                }

                return _cubeMapController;
            }
        }

        /// <summary>
        ///     Render the <see cref="Cubemap" /> associated with this placeholder and apply it.
        /// </summary>
        public void Render()
        {

            CubeMapController.Resolution = SKSGlobalRenderSettings.PlaceholderResolution;
            
            var rend = GetComponent<Renderer>();
            //Intentionally create copy
            _material = rend.material;

            var controllerBlock = new MaterialPropertyBlock();

            //rend.GetPropertyBlock(controllerBlock);
            rend.sharedMaterial.SetTexture("_Cube", CubeMapController.Map);
            rend.sharedMaterial.SetVector("_EnvBoxPos", CubeMapController.transform.position);
            rend.sharedMaterial.SetVector("_EnvBoxSize", new Vector4(5, 5, 5, 1));
            //rend.SetPropertyBlock(controllerBlock);
        }
    }
}