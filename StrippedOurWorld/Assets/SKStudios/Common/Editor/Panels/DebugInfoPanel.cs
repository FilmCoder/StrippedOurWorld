using System;
using System.Collections.Generic;
using System.IO;
using SKStudios.Common.Utils.SafeRemoveComponent;
using SKStudios.Mirrors.Editor;
using SKStudios.Portals.Editor;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace SKStudios.Common.Editor {
    public class DebugInfoPanel : GuiPanel {
        private readonly List<GraphRow> _displayGraph = new List<GraphRow>();
        private readonly AnimBool _fadeDebugDump;
        private readonly AnimBool _fadeDependencyGraph;
        
        private Vector2 _scrollPos;

        private int _selectedSection = -1;
        private readonly SkSettingsWindow _window;

        public DebugInfoPanel(SkSettingsWindow window)
        {
            this._window = window;

            _fadeDependencyGraph = new AnimBool(false, window.Repaint);
            _fadeDebugDump = new AnimBool(false, window.Repaint);

            _selectedSection = 1;
            UpdateFaders();
        }


        public override string Title { get { return "Debug"; } }

        [MenuItem(SkSettingsWindow.BaseMenuPath + "Debug", priority = 310)]
        private static void Show()
        {
            SkSettingsWindow.Show(true, SkSettingsWindow.Debug);
        }

        public override void OnEnable()
        {
            ConstructDisplayGraph();
        }

        public override void OnGui(Rect position)
        {
            Styles.Init();

            _scrollPos = GUILayout.BeginScrollView(_scrollPos);
            GUILayout.BeginVertical(Styles.DebugBody);

            GUILayout.Label(Title, GlobalStyles.SettingsHeaderText);

            EditorGUILayout.Space();

            //DoDebugInfo();
            DoDependencyGraph(position);

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private void UpdateFaders()
        {
            _fadeDebugDump.target = _selectedSection == 0;
            _fadeDependencyGraph.target = _selectedSection == 1;
        }

        private void DoDebugInfo()
        {
            EditorGUILayout.BeginVertical(Styles.SectionBody);

            EditorGUI.BeginChangeCheck();
            var toggled = GUILayout.Toggle(_selectedSection == 0, Content.DebugDumpHeading, Styles.SectionHeading);

            if (EditorGUI.EndChangeCheck()) {
                _selectedSection = toggled ? 0 : -1;
                UpdateFaders();
            }

            if (EditorGUILayout.BeginFadeGroup(_fadeDebugDump.faded)) {
                EditorGUILayout.Space();

                const string controlName = "pkpro_debug_dump";
                GUI.SetNextControlName(controlName);

                GUILayout.TextArea("debug log goes here", Styles.DebugTextArea, GUILayout.Height(70));
                if (GUI.GetNameOfFocusedControl() == controlName) {
                    var textEditor =
                        (TextEditor) GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                    if (Event.current.type == EventType.ValidateCommand) {
                        if (Event.current.commandName == "Copy") textEditor.Copy();
                    }
                    else if (Event.current.type != EventType.Layout || Event.current.type != EventType.Layout) {
                        textEditor.SelectAll();
                    }
                }
            }

            EditorGUILayout.EndFadeGroup();

            EditorGUILayout.EndVertical();
        }

        private void DoDependencyGraph(Rect position)
        {
            EditorGUILayout.BeginVertical(Styles.SectionBody);

            EditorGUI.BeginChangeCheck();
            var toggled = GUILayout.Toggle(_selectedSection == 1, Content.DependencyGraphHeading, Styles.SectionHeading);

            if (EditorGUI.EndChangeCheck()) {
                _selectedSection = toggled ? 1 : -1;
                UpdateFaders();
            }

            if (EditorGUILayout.BeginFadeGroup(_fadeDependencyGraph.faded)) {
                EditorGUILayout.Space();

                if (GUILayout.Button(Content.ScanButtonText, Styles.ScanButtonStyle, GUILayout.ExpandWidth(false))) {
                    EditorUtility.DisplayProgressBar(Content.RegeneratingGraphText.text, "", 1f);
                    Dependencies.RescanDictionary();
                    EditorUtility.ClearProgressBar();
                    ConstructDisplayGraph();
                }

                EditorGUILayout.Space();

                DrawGraph(position);
            }

            EditorGUILayout.EndFadeGroup();

            EditorGUILayout.EndVertical();
        }

        // resets expanded rows also
        private void ConstructDisplayGraph()
        {
            _displayGraph.Clear();

            if (Dependencies.DependencyGraph != null)
                foreach (var type in Dependencies.DependencyGraph.Keys) {
                    var row = new GraphRow();
                    row.Type = type;
                    row.Fader = new AnimBool(false, _window.Repaint);

                    // @Hack, Dependencies can return duplicates
                    List<Type> subTypes = null;
                    subTypes = new List<Type>();
                    foreach (var item in Dependencies.DependencyGraph[type]) {
                        if (subTypes.Contains(item)) continue;
                        subTypes.Add(item);
                    }

                    row.SubTypes = subTypes;
                    _displayGraph.Add(row);
                }
        }

        private void DrawGraph(Rect position)
        {
            if (Dependencies.DependencyGraph.Count == 0) {
                EditorGUILayout.HelpBox(Content.NoDataText.text, MessageType.Info);
            }
            else {
                EditorGUIUtility.SetIconSize(new Vector2(16, 16));

                foreach (var row in _displayGraph) {
                    var fader = row.Fader;
                    var type = row.Type;
                    var subTypes = row.SubTypes;

                    var entryIcon = GetIconForType(type);
                    var headerContent = new GUIContent(type.Name + " (" + subTypes.Count + ")", entryIcon);

                    fader.target = GUILayout.Toggle(fader.target, headerContent, Styles.TableRowHeaderStyle);

                    var showSubTypes = fader.value;

                    var canFade = _fadeDependencyGraph.faded == 1;
                    if (canFade) showSubTypes = EditorGUILayout.BeginFadeGroup(fader.faded);
                    if (showSubTypes) {
                        Handles.BeginGUI();
                        var color = Handles.color;
                        var firstRect = new Rect();
                        Handles.color = Styles.LineColor;

                        var indent = 10;

                        for (var i = 0; i < subTypes.Count; i++) {
                            var subType = subTypes[i];

                            var rowIcon = GetIconForType(subType);
                            GUILayout.Label(new GUIContent(subType.Name, rowIcon), Styles.TableRowStyle);

                            var iconRect = GUILayoutUtility.GetLastRect();

                            if (i == 0) firstRect = iconRect;

                            Handles.DrawLine(new Vector3((int) iconRect.x + indent - 3, (int) iconRect.y + 11),
                                new Vector3((int) iconRect.x + indent - 3 + 15, (int) iconRect.y + 11));
                        }

                        var lastRect = GUILayoutUtility.GetLastRect();
                        Handles.DrawLine(new Vector3(firstRect.x + indent - 3, firstRect.y),
                            new Vector3(lastRect.x + indent - 3, lastRect.y + 11));

                        Handles.color = color;
                        Handles.EndGUI();

                        GUILayout.Space(3);
                    }

                    if (canFade) EditorGUILayout.EndFadeGroup();
                }
            }
        }

        private static Texture2D GetIconForType(Type type)
        {
            var icon = AssetPreview.GetMiniTypeThumbnail(type);
            if (icon == null) {
                var guids = AssetDatabase.FindAssets("t:MonoScript " + type.Name);

                if (guids.Length > 0) {
                    if (guids.Length > 1)
                        for (var i = 0; i < guids.Length; i++) {
                            var guid = guids[i];
                            var path = AssetDatabase.GUIDToAssetPath(guid);
                            if (Path.GetFileNameWithoutExtension(path) == type.Name) {
                                icon = (Texture2D) AssetDatabase.GetCachedIcon(path);
                                break;
                            }
                        }
                    else
                        icon = (Texture2D) AssetDatabase.GetCachedIcon(AssetDatabase.GUIDToAssetPath(guids[0]));
                }

                if (icon == null) icon = EditorGUIUtility.FindTexture("DefaultAsset Icon");
            }

            return icon;
        }

        internal static class Content {
            public static readonly GUIContent DependencyGraphHeading =
                new GUIContent("RequireComponent Dependency Graph");

            public static readonly GUIContent DebugDumpHeading = new GUIContent("Debug Log Dump");

            public static readonly GUIContent ScanButtonText = new GUIContent("Refresh Graph");

            public static readonly GUIContent NoDataText = new GUIContent(
                "No data to show. This usually means something has gone wrong. Please press the 'refresh graph' button above.");

            public static readonly GUIContent
                RegeneratingGraphText = new GUIContent("Regenerating Dependency Graph...");
        }

        internal static class Styles {
            private static bool _initialized;


            public static GUIStyle SectionHeading;
            public static GUIStyle SectionBody;

            public static GUIStyle ScanButtonStyle;

            public static GUIStyle DebugBody;

            public static GUIStyle TableBgStyle;
            public static GUIStyle TableRowStyle;
            public static GUIStyle TableRowHeaderStyle;

            public static GUIStyle DebugTextArea;


            public static readonly Color LineColor = EditorGUIUtility.isProSkin
                ? new Color(0.157f, 0.157f, 0.157f)
                : new Color(0.5f, 0.5f, 0.5f);

            public static void Init()
            {
                if (_initialized) return;
                _initialized = true;

                SectionHeading = new GUIStyle("IN TitleText");

                ScanButtonStyle = new GUIStyle(EditorStyles.miniButton);
                ScanButtonStyle.margin = new RectOffset(0, 0, 0, 0); // reset margin to avoid shifting during fades

                TableBgStyle = new GUIStyle();
                TableBgStyle.normal.background = EditorGUIUtility.whiteTexture;
                TableBgStyle.padding = new RectOffset(0, 0, 10, 0);

                TableRowStyle = new GUIStyle(EditorStyles.label);
                TableRowStyle.border = new RectOffset(2, 2, 2, 2);
                TableRowStyle.padding = new RectOffset(30, 10, 4, 4);
                TableRowStyle.contentOffset = new Vector2(0, -2);
                TableRowStyle.alignment = TextAnchor.MiddleLeft;
                TableRowStyle.margin = new RectOffset(0, 0, 0, 0); // reset margin to avoid shifting during fades

                TableRowHeaderStyle = new GUIStyle("foldout");
                TableRowHeaderStyle.contentOffset = new Vector2(0, -2);
                TableRowHeaderStyle.margin = new RectOffset(0, 0, 0, 0); // reset margin to avoid shifting during fades

                DebugBody = new GUIStyle();
                DebugBody.padding = new RectOffset(10, 20, 10, 20);

                SectionBody = new GUIStyle(EditorStyles.helpBox);
                SectionBody.padding.left = 14;

                DebugTextArea = new GUIStyle(EditorStyles.textArea);
                DebugTextArea.margin = new RectOffset(0, 0, 0, 0); // reset margin to avoid shifting during fades
            }
        }

        private struct GraphRow {
            public Type Type;
            public List<Type> SubTypes;

            public AnimBool Fader;
        }
    }
}