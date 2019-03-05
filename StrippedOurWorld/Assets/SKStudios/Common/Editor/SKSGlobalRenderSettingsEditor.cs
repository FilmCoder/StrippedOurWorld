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

using System.Diagnostics;
using System.IO;
using SKStudios.Mirrors.Editor;
using SKStudios.Portals.Editor;
using SKStudios.Rendering;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.Events;

namespace SKStudios.Common.Editor {
    /// <summary>
    ///     Class providing a menu for <see cref="SKSGlobalRenderSettings" />
    /// </summary>
    public class SKSGlobalRenderSettingsEditor {
        private bool _editorFoldout;

        private bool _imageFoldout;
        private bool _interactionFoldout;

        private readonly AnimBool _fadeAggressiveRecursionWarning;
        private readonly AnimBool _fadePhysicsPassthroughWarning;
        private readonly AnimBool _fadeRecursionNumberWarning;

        /// <summary>
        ///     Initialize an instance of this menu with the given draw callback
        /// </summary>
        /// <param name="callback"></param>
        public SKSGlobalRenderSettingsEditor(UnityAction callback)
        {
            _fadeAggressiveRecursionWarning =
                new AnimBool(SKSGlobalRenderSettings.AggressiveRecursionOptimization, callback);
            _fadeRecursionNumberWarning = new AnimBool(SKSGlobalRenderSettings.RecursionNumber > 1, callback);
            _fadePhysicsPassthroughWarning = new AnimBool(SKSGlobalRenderSettings.PhysicsPassthrough, callback);
        }

        /// <summary>
        ///     Draw all settings UI
        /// </summary>
        public void DrawAllGui(string assetName)
        {
            GUILayout.Label("SKS Global settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;
            {
                if (_imageFoldout = EditorGUILayout.Foldout(_imageFoldout, "Image Settings", EditorStyles.foldout)) {
                    RenderingGui();
                    EditorGUILayout.Space();
                    RecursionGui();
                    EditorGUILayout.Space();
                }

#if SKS_PORTALS
                if (_interactionFoldout = EditorGUILayout.Foldout(_interactionFoldout,
                    "Interaction Settings (for PortalKit)", EditorStyles.foldout)) {
                    InteractionSettingsGui();
                    EditorGUILayout.Space();
                }
#endif

                if (_editorFoldout = EditorGUILayout.Foldout(_editorFoldout, "Editor Settings", EditorStyles.foldout)) {
                    EditorSettingsGui();
                    EditorGUILayout.Space();
                }

                EditorGUILayout.Space();
                HelpGui(assetName);
            }
            EditorGUI.indentLevel -= 1;
        }

        /// <summary>
        ///     Draw the GUI for rendering settings
        /// </summary>
        public void RenderingGui()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField(Content.RenderingTitle, EditorStyles.boldLabel);
#if SKS_PORTALS
            SKSGlobalRenderSettings.Clipping =
                EditorGUILayout.Toggle(Content.ClippingLabel, SKSGlobalRenderSettings.Clipping);
#endif
            SKSGlobalRenderSettings.CustomSkybox =
                EditorGUILayout.Toggle(Content.OverrideSkyboxLabel, SKSGlobalRenderSettings.CustomSkybox);
            SKSGlobalRenderSettings.ShouldOverrideMask = EditorGUILayout.Toggle(Content.OverrideMasksLabel,
                SKSGlobalRenderSettings.ShouldOverrideMask);
            GUI.enabled = SKSGlobalRenderSettings.ShouldOverrideMask;
            {
                EditorGUI.indentLevel += 1;
                SKSGlobalRenderSettings.Mask =
                    (Texture2D) EditorGUILayout.ObjectField(SKSGlobalRenderSettings.Mask, typeof(Texture2D), false);
                EditorGUI.indentLevel -= 1;
            }
            GUI.enabled = true;
            SKSGlobalRenderSettings.UvFlip =
                EditorGUILayout.Toggle(Content.FlipUVsLabel, SKSGlobalRenderSettings.UvFlip);
            SKSGlobalRenderSettings.PlaceholderResolution = EditorGUILayout.IntField(Content.PlaceholderResLabel,
                SKSGlobalRenderSettings.PlaceholderResolution);
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(SKSGlobalRenderSettings.Instance);
        }

        /// <summary>
        ///     Draw the GUI for Recursion settings
        /// </summary>
        public void RecursionGui()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField(Content.RecursionTitle, EditorStyles.boldLabel);

            // recursion number
            SKSGlobalRenderSettings.RecursionNumber = EditorGUILayout.IntSlider(Content.RecursionLabel,
                SKSGlobalRenderSettings.RecursionNumber, 0, 10);
            _fadeRecursionNumberWarning.target = SKSGlobalRenderSettings.RecursionNumber > 1;

