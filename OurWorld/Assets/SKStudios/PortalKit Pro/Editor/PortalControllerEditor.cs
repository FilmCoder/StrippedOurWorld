using System;
using SKStudios.Rendering;
using UnityEditor;
using UnityEngine;

namespace SKStudios.Portals.Editor {
    /// <summary>
    ///     Editor for <see cref="PortalController" /> components, which control and spawn <see cref="Portal" />s.
    /// </summary>
    [CustomEditor(typeof(PortalController))]
    [InitializeOnLoad]
    [CanEditMultipleObjects]
    [ExecuteInEditMode]
    public class PortalControllerEditor : EffectRendererControllerEditor {
        private static Material _invisibleMaterial;

        private static bool _interactionFoldout;
        private static bool _editorFoldout;
        private static bool _imageFoldout;

        private static Camera _sceneCameraDupe;

        private PortalController _portalController;

        //Preview texture for portals
        private RenderTexture _previewTex;

        private string _sourceName;

        protected override string AssetName { get { return "PortalKit Pro"; } }

        private static Material InvisibleMaterial {
            get {
                if (!_invisibleMaterial)
                    _invisibleMaterial = Resources.Load<Material>("UI/Materials/Invisible");
                return _invisibleMaterial;
            }
        }

        protected override GameObject Target { get { return TargetController.gameObject; } }

        protected override bool MirroredPreview { get { return false; } }

        private RenderTexture PreviewTex {
            get {
                if (_previewTex)
                    RenderTexture.ReleaseTemporary(_previewTex);

                _previewTex = RenderTexture.GetTemporary(Screen.width, Screen.height, 24,
                    RenderTextureFormat.ARGB2101010, RenderTextureReadWrite.Default);

                return _previewTex;
            }
        }

        protected PortalController TargetController {
            get {
                if (!_portalController)
                    _portalController = (PortalController) target;
                return _portalController;
            }
        }

        protected override Material TargetMaterial {
            get { return TargetController.Settings ? TargetController.Settings.OriginalMaterial : null; }
            set { TargetController.Settings.OriginalMaterial = value; }
        }

        private void OnEnable()
        {
            if (Application.isPlaying) return;

            if (!TargetController.gameObject.activeInHierarchy)
                return;
            if (!TargetController.isActiveAndEnabled)
                return;
            if (!TargetController.TargetController)
                return;

            if (SKSGlobalRenderSettings.Preview) {
                TargetController.PreviewRoot.SetActive(true);
                TargetController.GetComponent<Renderer>().sharedMaterial = InvisibleMaterial;
                var pokecam = TargetController.PreviewCamera;
                var pokeObj = TargetController.PreviewRoot;
                var pokecam2 = TargetController.TargetController.PreviewCamera;
                var pokeObj2 = TargetController.TargetController.PreviewRoot;
                pokecam2.enabled = false;
                pokeObj2.SetActive(false);
            }

            TargetController.UpdatePropertyBlock(TargetController.Color);

            //EditorApplication.update -= UpdatePreview;
            //EditorApplication.update += UpdatePreview;

#if SKS_VR
//GlobalPortalSettings.SinglePassStereo = settings.SinglePassStereoCached;
#endif
        }


        private void OnDisable()
        {
            if (!TargetController) return;
            if (Selection.activeGameObject == TargetController.gameObject) return;
            CleanupTemp();

            if (SKSGlobalRenderSettings.Preview && TargetController.PreviewRoot)
                TargetController.PreviewRoot.SetActive(false);

            if (TargetController && TargetController.TargetController)
                TargetController.TargetController.CleanupTemp();

            if (!Application.isPlaying) {
                TargetController.GetComponent<Renderer>().sharedMaterial = TargetController.OriginalMaterial;
            }
        }

