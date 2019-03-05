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

using SKStudios.Common.Utils;
using SKStudios.ProtectedLibs.Rendering;
using UnityEngine;

/// <summary>
///     Placeholder for Portals. Currently unused.
/// </summary>
public class PortalPlaceholder : MonoBehaviour {
    private Camera _camera;
    private SKStudios.Portals.Portal _portal;

    public void Instantiate(SKStudios.Portals.Portal portal)
    {
        _portal = portal;

        var cameraParent = new GameObject();

        cameraParent.transform.parent = _portal.EffectCamera.RenderingCameraParent;
        cameraParent.transform.localPosition = Vector3.zero;
        cameraParent.transform.localRotation = Quaternion.identity;

        _camera = cameraParent.AddComponent<Camera>();
        _camera.cullingMask &= ~(1 << Keywords.Layers.CustomRenderer);
        _camera.cullingMask &= ~(1 << Keywords.Layers.CustomRendererPlaceholder);
        _camera.name = "Portal Placeholder Camera";
        _camera.enabled = false;
    }
}