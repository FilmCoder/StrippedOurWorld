using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using SKStudios.Common;
using SKStudios.Common.Editor;
using SKStudios.Common.Utils;
using SKStudios.Portals.Editor;
using SKStudios.Rendering;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
///     base Editor for <see cref="EffectRenderer" /> components
/// </summary>
public abstract class EffectRendererControllerEditor : MeshFilterPreview
{
    private const int PortalMatSlot = 2;

    protected static float _BumperSize = 3;
    protected static int _IndentSize = 1;
    private readonly AnimBool _globalSettingOpenAnim;
    private readonly AnimBool _instanceOpenAnim;
    protected readonly List<string> TabOptions = new List<string>();

    private MeshFilter _editorPreviewFilter;

    private Camera _editorWindowPreviewCamera;

    private MeshRenderer _editorWindowPreviewRenderer;

    [NonSerialized] private Editor _matEditor;

    private PreviewRenderUtility _previewRenderUtility;

    private int _tab;

    private EffectRendererController _targetEffectRenderer;

    protected EffectRendererControllerEditor()
    {
        TabOptions.Add("Instance Settings");
        TabOptions.Add("Global Settings");
        _instanceOpenAnim = new AnimBool(true, Repaint);
        _globalSettingOpenAnim = new AnimBool(false, Repaint);
    }

    protected abstract Material TargetMaterial { get; set; }

    protected abstract GameObject Target { get; }
    protected abstract bool MirroredPreview { get; }
    protected abstract string AssetName { get; }

    private Camera EditorWindowPreviewCamera {
        get {
            if (_editorWindowPreviewCamera == null)
            {
                if (!TargetMeshRenderer) return null;
                var cameraObj = EditorUtility.CreateGameObjectWithHideFlags("Editor Preview Camera",
                    HideFlags.HideAndDontSave | HideFlags.NotEditable);
                cameraObj.transform.SetParent(TargetMeshRenderer.transform);
                _editorWindowPreviewCamera = cameraObj.AddComponent<Camera>();
                _editorWindowPreviewCamera.enabled = false;
                _editorWindowPreviewCamera.cullingMask = 0;
                var previewBox = EditorWindowPreviewCamera.gameObject.AddComponent<Skybox>();
                previewBox.material = Resources.Load<Material>("UI/Materials/PreviewSkybox/PreviewSkybox");
            }

            return _editorWindowPreviewCamera;
        }
    }

    private MeshRenderer EditorWindowWindowPreviewRenderer {
        get {
            if (!_editorWindowPreviewRenderer)
            {
                var editorPreviewObj = EditorUtility.CreateGameObjectWithHideFlags("Preview Object",
                    HideFlags.HideAndDontSave | HideFlags.NotEditable);
                editorPreviewObj.tag = Keywords.Tags.SKEditorTemp;
                if (Target.activeInHierarchy)
                    editorPreviewObj.transform.SetParent(Target.transform);
                editorPreviewObj.transform.localPosition = Vector3.zero;

                _editorWindowPreviewRenderer = editorPreviewObj.AddComponent<MeshRenderer>();

                _editorWindowPreviewRenderer.sharedMaterials = new Material[2];
                var materialArray = new Material[3];
                materialArray[1] = Resources.Load<Material>("UI/Materials/Background");
                materialArray[0] = Resources.Load<Material>("UI/Materials/Backdrop");
                _editorWindowPreviewRenderer.sharedMaterials = materialArray;
                UpdateEditorPreviewMat(TargetMaterial);
            }

            return _editorWindowPreviewRenderer;
        }
    }

    private EffectRendererController TargetEffectRenderer {
        get {
            if (!_targetEffectRenderer)
                _targetEffectRenderer = (EffectRendererController)target;
            return _targetEffectRenderer;
        }
    }

    private MeshFilter EditorPreviewFilter {
        get {
            if (!_editorPreviewFilter)
            {
                _editorPreviewFilter = EditorWindowWindowPreviewRenderer.gameObject.AddComponent<MeshFilter>();
                _editorPreviewFilter.mesh = Resources.Load<Mesh>("UI/RendererPreview");
            }

            return _editorPreviewFilter;
        }
    }


    public override MeshFilter TargetMeshFilter { get { return EditorPreviewFilter; } set { } }

