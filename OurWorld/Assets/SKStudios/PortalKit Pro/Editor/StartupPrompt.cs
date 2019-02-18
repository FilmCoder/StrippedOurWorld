using SKStudios.Common.Editor;
using SKStudios.Mirrors.Editor;
using UnityEditor;
using UnityEngine;

namespace SKStudios.Portals.Editor {
    public class SkSettingsWindow : EditorWindow {
        public const int Setup = 0;
        public const int GeneralSettings = 1;
        public const int InteractionSettings = 2;
        public const int EditorSettings = 3;
        public const int Feedback = 4;
        public const int Debug = 5;
        public const int About = 6;

        private const string PanelPrefPath = "pkpro_active_settings_panel";

        public const string BaseMenuPath = "Tools/SK Studios/PortalKit Pro/";
        public const string MenuPath = BaseMenuPath + "Setup";

        private GuiPanel _activePanel;

        private int _activePanelIndex;

        private int[] _dividers;

        private FirstRunPanel _firstRunPanel;

        private bool _manualClose;

        private GuiPanel[] _panels;

        public ProjectMode SelectedMode;

        [MenuItem(MenuPath, priority = 0)]
        private static void ShowFromMenu()
        {
            Show(true, Setup);
        }

        public static void Hide(bool manualClose = false)
        {
            // are we already focused?
            var window = focusedWindow as SkSettingsWindow;

            if (window == null) {
                // check if window exists anywhere
                FocusWindowIfItsOpen<SkSettingsWindow>();

                window = focusedWindow as SkSettingsWindow;

                if (window != null) {
                    // create a new window
                    if (window._manualClose) {
                        window.Close();
                        window = GetWindow(true);
                    }
                    else {
                        window.Close();
                    }
                }
            }
            else {
                // window found, but it's probably hidden, and we can't do anything about that... force it open again!
                if (window._manualClose) {
                    window.Close();
                    window = GetWindow(true);
                }
                else {
                    window.Close();
                }
            }
        }

        public static void Show(bool manualClose = false, int openPanelIndex = -1)
        {
            // are we already focused?
            var window = focusedWindow as SkSettingsWindow;

            if (window == null) {
                // check if window exists anywhere
                FocusWindowIfItsOpen<SkSettingsWindow>();

                window = focusedWindow as SkSettingsWindow;

                if (window == null) window = GetWindow(manualClose);
            }
            else {
                // window found, but it's probably hidden, and we can't do anything about that... force it open again!
                window.Close();
                window = GetWindow(manualClose);
            }

            if (openPanelIndex > -1) window.ShowPanel(openPanelIndex);
        }

        private static SkSettingsWindow GetWindow(bool manualClose)
        {
            var window = CreateInstance<SkSettingsWindow>();
            window.titleContent = new GUIContent("PortalKit PRO Settings");
            window.ShowUtility();
            window._manualClose = manualClose;
            return window;
        }