            if (EditorGUILayout.BeginFadeGroup(_fadeRecursionNumberWarning.faded)) {
                var msgType = SKSGlobalRenderSettings.RecursionNumber > 5 ? MessageType.Error : MessageType.Warning;
                EditorGUILayout.HelpBox(Content.RecursionQualityWarning.text, msgType, true);
            }

            EditorGUILayout.EndFadeGroup();

            // adaptive quality
            SKSGlobalRenderSettings.AdaptiveQuality = EditorGUILayout.Toggle(Content.AdaptiveQualityLabel,
                SKSGlobalRenderSettings.AdaptiveQuality);

            // aggressive optimization
            SKSGlobalRenderSettings.AggressiveRecursionOptimization = EditorGUILayout.Toggle(
                Content.AggressiveOptimiziationLabel, SKSGlobalRenderSettings.AggressiveRecursionOptimization);
            _fadeAggressiveRecursionWarning.target = SKSGlobalRenderSettings.AggressiveRecursionOptimization;

            if (EditorGUILayout.BeginFadeGroup(_fadeAggressiveRecursionWarning.faded))
                EditorGUILayout.HelpBox(Content.AggressiveOptimizinationWarning.text, MessageType.Info, true);

            EditorGUILayout.EndFadeGroup();

            if(EditorGUI.EndChangeCheck())
               EditorUtility.SetDirty(SKSGlobalRenderSettings.Instance);
        }