    public override MeshRenderer TargetMeshRenderer { get { return EditorWindowWindowPreviewRenderer; } set { } }

    protected abstract void DrawInstanceUi();
    protected virtual void DrawCustomGlobalUi() { }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        {
            EditorWindowWindowPreviewRenderer.enabled = false;
            _tab = GUILayout.Toolbar(_tab, TabOptions.ToArray());


            _instanceOpenAnim.target = _tab == 0;
            _globalSettingOpenAnim.target = !_instanceOpenAnim.target;
            if (EditorGUILayout.BeginFadeGroup(_instanceOpenAnim.faded))
            {
                DrawSettingsUi();
                DrawInstanceUi();
            }

            EditorGUILayout.EndFadeGroup();

            if (EditorGUILayout.BeginFadeGroup(_globalSettingOpenAnim.faded))
                DrawGlobalUi();
            EditorGUILayout.EndFadeGroup();

            if (!Application.isPlaying)
                try
                {
                    if (TargetMaterial)
                        if (_matEditor == null)
                            _matEditor = CreateEditor(TargetMaterial);


                    _matEditor.DrawHeader();
                    _matEditor.OnInspectorGUI();
                    DestroyImmediate(_matEditor);
                }
                catch { }
        }
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(TargetEffectRenderer);
            EditorUtility.SetDirty(TargetEffectRenderer.Settings);
            EditorUtility.SetDirty(SKSGlobalRenderSettings.Instance);
        }
    }

    protected void UpdateEditorPreviewMat(Material mat)
    {
        var materialArray = EditorWindowWindowPreviewRenderer.sharedMaterials;
        if (mat)
        {
            materialArray[PortalMatSlot] = mat;
            materialArray[PortalMatSlot].SetFloat(Keywords.ShaderKeys.Mask, 1);
            materialArray[PortalMatSlot].SetFloat(Keywords.ShaderKeys.ZTest, (float)CompareFunction.LessEqual);
        }

        EditorWindowWindowPreviewRenderer.sharedMaterials = materialArray;
    }


    public override bool HasPreviewGUI()
    {
        return HasPreviewGUI_s();
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        if (Application.isPlaying) return;

        TargetMeshRenderer.gameObject.SetActive(true);
        TargetMeshRenderer.gameObject.transform.localPosition = Vector3.zero;

        //Update the preview cam and the cam to render with
        var previewTransform = MoveCamera();
        if (previewTransform == null) return;
        EditorWindowPreviewCamera.transform.rotation = previewTransform.rotation;
        EditorWindowPreviewCamera.transform.position = previewTransform.position;
        if (MirroredPreview)
        {
            EditorWindowPreviewCamera.transform.rotation =
                MirrorTransformUtils.MirrorRotate(EditorWindowPreviewCamera.transform.rotation);

            MirrorTransformUtils.MirrorMoveTransform(EditorWindowPreviewCamera.transform);
        }

        EditorWindowWindowPreviewRenderer.enabled = true;
        RenderTexture rt = null;
        try
        {
            if (r.width > 0 && (int)r.height > 0)
            {
                rt = RenderTexture.GetTemporary((int)r.width, (int)r.height, 16);
                EditorWindowPreviewCamera.targetTexture = rt;
                TargetMaterial.SetTexture(Keywords.ShaderKeys.LeftEye, rt);
                TargetMaterial.SetVector(Keywords.ShaderKeys.LeftPos, new Vector4(0, 0, 1, 1));
                TargetMaterial.SetTexture(Keywords.ShaderKeys.RightEye, rt);
                TargetMaterial.SetVector(Keywords.ShaderKeys.RightPos, new Vector4(0, 0, 1, 1));
                EditorWindowPreviewCamera.Render();
                //EditorWindowPreviewCamera.Render();
                OnPreviewGUI_s(r, background);
            }
        }
        catch
        {
            //Unity silliness again
        }

        TargetMeshRenderer.gameObject.SetActive(false);
        if (rt != null)
            RenderTexture.ReleaseTemporary(rt);
        EditorWindowWindowPreviewRenderer.enabled = false;
    }

    private void DrawSettingsUi()
    {
        GUILayout.Label("Effect Renderer Settings", EditorStyles.boldLabel);
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            {
                EditorGUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                {

                    EditorGUI.BeginChangeCheck();
                    TargetEffectRenderer.Settings =
                        (EffectRendererSettings)EditorGUILayout.ObjectField(
                            new GUIContent("Settings ScriptableObject",
                                "The Settings ScriptableObject for this EffectRenderer"),
                            TargetEffectRenderer.Settings, typeof(EffectRendererSettings), false, null);
                    var newSettings = false;
                    if (GUILayout.Button("New Settings"))
                    {
                        TargetEffectRenderer.Settings = EffectRendererSettings.CreateNewEffectRendererSettings(
                            TargetEffectRenderer.name,
                            Resources.Load<Material>("Materials/Visible effects/Portal Materials/PortalMat"),
                            Resources.Load<GameObject>("Prefabs/Internal/RectPortal")
                        );
                        newSettings = true;
                    }

                    if (EditorGUI.EndChangeCheck() || newSettings)
                        foreach (var o in targets)
                        {
                            var p = o as EffectRendererController;
                            if (p != null) p.Settings = TargetEffectRenderer.Settings;
                        }
                }
                GUILayout.EndHorizontal();

                try
                {
                    Undo.RecordObjects(targets, "Mirror Controller Editor Changes");

                    EditorGUI.BeginChangeCheck();
                    TargetEffectRenderer.Settings.RendererPrefab =
                        (GameObject)EditorGUILayout.ObjectField(
                            new GUIContent("EffectRenderer Prefab",
                                "The Prefab to use for when the EffectRenderer is spawned"),
                            TargetEffectRenderer.Settings.RendererPrefab, typeof(GameObject), false, null);
                    if (EditorGUI.EndChangeCheck())
                    {
                        foreach (var o in targets)
                        {
                            var p = o as EffectRendererController;
                            if (p != null)
                                p.Settings.RendererPrefab = TargetEffectRenderer.Settings.RendererPrefab;
                        }

                        EditorUtility.SetDirty(TargetEffectRenderer.Settings);
                    }


                    if (SKSGlobalRenderSettings.ShouldOverrideMask)
                        EditorGUILayout.HelpBox("Your Global EffectRenderer Settings are currently overriding the mask",
                            MessageType.Warning);

                    EditorGUI.BeginChangeCheck();
                    TargetEffectRenderer.Settings.Mask =
                        (Texture2D)EditorGUILayout.ObjectField(
                            new GUIContent("EffectRenderer Mask", "The transparency mask to use on the EffectRenderer"),
                            TargetEffectRenderer.Settings.Mask, typeof(Texture2D), false,
                            GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));
                    if (EditorGUI.EndChangeCheck())
                    {
                        foreach (var o in targets)
                        {
                            var p = o as EffectRendererController;
                            if (p != null)
                                p.Settings.Mask = TargetEffectRenderer.Settings.Mask;
                        }

                        EditorUtility.SetDirty(TargetEffectRenderer.Settings);
                    }


                    EditorGUI.BeginChangeCheck();
                    TargetEffectRenderer.Settings.OriginalMaterial =
                        (Material)EditorGUILayout.ObjectField(
                            new GUIContent("EffectRenderer Material", "The material to use for the EffectRenderer"),
                            TargetEffectRenderer.Settings.OriginalMaterial, typeof(Material), false, null);
                    if (EditorGUI.EndChangeCheck())
                    {
                        foreach (var o in targets)
                        {
                            var p = o as EffectRendererController;
                            if (p != null)
                                p.Settings.OriginalMaterial = TargetEffectRenderer.Settings.OriginalMaterial;
                        }

                        EditorUtility.SetDirty(TargetEffectRenderer.Settings);
                    }

                    EditorGUI.BeginChangeCheck();
                    TargetEffectRenderer.Settings.PixelPadding = EditorGUILayout.IntField(new GUIContent(
                            "Pixel Padding",
                            "The number of extra pixels to draw on the top and bottom. Useful for distortion materials."),
                        TargetEffectRenderer.Settings.PixelPadding);
                    if (EditorGUI.EndChangeCheck())
                    {
                        foreach (var o in targets)
                        {
                            var p = (EffectRendererController)o;
                            if (p != null)
                                p.Settings.PixelPadding = TargetEffectRenderer.Settings.PixelPadding;
                        }

                        EditorUtility.SetDirty(TargetEffectRenderer.Settings);
                    }


                    EditorGUI.BeginChangeCheck();
                    TargetEffectRenderer.Settings.RenderingEnabled = EditorGUILayout.Toggle(new GUIContent(
                        "Rendering Enabled",
                        "Is this EffectRenderer going to spawn enabled?"), TargetEffectRenderer.Settings.RenderingEnabled);
                    if (EditorGUI.EndChangeCheck())
                    {
                        foreach (var o in targets)
                        {
                            var p = o as EffectRendererController;
                            if (p != null)
                                p.Settings.RenderingEnabled = TargetEffectRenderer.Settings.RenderingEnabled;
                        }

                        EditorUtility.SetDirty(TargetEffectRenderer.Settings);
                    }


                    EditorGUI.BeginChangeCheck();
                    TargetEffectRenderer.Settings.RecursionEnabled = EditorGUILayout.Toggle(new GUIContent(
                            "Recursion Enabled",
                            "Is this EffectRenderer going to use recursive rendering?"),
                        TargetEffectRenderer.Settings.RecursionEnabled);
                    if (EditorGUI.EndChangeCheck())
                    {
                        foreach (var o in targets)
                        {
                            var p = o as EffectRendererController;
                            if (p != null)
                                p.Settings.RecursionEnabled = TargetEffectRenderer.Settings.RecursionEnabled;
                        }

                        EditorUtility.SetDirty(TargetEffectRenderer.Settings);
                    }


                    EditorGUI.BeginChangeCheck();
                    TargetEffectRenderer.Settings.IsLowQualityEffect =
                        EditorGUILayout.Toggle(
                            new GUIContent("Low-detail effect",
                                "Decimates resolution for non-clear effects where resolution is less important"),
                            TargetEffectRenderer.Settings.IsLowQualityEffect);
                    if (EditorGUI.EndChangeCheck())
                    {
                        foreach (var o in targets)
                        {
                            var p = o as EffectRendererController;
                            if (p != null)
                                p.Settings.IsLowQualityEffect = TargetEffectRenderer.Settings.IsLowQualityEffect;
                        }

                        EditorUtility.SetDirty(TargetEffectRenderer.Settings);
                    }
                }
                catch
                {
                    //Catch un-set fields
                }
            }
            EditorGUILayout.EndVertical();
        }
    }

    private SKSGlobalRenderSettingsEditor _globalSettingsEditor;

    private SKSGlobalRenderSettingsEditor GlobalSettingsEditor {
        get {
            if(_globalSettingsEditor == null)
                _globalSettingsEditor = new SKSGlobalRenderSettingsEditor(Repaint);
            return _globalSettingsEditor;
        }
    }

    //protected abstract MonoBehaviour TargetController { get; }
    private void DrawGlobalUi()
    {
        DrawCustomGlobalUi();

        GlobalSettingsEditor.DrawAllGui(AssetName);

#if false
        GUILayout.Label("SKS Global settings", EditorStyles.boldLabel);
        EditorGUI.indentLevel = EditorGUI.indentLevel + _IndentSize;

        GUILayout.Space(_BumperSize);
        if (_imageFoldout = EditorGUILayout.Foldout(_imageFoldout, "Image Settings", EditorStyles.foldout))
        {
            EditorGUI.indentLevel = EditorGUI.indentLevel + _IndentSize;

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(EditorGUI.indentLevel * 10);
#if SKS_VR
                GUILayout.Label("Single Pass Stereo Rendering: " + SKSGlobalRenderSettings.SinglePassStereo);
#endif
            }
            GUILayout.EndHorizontal();


            GUI.enabled = !Application.isPlaying;


#if SKS_VR
                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUI.indentLevel * 10);
                GUILayout.Label("Recursion in VR is very expensive. 3 is the typically acceptable max (prefer 0 if possible)");
                GUILayout.EndHorizontal();