        private void CleanupTemp()
        {
            if (TargetController) {
                var renderer = TargetController.GetComponent<MeshRenderer>();
                if (renderer)
                    renderer.enabled = true;
            }

            TargetController.CleanupTemp();
        }


        protected override void DrawInstanceUi()
        {
            try
            {
                GUILayout.Label("Instance settings:", EditorStyles.boldLabel);
                Undo.RecordObjects(targets, "Portal Controller Editor Changes");

                EditorGUI.BeginChangeCheck();
                TargetController.TargetController = (PortalController) EditorGUILayout.ObjectField(
                    new GUIContent("Target Controller", "The targetTransform of this Portal."),
                    TargetController.TargetController, typeof(PortalController), true, null);
                if (EditorGUI.EndChangeCheck())
                    foreach (PortalController p in targets) {
                        EditorUtility.SetDirty(p);
                        p.TargetController = TargetController.TargetController;
                    }

                //if (!PortalController.PortalScript.PortalCamera ||
                //    !PortalController.TargetController.PortalScript.PortalCamera) return;

                EditorGUI.BeginChangeCheck();
                TargetController.Enterable =
                    EditorGUILayout.Toggle(
                        new GUIContent("Enterable", "Is the Portal Enterable by Teleportable Objects?"),
                        TargetController.Enterable);
                if (EditorGUI.EndChangeCheck())
                    foreach (PortalController p in targets)
                        p.Enterable = TargetController.Enterable;

                //Deprecated
                /*
                EditorGUI.BeginChangeCheck();
                TargetController.Is3D =
                    EditorGUILayout.Toggle(
                        new GUIContent("Portal is 3D Object", "Is the Portal a 3d object, such as a Crystal ball?"),
                        TargetController.Is3D);
                if (EditorGUI.EndChangeCheck())
                    foreach (PortalController p in targets)
                        p.Is3D = TargetController.Is3D;*/


                EditorGUI.BeginChangeCheck();
                TargetController.DetectionScale = EditorGUILayout.Slider(
                    new GUIContent("Detection Zone Scale", "The scale of the portal detection zone."),
                    TargetController.DetectionScale, 0.1f, 10f);
                if (EditorGUI.EndChangeCheck())
                    foreach (PortalController p in targets)
                        p.DetectionScale = TargetController.DetectionScale;

                if (TargetController.DetectionScale < 1)
                    EditorGUILayout.HelpBox(
                        "It is recommended that you do not set the detection scale " +
                        "below 1, as lower values have a very high chance of not " +
                        "detecting objects at all as they may move too quickly",
                        MessageType.Warning);
                EditorGUI.BeginChangeCheck();
                TargetController.IgnoreRearCollisions = EditorGUILayout.Toggle(
                    new GUIContent("Ignore Rear Colliders",
                        "Allows objects to pass through this portal " +
                        "even if there are colliders behind it. " +
                        "You generally want this to be enabled."), TargetController.IgnoreRearCollisions);


                //Show the Portal Material Inspector
                if (Application.isPlaying)
                    return;
            }
            catch(Exception e) {
#if SKS_DEV
                Debug.Log("SKS Dev Error from PortalControllerEditor: " + e.Message);
#endif
            }

            TargetMeshRenderer.gameObject.SetActive(false);
        }

