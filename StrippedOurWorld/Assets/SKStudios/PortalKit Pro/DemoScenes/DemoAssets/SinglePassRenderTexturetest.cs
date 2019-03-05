using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SKStudios.Common.Demos
{
    public class SinglePassRenderTexturetest : MonoBehaviour {
    private Camera cam;
    public RenderTexture renderTexture;

    void Start() {
        cam = GetComponent<Camera>();
        renderTexture = new RenderTexture(cam.pixelWidth * 2, cam.pixelHeight, 24, RenderTextureFormat.Default);
        cam.targetTexture = renderTexture;
    }
}
}