        /// <summary>
        ///     Draw the editor setting GUI
        /// </summary>
        public void EditorSettingsGui()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField(Content.EditorTitle, EditorStyles.boldLabel);
            SKSGlobalRenderSettings.Gizmos =
                EditorGUILayout.Toggle(Content.DrawGizmosLabel, SKSGlobalRenderSettings.Gizmos);
            SKSGlobalRenderSettings.Visualization = EditorGUILayout.Toggle(Content.VisualizeConnectionsLabel,
                SKSGlobalRenderSettings.Visualization);
            SKSGlobalRenderSettings.Preview =
                EditorGUILayout.Toggle(Content.PreviewLabel, SKSGlobalRenderSettings.Preview);
            SKSGlobalRenderSettings.PreviewResolution = EditorGUILayout.IntField(Content.PreviewResLabel,
                SKSGlobalRenderSettings.PreviewResolution);
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(SKSGlobalRenderSettings.Instance);
        }

        /// <summary>
        ///     Draw the interaction setting GUI
        /// </summary>
        public void InteractionSettingsGui()
        {
#if SKS_PORTALS
            EditorGUI.BeginChangeCheck();
            Undo.RecordObject(SKSGlobalRenderSettings.Instance, "Update PortalKit Pro Settings");

            EditorGUILayout.LabelField(Content.PhysicsHeading, EditorStyles.boldLabel);

            // physics passthrough
            SKSGlobalRenderSettings.PhysicsPassthrough = EditorGUILayout.Toggle(Content.PhysicsPassthroughLabel,
                SKSGlobalRenderSettings.PhysicsPassthrough);
            _fadePhysicsPassthroughWarning.target = SKSGlobalRenderSettings.PhysicsPassthrough;

            if (EditorGUILayout.BeginFadeGroup(_fadePhysicsPassthroughWarning.faded))
                EditorGUILayout.HelpBox(Content.PhysicsPassthroughWarning.text, MessageType.Info, true);
            EditorGUILayout.EndFadeGroup();

            // phys style b
            SKSGlobalRenderSettings.PhysStyleB =
                EditorGUILayout.Toggle(Content.PhysicsModelBLabel, SKSGlobalRenderSettings.PhysStyleB);

            // scaled renderers
            SKSGlobalRenderSettings.NonScaledRenderers = !EditorGUILayout.Toggle(Content.PortalScalingLabel,
                !SKSGlobalRenderSettings.NonScaledRenderers);
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(SKSGlobalRenderSettings.Instance);
#endif
        }

        /// <summary>
        ///     Draw the help GUI for this object
        /// </summary>
        /// <param name="assetName">Name of the asset that invoked this method</param>
        public void HelpGui(string assetName)
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.Label("Something doesn't look right!/I'm getting errors!", EditorStyles.boldLabel);

            SKSGlobalRenderSettings.UvFlip = GUILayout.Toggle(SKSGlobalRenderSettings.UvFlip,
                "My stuff is rendering upside down!");

            GUILayout.Label("Troubleshooting:");

            var path = new StackTrace(true).GetFrame(0).GetFileName();
            if (path == null)
                return;
            path = path.Substring(0, path.LastIndexOf(Path.DirectorySeparatorChar));
            var studioName = "SKStudios";
            var root = path.Substring(0, path.LastIndexOf(studioName) + studioName.Length + 1);
            var pdfPath = Path.Combine(root, assetName);
            pdfPath = Path.Combine(pdfPath, "README.pdf");
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(assetName + " Manual")) Application.OpenURL(pdfPath);
                if (GUILayout.Button("Setup")) SkSettingsWindow.Show();
            }
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(SKSGlobalRenderSettings.Instance);
        }

        internal static class Content {
            public static readonly GUIContent RecursionTitle = new GUIContent("Recursion");
            public static readonly GUIContent RenderingTitle = new GUIContent("Rendering");
            public static readonly GUIContent EditorTitle = new GUIContent("Editor");

            public static readonly GUIContent RecursionLabel = new GUIContent("Recursion Number",
                "The number of times that effect renderers will draw through each other.");

            public static readonly GUIContent RecursionQualityWarning =
                new GUIContent(
                    "Recursion can get very expensive very quickly. Consider making this scale with the quality settings of your game.");

            public static readonly GUIContent AggressiveOptimiziationLabel = new GUIContent(
                "Use Aggressive Optimization",
                "Aggressive optimization will halt recursive rendering immediately if the " +
                "source effect renderer cannot raycast to the effect renderer it is trying to render.\n" +
                "Without occlusion culling (due to lack of Unity support), this is a lifesaver for " +
                "large scenes."
            );

            public static readonly GUIContent AggressiveOptimizinationWarning = new GUIContent(
                "Enabling aggressive optimization can save some serious performance, but it is possible for visual bugs to arise due to effect renderers being partially " +
                "inside walls.\n" +
                "If you are seeing black effect renderers while recursing, try turning this option off and see if it helps. If it does, then please " +
                "make sure that your effect renderers are not inside walls."
            );


            public static readonly GUIContent AdaptiveQualityLabel = new GUIContent(
                "Use Adaptive Quality (Recommended)",
                "Adaptive quality rapidly degrades the quality of recursively rendered effect renderers. This is usually desirable.");

            public static readonly GUIContent ClippingLabel = new GUIContent("Use Perfect Object Clipping",
                "Enable objects clipping as they enter effect renderers. This is usually desirable.");

            public static readonly GUIContent OverrideSkyboxLabel = new GUIContent("Override Skybox",
                "Enable custom skybox rendering. This is needed for skyboxes to not look strange through SKSEffectCameras.");

            public static readonly GUIContent OverrideMasksLabel =
                new GUIContent("Override All effect rendererSpawner Masks");

            public static readonly GUIContent PlaceholderResLabel =
                new GUIContent("Resolution of placeholders", "Must be a power of 2");

            public static readonly GUIContent PreviewResLabel =
                new GUIContent("Resolution of editor Cubemap previews", "Must be a power of 2");

            public static readonly GUIContent FlipUVsLabel = new GUIContent("Flip UVs",
                "On some hardware, UVs are laid out top-to-bottom rather than bottom-to-top. Check this if your effect renderers are rendering upside down.");

            public static readonly GUIContent DrawGizmosLabel = new GUIContent("Draw effect renderer Gizmos",
                "Draw effect renderer gizmos when selected in the editor, and when all effect renderers are visualized.");

            public static readonly GUIContent VisualizeConnectionsLabel =
                new GUIContent("Visualize effect renderer Connections",
                    "Visualize all effect renderer connections in the scene.");

            public static readonly GUIContent PreviewLabel = new GUIContent(
                "Draw effect renderer Previews (Experimental)",
                "Draw effect renderer previews when selected in the editor. Experimental, and only works with shallow viewing angles.");

            public static readonly GUIContent PhysicsHeading = new GUIContent("Physics");

            public static readonly GUIContent PhysicsPassthroughLabel = new GUIContent("Enable Physics Passthrough",
                "Enable collision with objects on the other side of portals.");

            public static readonly GUIContent PhysicsPassthroughWarning = new GUIContent(
                "This setting enables interaction with objects on the other side of portals. " +
                "Objects can pass through portals without it, and it is not needed for most games. " +
                "In extreme cases, it can cause a slight performance hit."
            );

            public static readonly GUIContent PhysicsModelBLabel = new GUIContent("Use Physics Model B",
                "Physics Model B maintains relative momentum between portals. This may or may not be desirable when the portals move.");

            public static readonly GUIContent PortalScalingLabel = new GUIContent("Enable portal Scaling",
                "This should be disabled if portals are never used to change an object's size.");
        }
    }
}