        protected override void DrawCustomGlobalUi()
        {
            GUILayout.Label("Global Portal Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel = EditorGUI.indentLevel + _IndentSize;
#if !SKS_VR
            if (!PlayerTeleportable.PlayerTeleportableScript) {
                EditorGUILayout.HelpBox(
                    "No PlayerTeleportable set. Seamless camera passthrough will not function." +
                    " Add a PlayerTeleportable script to your teleportable player object.",
                    MessageType.Warning);
            }
            else {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Player Teleportable");
                EditorGUILayout.ObjectField(PlayerTeleportable.PlayerTeleportableScript.gameObject, typeof(object),
                    true);
                GUILayout.EndHorizontal();
            }
#endif
            if (_interactionFoldout =
                EditorGUILayout.Foldout(_interactionFoldout, "Interaction Settings", EditorStyles.foldout)) {
                EditorGUI.indentLevel = EditorGUI.indentLevel + _IndentSize;

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(EditorGUI.indentLevel * 10);
                    SKSGlobalRenderSettings.PhysicsPassthrough = GUILayout.Toggle(
                        SKSGlobalRenderSettings.PhysicsPassthrough,
                        new GUIContent("Enable Physics Passthrough",
                            "Enable collision with objects on the other side of portals"));
                }
                GUILayout.EndHorizontal();


                if (SKSGlobalRenderSettings.PhysicsPassthrough)
                    EditorGUILayout.HelpBox(
                        "This setting enables interaction with objects on the other side of portals. " +
                        "Objects can pass through portals without it, and it is not needed for most games. " +
                        "In extreme cases, it can cause a slight performance hit.",
                        MessageType.Info);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(EditorGUI.indentLevel * 10);
                    SKSGlobalRenderSettings.PhysStyleB = GUILayout.Toggle(SKSGlobalRenderSettings.PhysStyleB,
                        new GUIContent("Enable Physics Model B (More Accurate)",
                            "Physics Model B maintains relative momentum between portals." +
                            " This may or may not be desirable when the portals move."));
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(EditorGUI.indentLevel * 10);
                    SKSGlobalRenderSettings.NonScaledRenderers = GUILayout.Toggle(
                        SKSGlobalRenderSettings.NonScaledRenderers,
                        new GUIContent("Disable Portal scaling",
                            "Disable portal scaling. This should be enabled if " +
                            "portals are never used to change object's size."));
                }
                GUILayout.EndHorizontal();

                EditorGUI.indentLevel = EditorGUI.indentLevel - _IndentSize;
            }

            if (_editorFoldout = EditorGUILayout.Foldout(_editorFoldout, "Editor Settings", EditorStyles.foldout)) {
                EditorGUI.indentLevel = EditorGUI.indentLevel + _IndentSize;
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(EditorGUI.indentLevel * 10);
                    SKSGlobalRenderSettings.Visualization = GUILayout.Toggle(SKSGlobalRenderSettings.Visualization,
                        new GUIContent("Visualize Portal Connections",
                            "Visualize all portal connections in the scene"));
                }
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel = EditorGUI.indentLevel - _IndentSize;
            }

            if (_imageFoldout = EditorGUILayout.Foldout(_imageFoldout, "Image Settings", EditorStyles.foldout)) {
                EditorGUI.indentLevel = EditorGUI.indentLevel + _IndentSize;
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(EditorGUI.indentLevel * 10);
                    SKSGlobalRenderSettings.Clipping = GUILayout.Toggle(SKSGlobalRenderSettings.Clipping,
                        new GUIContent("Enable perfect object clipping",
                            "Enable objects clipping as they enter portals. This is usually desirable."));
                }
                GUILayout.EndHorizontal();

                /*
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(EditorGUI.indentLevel * 10);
                    SKSGlobalRenderSettings.ShouldOverrideMask = GUILayout.Toggle(
                        SKSGlobalRenderSettings.ShouldOverrideMask,
                        "Override Masks on all PortalSpawners");
                }
                GUILayout.EndHorizontal();

                if (SKSGlobalRenderSettings.ShouldOverrideMask) {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(EditorGUI.indentLevel * 10);
                    SKSGlobalRenderSettings.Mask =
                        (Texture2D) EditorGUILayout.ObjectField(SKSGlobalRenderSettings.Mask, typeof(Texture2D),
                            false);
                    GUILayout.EndHorizontal();
                }*/
                EditorGUI.indentLevel = EditorGUI.indentLevel - _IndentSize;
            }

            EditorGUI.indentLevel = EditorGUI.indentLevel - _IndentSize;
        }
    }
}