#endif

            SKSGlobalRenderSettings.RecursionNumber = EditorGUILayout.IntSlider(
                new GUIContent("Recursion Number",
                    "The number of times that EffectRenderers will draw through each other."),
                SKSGlobalRenderSettings.RecursionNumber, 0, 10);


            if (SKSGlobalRenderSettings.RecursionNumber > 1)
                EditorGUILayout.HelpBox(
                    "Please be aware that recursion can get very expensive very quickly." +
                    " Consider making this scale with the Quality setting of your game.",
                    MessageType.Warning);


            GUI.enabled = true;


            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(EditorGUI.indentLevel * 10);
                SKSGlobalRenderSettings.AggressiveRecursionOptimization = GUILayout.Toggle(
                    SKSGlobalRenderSettings.AggressiveRecursionOptimization,
                    new GUIContent("Enable Aggressive Optimization for Recursion",
                        "Aggressive optimization will halt recursive rendering immediately if the " +
                        "source EffectRenderer cannot raycast to the EffectRenderers it is trying to render. " +
                        "Without Occlusion Culling (due to lack of Unity Support), this is a lifesaver for " +
                        "large scenes."));
            }
            GUILayout.EndHorizontal();

            if (SKSGlobalRenderSettings.AggressiveRecursionOptimization)
                EditorGUILayout.HelpBox(
                    "Enabling this option can save some serious performance, " +
                    "but it is possible for visual bugs to arise due to portals being partially inside walls. " +
                    "If you are seeing black EffectRenderers while recursing, try turning this option off " +
                    "and see if it helps. If it does, then please make sure that your EffectRenderers are not" +
                    "inside walls.",
                    MessageType.Warning);

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(EditorGUI.indentLevel * 10);
                SKSGlobalRenderSettings.AdaptiveQuality = GUILayout.Toggle(SKSGlobalRenderSettings.AdaptiveQuality,
                    new GUIContent("Enable Adaptive Quality Optimization for Recursion",
                        "Adaptive quality rapidly degrades the quality of recursively " +
                        "rendered EffectRenderers. This is usually desirable."));
            }
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(EditorGUI.indentLevel * 10);
                SKSGlobalRenderSettings.CustomSkybox = GUILayout.Toggle(SKSGlobalRenderSettings.CustomSkybox,
                    new GUIContent("Enable Skybox Override",
                        "Enable custom skybox rendering. This is needed for skyboxes to not look strange through" +
                        "SKSEffectCameras on some platforms when optimizations are enabled."));
            }
            GUILayout.EndHorizontal();
            EditorGUI.indentLevel = EditorGUI.indentLevel - _IndentSize;
        }

        GUILayout.Space(_BumperSize);
        if (_editorFoldout = EditorGUILayout.Foldout(_editorFoldout, "Editor Settings", EditorStyles.foldout))
        {
            EditorGUI.indentLevel = EditorGUI.indentLevel + _IndentSize;

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(EditorGUI.indentLevel * 10);
                SKSGlobalRenderSettings.Gizmos = GUILayout.Toggle(SKSGlobalRenderSettings.Gizmos,
                    new GUIContent("Draw Gizmos",
                        "Draw SKS Gizmos when applicable assets are selected in the Editor"));
            }
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(EditorGUI.indentLevel * 10);
                SKSGlobalRenderSettings.Preview = GUILayout.Toggle(SKSGlobalRenderSettings.Preview,
                    new GUIContent("Draw EffectRenderer Previews (experimental, buggy on many Unity versions)",
                        "Draw EffectRenderer Previews when selected in the Editor." +
                        " Experimental."));
            }
            GUILayout.EndHorizontal();
            EditorGUI.indentLevel = EditorGUI.indentLevel - _IndentSize;
        }

        GUILayout.Label("Something doesn't look right!/I'm getting errors!");

        SKSGlobalRenderSettings.UvFlip = GUILayout.Toggle(SKSGlobalRenderSettings.UvFlip,
            "My stuff is rendering upside down!");

        GUILayout.Label("Troubleshooting:");

        var path = new StackTrace(true).GetFrame(0).GetFileName();
        if (path == null)
            return;
        path = path.Substring(0, path.LastIndexOf(Path.DirectorySeparatorChar));
        var studioName = "SKStudios";
        var root = path.Substring(0, path.LastIndexOf(studioName) + studioName.Length + 1);
        var pdfPath = Path.Combine(root, AssetName);
        pdfPath = Path.Combine(pdfPath, "README.pdf");
        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Button(AssetName + " Manual")) Application.OpenURL(pdfPath);
            if (GUILayout.Button("Setup")) SK_SettingsWindow.Show();
        }
        GUILayout.EndHorizontal();
        EditorGUI.indentLevel = EditorGUI.indentLevel - _IndentSize;
#endif

    }
}