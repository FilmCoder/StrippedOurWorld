using System;
using System.IO;
using SKStudios.Common.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SKStudios.Portals.Editor {
    public class SetupPanel : GuiPanel {
        private string _documentPath;

        private readonly ProjectModeSelector _selector;
        private readonly SkSettingsWindow _window;

        public SetupPanel(SkSettingsWindow window)
        {
            this._window = window;
            _selector = new ProjectModeSelector(window);
        }


        public override string Title { get { return "Setup"; } }

        //The time that the window was opened
        private long _openTime;
        //Time before the console should be cleared on window open
        private long _timeToClear = 5000;
        private bool _cleared = false;
        /// <summary>
        /// Get the current time in ticks
        /// </summary>
        /// <returns>The current time in ticks</returns>
        private long GetTicks()
        {
           return System.DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        public override void OnEnable()
        {
            _documentPath = SetupUtility.GetDocumentationPath();
            _openTime = GetTicks();
        }

        public override void OnDisable()
        {
            _window.SelectedMode = SetupUtility.ProjectMode;
        }

        public override void OnGui(Rect position)
        {
            if (!_cleared) {
                if(GetTicks() - _openTime > _timeToClear)
                    Debug.ClearDeveloperConsole();
            }
            Styles.Init();

            GUILayout.Label(Content.WelcomeText, GlobalStyles.WelcomeTextStyle);

            var selectorRect = GUILayoutUtility.GetRect(position.width, 260);
            _selector.Draw(selectorRect);

            GUILayout.BeginVertical(Styles.ExtraStyle);

            GUILayout.Label(Content.ExtraText, EditorStyles.boldLabel);

            EditorGUIUtility.SetIconSize(new Vector2(16, 16));

            var noExtrasToShow = true;
            if (!string.IsNullOrEmpty(_documentPath)) {
                GlobalStyles.LayoutExternalLink(Content.DocumentationText, _documentPath);
                noExtrasToShow = false;
            }

            var demoPath = GetDemoPath();
            if (!string.IsNullOrEmpty(demoPath)) {
                if (GlobalStyles.LayoutButtonLink(Content.ViewDemoText))
                    EditorSceneManager.OpenScene(demoPath, OpenSceneMode.Single);
                noExtrasToShow = false;
            }

            if (noExtrasToShow) GUILayout.Label(Content.NoExtrasText);

            GUILayout.EndVertical();
        }

        private string GetDemoPath()
        {
            var path = SkEditorUtils.GetAssetRoot("PortalKit Pro");
            if (SetupUtility.ProjectMode == ProjectMode.Vr)
                path = path + "DemoScenes/VR/VRDemoScene.unity";
            else
                path = path + "DemoScenes/Non VR/Demo Scene.unity";
            path = path.Replace(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar, "");
            if (File.Exists(path)) return path;
            return string.Empty;
        }

        internal static class Content {
            public static readonly GUIContent WelcomeText = new GUIContent(
                "You're good to go! You can switch project types any time below, or configure any other setting via the left menu.");

            public static readonly GUIContent DocumentationText =
                new GUIContent("View the documentation", EditorGUIUtility.FindTexture("_Help"));

            public static readonly GUIContent ViewDemoText =
                new GUIContent("Check out a demo", EditorGUIUtility.FindTexture("SceneAsset Icon"));

            public static readonly GUIContent ExtraText = new GUIContent("What's next?");
            public static readonly GUIContent NoExtrasText = new GUIContent("• Enjoy PortalKit Pro!");
        }

        internal static class Styles {
            private static bool _initialized;

            public static GUIStyle ExtraStyle;

            public static void Init()
            {
                if (_initialized) return;
                _initialized = true;

                ExtraStyle = new GUIStyle();
                ExtraStyle.padding = new RectOffset(20, 10, 15, 0);
            }
        }
    }
}