        private void OnEnable()
        {
            var ignoringSetup = SetupUtility.IgnoringInitialSetup;

            SelectedMode = SetupUtility.ProjectMode;

            if (ignoringSetup) SelectedMode = ProjectMode.Default;

            if (!SetupUtility.ProjectInitialized && _firstRunPanel == null) _firstRunPanel = new FirstRunPanel(this);

            if (_panels == null) {
                _panels = new GuiPanel[] {
                    new SetupPanel(this),
                    new ImageSettingsPanel(this),
                    new InteractionSettingsPanel(this),
                    new EditorSettingsPanel(this),
                    new FeedbackPanel(this),
                    new DebugInfoPanel(this),
                    new AboutPanel()
                };

                _dividers = new[] {1, 0, 0, 1, 0, 1, 0};
            }

            _activePanelIndex = EditorPrefs.GetInt(PanelPrefPath, 0);

            UpdateEnabledPanel();

            if (SetupUtility.ProjectInitialized)
                maxSize = minSize = new Vector2(560, 480);
            else
                maxSize = minSize = new Vector2(680, 600);

            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnUndoRedo()
        {
            Repaint();
        }

        private void OnDisable()
        {
            if (_activePanel != null) _activePanel.OnDisable();

            SetupUtility.TimedFeedbackPopupActive = false;
            EditorUtility.UnloadUnusedAssetsImmediate();
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        private void OnGUI()
        {
            GlobalStyles.Init();

            if (SetupUtility.ProjectInitialized)
                DoSettingsGui();
            else
                _firstRunPanel.OnGui(position);

            //if( GUI.Button( new Rect( 0, position.height - 20, position.width, 20 ), "clear data" ) ) {
            //	SetupUtility.DEBUG_ClearSetupData();
            //}
        }

        public void ShowPanel(int panelIndex)
        {
            _activePanelIndex = panelIndex;
            UpdateEnabledPanel();
        }

        private void DoSettingsGui()
        {
            Styles.Init();

            var tabRect = new Rect(0, 0, 120, position.height);

            // tab select
            tabRect.y -= 1;
            tabRect.height += 2;


            var current = Event.current;
            if (current.type == EventType.KeyDown && GUIUtility.keyboardControl == 0) {
                if (current.keyCode == KeyCode.UpArrow) {
                    _activePanelIndex--;
                    current.Use();
                    UpdateEnabledPanel();
                }
                else if (current.keyCode == KeyCode.DownArrow) {
                    _activePanelIndex++;
                    current.Use();
                    UpdateEnabledPanel();
                }
            }


            GUILayout.BeginArea(tabRect, GUIContent.none, Styles.SidebarBg);

            GUILayout.Space(40);

            for (var i = 0; i < _panels.Length; i++) {
                var panel = _panels[i];

                var rect = GUILayoutUtility.GetRect(new GUIContent(panel.Title), Styles.SectionElement,
                    GUILayout.ExpandWidth(true));

                if (i == _activePanelIndex && Event.current.type == EventType.Repaint)
                    Styles.Selected.Draw(rect, false, false, false, false);

                EditorGUI.BeginChangeCheck();
                if (GUI.Toggle(rect, i == _activePanelIndex, panel.Title, Styles.SectionElement)) _activePanelIndex = i;

                if (_dividers[i] == 1) {
                    GUILayout.Space(10);

                    var r = GUILayoutUtility.GetRect(GUIContent.none, Styles.Divider, GUILayout.Height(1));

                    if (Event.current.type == EventType.Repaint) {
                        var c = GUI.color;
                        GUI.color = Styles.DividerColor;
                        Styles.Divider.Draw(r, false, false, false, false);
                        GUI.color = c;
                    }

                    GUILayout.Space(10);
                }

                if (EditorGUI.EndChangeCheck()) {
                    UpdateEnabledPanel();
                    GUIUtility.keyboardControl = 0;
                }
            }

            GUILayout.EndArea();

            // draw active panel
            if (_activePanel != null) {
                var panelRect = new Rect(tabRect.width + 1, 0, position.width - tabRect.width - 1, position.height);
                GUILayout.BeginArea(panelRect);
                panelRect.x = 0;
                panelRect.y = 0;
                _activePanel.OnGui(panelRect);
                GUILayout.EndArea();
            }
        }

        private void UpdateEnabledPanel()
        {
            if (_activePanelIndex < 0) _activePanelIndex = 0;
            if (_activePanelIndex > _panels.Length - 1) _activePanelIndex = _panels.Length - 1;

            if (_activePanel != null) _activePanel.OnDisable();
            _activePanel = _panels[_activePanelIndex];

            _activePanel.OnEnable();

            EditorPrefs.SetInt(PanelPrefPath, _activePanelIndex);
        }

        internal static class Styles {
            private static bool _initialized;

            public static GUIStyle SectionElement = "PreferencesSection";
            public static GUIStyle Selected = "ServerUpdateChangesetOn";
            public static GUIStyle SidebarBg = "PreferencesSectionBox";

            public static GUIStyle Divider;

            public static readonly Color DividerColor = EditorGUIUtility.isProSkin
                ? new Color(0.157f, 0.157f, 0.157f)
                : new Color(0.5f, 0.5f, 0.5f);

            public static void Init()
            {
                if (_initialized) return;
                _initialized = true;

                SidebarBg.padding.bottom = 10;

                Divider = new GUIStyle();
                Divider.normal.background = EditorGUIUtility.whiteTexture;
                Divider.stretchWidth = true;
            }
        }
    }
}