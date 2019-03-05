using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class UiBlurController {
    public static Material MMaterial;

    private static Camera _mCam;

    private static readonly Dictionary<Camera, CommandBuffer> MCameras = new Dictionary<Camera, CommandBuffer>();

    private static void Cleanup()
    {
        foreach (var cam in MCameras)
            if (cam.Key)
                cam.Key.RemoveCommandBuffer(CameraEvent.AfterImageEffectsOpaque, cam.Value);
        MCameras.Clear();
        Object.DestroyImmediate(MMaterial);
    }

    public static void OnEnable()
    {
        Cleanup();
    }

    public static void OnDisable()
    {
        Cleanup();
    }

    // Whenever any camera will render us, add a command buffer to do the work on it
    public static void AddBlurToCamera(Camera cam)
    {
        if (!cam)
            return;

        CommandBuffer buf = null;
        // Did we already add the command buffer on this camera? Nothing to do then.
        if (MCameras.ContainsKey(cam))
            return;

        if (!MMaterial) {
            MMaterial = new Material(Shader.Find("Hidden/SeparableGlassBlur"));
            MMaterial.hideFlags = HideFlags.HideAndDontSave;
        }

        buf = new CommandBuffer();
        buf.name = "Grab screen and blur";
        MCameras[cam] = buf;

        // copy screen into temporary RT
        var screenCopyId = Shader.PropertyToID("_ScreenCopyTexture");
        buf.GetTemporaryRT(screenCopyId, -1, -1, 0, FilterMode.Bilinear);
        buf.Blit(BuiltinRenderTextureType.CurrentActive, screenCopyId);

        // get two smaller RTs
        var blurredId = Shader.PropertyToID("_Temp1");
        var blurredId2 = Shader.PropertyToID("_Temp2");
        buf.GetTemporaryRT(blurredId, -2, -2, 0, FilterMode.Bilinear);
        buf.GetTemporaryRT(blurredId2, -2, -2, 0, FilterMode.Bilinear);

        // downsample screen copy into smaller RT, release screen RT
        buf.Blit(screenCopyId, blurredId);
        buf.ReleaseTemporaryRT(screenCopyId);

        // horizontal blur
        buf.SetGlobalVector("offsets", new Vector4(4.0f / Screen.width, 0, 0, 0));
        buf.Blit(blurredId, blurredId2, MMaterial);
        // vertical blur
        buf.SetGlobalVector("offsets", new Vector4(0, 4.0f / Screen.height, 0, 0));
        buf.Blit(blurredId2, blurredId, MMaterial);
        // horizontal blur
        buf.SetGlobalVector("offsets", new Vector4(8.0f / Screen.width, 0, 0, 0));
        buf.Blit(blurredId, blurredId2, MMaterial);
        // vertical blur
        buf.SetGlobalVector("offsets", new Vector4(0, 8.0f / Screen.height, 0, 0));
        buf.Blit(blurredId2, blurredId, MMaterial);

        buf.SetGlobalTexture("_GrabBlurTexture", blurredId);

        cam.AddCommandBuffer(CameraEvent.AfterImageEffectsOpaque, buf);
    }
}