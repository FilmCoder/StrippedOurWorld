using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using SKStudios.Common.Utils;
using SKStudios.Rendering;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace SKStudios.Portals.Editor {
    /// <summary>
    ///     Used to setup SKSGlobalRenderSettings. Appears as a semitransparent fogged docked window.
    /// </summary>
    [InitializeOnLoad]
    [ExecuteInEditMode]
    public class SceneViewSettings : EditorWindow {
        private const int MaxRating = 5;
        private const int SupportRating = 3;

        private static Rect _windowRect;
        private static GUISkin _skin;

        private static Vector2 _scrollPos;
        private static Vector2 _defaultDockLocation = Vector2.zero;


        private static double _startTime;
        private static readonly float Movetime = 4f;
        private static readonly Vector2 DefaultSize = new Vector2(740, 220);
        private static Vector2 _size = DefaultSize;
        private static readonly Vector2 MinimizedSize = new Vector2(200, 80);
        private static float _time;
        private static Camera _sceneCamera;
        private static bool _compatabilitymode;

        private static bool _hasLoadedWebpage;

        private static Texture _texturePlaceholder = new Texture2D(1, 1);

        private static NotificationInfo _notificationInfo;

        private static Color _borderColor = new Color(0.5f, 0.5f, 0.5f, 1);

        private static string _notificationString;

        static SceneViewSettings()
        {
            EditorApplication.update += Enable;
        }

        public static Color BorderColor {
            get { return _borderColor; }
            set {
                _borderColor = value;
                Styles.BorderMat.color = _borderColor;
            }
        }

        [DidReloadScripts]
        public static void AutoOpen()
        {
            //Touch all setting ScriptableObjects so that they are not created off of the main thread
            var touch = SKSGlobalRenderSettings.Instance;
            var touch2 = GlobalPortalSettings.Instance;
            //todo: Restore
            //if (!hasLoadedWebpage)
            {
                _notificationInfo = new NotificationInfo();
                new Thread(GetNotifications).Start();
                _hasLoadedWebpage = true;
            }
        }

        /// <summary>
        ///     Enable the menu
        /// </summary>
        [MenuItem("Tools/SK Studios/PortalKit Pro/Notifications", priority = 100)]
        public static void MenuEnable()
        {
            //Undo.RecordObject(SKSGlobalRenderSettings.Instance, "Global Portal Settings");
            SKSGlobalRenderSettings.MenuClosed = false;
            EditorApplication.update += Enable;
        }

        /// <summary>
        ///     Setup the menu
        /// </summary>
        public static void Enable()
        {
            Disable();
            _compatabilitymode = SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX;
            EditorApplication.update -= Enable;
            SKSGlobalRenderSettings.Minimized = true;
            // EditorApplication.update += UpdateRect;
            _skin = Resources.Load<GUISkin>("UI/PortalEditorGUISkin");
            SceneView.onSceneGUIDelegate += OnScene;
        }

        /// <summary>
        ///     Cleanup
        /// </summary>
        public static void Disable()
        {
            //SceneView.onSceneGUIDelegate -= OnScene;
        }

        private static void UpdateRect()
        {
            if (Event.current.type == EventType.Layout) {
                /*
                if (!compatabilitymode)
                    defaultDockLocation = new Vector2((Screen.width) - size.x - 10, (Screen.height) - size.y - 19);
                else
                    defaultDockLocation = new Vector2(0, 20);*/
                _defaultDockLocation = new Vector2(Screen.width / 2 - _windowRect.width / 2, 16);

                _windowRect = new Rect(_defaultDockLocation.x, _defaultDockLocation.y, _size.x, _size.y);
                _time = (float) (EditorApplication.timeSinceStartup - _startTime) * Movetime;
                if (_time > 1)
                    _time = 1;

                if (SKSGlobalRenderSettings.Minimized) {
                    //windowRect.position = Mathfx.Sinerp(defaultDockLocation + new Vector2(0, size.y - 20),
                    //    defaultDockLocation, 1 - time);
                    _size.x = Mathfx.Sinerp(DefaultSize.x, MinimizedSize.x, _time);
                    _size.y = Mathfx.Sinerp(DefaultSize.y, MinimizedSize.y, _time);
                    if (_time < 1)
                        SceneView.RepaintAll();
                }
                else {
                    //windowRect.position = Mathfx.Sinerp(defaultDockLocation + new Vector2(0, size.y - 20),
                    //    defaultDockLocation, time);
                    _size.x = Mathfx.Sinerp(DefaultSize.x, MinimizedSize.x, 1 - _time);
                    _size.y = Mathfx.Sinerp(DefaultSize.y, MinimizedSize.y, 1 - _time);
                    if (_time < 1)
                        SceneView.RepaintAll();
                }
            }
        }


        private static void OnScene(SceneView sceneview)
        {
            Styles.Init();
            if (!_texturePlaceholder) _texturePlaceholder = new Texture2D(1, 1);
            if (!_hasLoadedWebpage) {
                _notificationInfo = new NotificationInfo();
                new Thread(GetNotifications).Start();
                _hasLoadedWebpage = true;
            }

#if SKS_VR
            SKSGlobalRenderSettings.SinglePassStereo =
 PlayerSettings.stereoRenderingPath == StereoRenderingPath.SinglePass;
#endif

            if (Camera.current.name.Equals("SceneCamera")) {
                _sceneCamera = Camera.current;
                UiBlurController.AddBlurToCamera(_sceneCamera);
            }


            Handles.BeginGUI();


            GUI.skin = _skin;
            //windowRect = new Rect (Screen.width - size.x - 100, Screen.height - size.y - 190, 200, 200);
            if (SetupUtility.ProjectInitialized) {
                if (SKSGlobalRenderSettings.MenuClosed) {
                    Handles.EndGUI();
                    return;
                }

                //Blur
                Graphics.DrawTexture(new Rect(_windowRect.x, _windowRect.y - 16, _windowRect.width, _windowRect.height),
                    _texturePlaceholder, Styles.BlurMat);
                //Window Border
                //Graphics.DrawTexture(new Rect(windowRect.x, windowRect.y - 16, windowRect.width, windowRect.height),
                //    Styles.borderTex, Styles.borderMat);
                GUI.Window(0, _windowRect, DoWindow, "<color=#2599f5>[PKPRO]</color> Notifications:");

                GUI.skin = null;
                Handles.EndGUI();
            }
            else {
                Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _texturePlaceholder, Styles.BlurMat);
                if (GUILayout.Button("Click here to re-open the import dialog"))
                    SkSettingsWindow.Show();
#if SKS_DEV

                if (GUILayout.Button(
                    "(Intended for the devs of the asset) Click here to reset the ScriptableObjects for deployment")) {
                    SKSGlobalRenderSettings.RecursionNumber = 0;
                    SKSGlobalRenderSettings.AggressiveRecursionOptimization = true;
                    SKSGlobalRenderSettings.AdaptiveQuality = true;
                    SKSGlobalRenderSettings.CustomSkybox = true;
                    SKSGlobalRenderSettings.Preview = false;
                    SKSGlobalRenderSettings.PhysicsPassthrough = false;
                    SKSGlobalRenderSettings.PhysStyleB = true;
                    SKSGlobalRenderSettings.Gizmos = true;
                    SKSGlobalRenderSettings.IgnoredNotifications = new List<int>();
                    SKSGlobalRenderSettings.MenuClosed = false;
                    SKSGlobalRenderSettings.Minimized = false;
                    GlobalPortalSettings.Nagged = false;
                    EditorUtility.SetDirty(GlobalPortalSettings.Instance);
                }
#endif
            }
        }

        /// <summary>
        ///     Displays the window
        /// </summary>
        /// <param name="windowId">ID of the window</param>
        public static void DoWindow(int windowId)
        {
            BorderColor = Color.Lerp(new Color(1, 1, 1, 1), new Color(0, 0, 0, 1),
                Mathf.Sin((float) EditorApplication.timeSinceStartup));
            UpdateRect();
            EditorGUILayout.BeginVertical();
            Undo.RecordObject(SKSGlobalRenderSettings.Instance, "SKSGlobalRenderSettings");
            //Header controls
            EditorGUI.BeginChangeCheck();
            if (GUI.Button(new Rect(3, 3, 15, 15), "X", Styles.MenuOptionsStyle)) {
                SKSGlobalRenderSettings.MenuClosed = true;
                Disable();
            }

            if (GUI.Button(new Rect(20, 3, 15, 15), SKSGlobalRenderSettings.Minimized ? "□" : "_",
                Styles.MenuOptionsStyle)) {
                SKSGlobalRenderSettings.Minimized = !SKSGlobalRenderSettings.Minimized;
                _startTime = EditorApplication.timeSinceStartup;
            }


            var guiColor = GUI.color;

            var activeNotifications = _notificationInfo.GetActiveNotifications();
            if (activeNotifications.Count == 0) {
                GUILayout.FlexibleSpace();
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("No notifications to display");
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndHorizontal();
                }
                //GUILayout.FlexibleSpace();
            }
            else if (SKSGlobalRenderSettings.Minimized) {
                var messages = 0;
                var warnings = 0;
                var errors = 0;
                foreach (var n in activeNotifications)
                    switch (n.Type) {
                        case 1:
                            messages++;
                            break;
                        case 2:
                            warnings++;
                            break;
                        case 3:
                            errors++;
                            break;
                        default:
                            messages++;
                            break;
                    }

                Func<int, int, Rect> getInfoIconPos = (index, total) => {
                    var hsize = 16;
                    var vsize = 16;
                    var padding = 3;
                    var vOffset = 10;
                    return new Rect(
                        0, vOffset + ((vsize + padding) * index - (vsize * total + (padding - 1) * total) / 2) +
                           _windowRect.height / 2, hsize, vsize);
                };
                GUILayout.BeginHorizontal();
                {
                    int number = 0, index = 0;
                    if (messages > 0)
                        number++;
                    if (warnings > 0)
                        number++;
                    if (errors > 0)
                        number++;

                    Rect positionRect;
                    if (errors > 0) {
                        positionRect = getInfoIconPos(index++, number);
                        GUI.DrawTexture(positionRect, Content.ErrorIconSmall);
                        positionRect.x += positionRect.width;
                        positionRect.width = 400;
                        GUI.Label(positionRect, string.Format("{0} new critical notifications", errors),
                            Styles.NotificationTextStyle);
                    }


                    if (warnings > 0) {
                        positionRect = getInfoIconPos(index++, number);
                        GUI.DrawTexture(positionRect, Content.WarnIconSmall);
                        positionRect.x += positionRect.width;
                        positionRect.width = 400;
                        GUI.Label(positionRect, string.Format("{0} new important notifications", warnings),
                            Styles.NotificationTextStyle);
                    }


                    if (messages > 0) {
                        positionRect = getInfoIconPos(index++, number);
                        GUI.DrawTexture(positionRect, Content.InfoIconSmall);
                        positionRect.x += positionRect.width;
                        positionRect.width = 400;
                        GUI.Label(positionRect, string.Format("{0} new information notifications", messages),
                            Styles.NotificationTextStyle);
                    }
                }
                GUILayout.EndHorizontal();
                GUI.color = guiColor;
                return;
            }
            else {
                GUI.color = Color.white;
                if (_time >= 1)
                    try {
                        _scrollPos = GUILayout.BeginScrollView(_scrollPos, false, false,
                            GUILayout.Width(_windowRect.width - 20), GUILayout.Height(_windowRect.height - 20));
                    }
                    catch (InvalidCastException) { }

                foreach (var n in activeNotifications) {
                    var message = n.Message;
                    var size = Styles.BgStyle.CalcSize(new GUIContent(message));
                    GUILayout.Box(message, Styles.BgStyle, GUILayout.MinHeight(Mathf.Max(size.y, 32)));

                    var labelFieldRect = GUILayoutUtility.GetLastRect();
                    GUI.DrawTexture(new Rect(labelFieldRect.x, labelFieldRect.y, 32, 32),
                        Content.GetIcon(n.Type, true));
                    GUILayout.Space(10);
                    var width = 15;
                    if (GUI.Button(
                        new Rect(labelFieldRect.x + labelFieldRect.width - width, labelFieldRect.y, width, width), "X",
                        Styles.NotificationCloseStyle))
                        _notificationInfo.IgnoreMessage(n.Id);
                }
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Restore all notifications",
                    Styles.MenuOptionsStyle, GUILayout.MaxWidth(200)))
                    SKSGlobalRenderSettings.IgnoredNotifications = new List<int>();
#if SKS_DEV
                /*
                if (GUILayout.Button("Re-scan server for notifications",
                    Styles.menuOptionsStyle, GUILayout.MaxWidth(200)))
                    AutoOpen();*/
#endif
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();

            if (activeNotifications.Count != 0 && !SKSGlobalRenderSettings.Minimized && _time >= 1)
                GUILayout.EndScrollView();


            GUI.color = guiColor;
            //GUILayout.FlexibleSpace();

            EditorGUILayout.EndVertical();
        }

        private static void GetNotifications()
        {
            var clientInfo = JsonUtility.ToJson(new ClientInfo());
            var request = WebRequest.Create(ConnectInfo.UpdateUrl);
            request.Method = WebRequestMethods.Http.Post;
            try {
                var encodedData = Encoding.ASCII.GetBytes(clientInfo.ToCharArray());
                request.ContentLength = encodedData.Length;
                request.ContentType = "application/json";
                var dataStream = request.GetRequestStream();
                dataStream.Write(encodedData, 0, encodedData.Length);
                dataStream.Close();
                using (var response = request.GetResponse()) {
                    var stream = response.GetResponseStream();
                    if (stream == null) return;
                    using (var sr = new StreamReader(stream)) {
                        var notificationString = sr.ReadToEnd();
                        _notificationString = notificationString;


                        EditorApplication.update -= SetNotifications;
                        EditorApplication.update += SetNotifications;
                    }
                }
            }

        catch (Exception) {  
                //silent failures, non-essential
            }
        }

        private static void SetNotifications()
        {
            EditorApplication.update -= SetNotifications;
            _notificationInfo = (NotificationInfo) JsonUtility.FromJson(_notificationString, typeof(NotificationInfo));
            if (_notificationInfo.GetActiveNotifications().Count > 0)
                MenuEnable();
        }

        internal static class ConnectInfo {
            public static string UpdateUrl = "http://34.211.133.6:3010/";
        }

        internal static class Content {
            public static Texture ErrorIcon = EditorGUIUtility.Load("icons/console.erroricon.png") as Texture2D;

            public static Texture ErrorIconSmall =
                EditorGUIUtility.Load("icons/console.erroricon.sml.png") as Texture2D;

            public static Texture WarnIcon = EditorGUIUtility.Load("icons/console.warnicon.png") as Texture2D;
            public static Texture WarnIconSmall = EditorGUIUtility.Load("icons/console.warnicon.sml.png") as Texture2D;
            public static Texture InfoIcon = EditorGUIUtility.Load("icons/console.infoicon.png") as Texture2D;
            public static Texture InfoIconSmall = EditorGUIUtility.Load("icons/console.infoicon.sml.png") as Texture2D;

            public static Texture GetIcon(int id, bool large)
            {
                switch (id) {
                    case 1:
                        return large ? InfoIcon : InfoIconSmall;
                    case 2:
                        return large ? WarnIcon : WarnIconSmall;
                    case 3:
                        return large ? ErrorIcon : ErrorIconSmall;
                    default:
                        goto case 1;
                }
            }
        }
        //private static readonly float starSizeFull = starSize * maxRating;

        internal static class Styles {
            private static bool _initialized;

            public static GUIStyle BgStyle;

            public static Material BlurMat;
            public static Material BorderMat;

            //public static Texture2D borderTex;
            public static Texture TexturePlaceholder;

            public static GUIStyle WindowStyle;
            public static GUIStyle ColoredFoldout;
            public static GUIStyle MenuOptionsStyle;
            public static GUIStyle NotificationCloseStyle;
            public static GUIStyle NotificationTextStyle;

            public static void Init()
            {
                if (_initialized) return;
                _initialized = true;

                BlurMat = Resources.Load<Material>("UI/blur");
                BorderMat = Resources.Load<Material>("UI/BorderMat");
                _initialized = true;

                WindowStyle = new GUIStyle(GUI.skin.window) {
                    normal = {background = null},
                    border = new RectOffset(4, 4, 4, 4),
                    margin = new RectOffset(0, 0, 0, 0),
                    richText = true
                };

                TexturePlaceholder = new Texture2D(1, 1);

                if (EditorStyles.foldout != null)
                    ColoredFoldout = new GUIStyle(EditorStyles.foldout);
                else
                    ColoredFoldout = new GUIStyle();

                ColoredFoldout.normal.textColor = Color.white;
                ColoredFoldout.hover.textColor = Color.white;
                ColoredFoldout.active.textColor = Color.white;
                ColoredFoldout.focused.textColor = Color.white;
                ColoredFoldout.active.textColor = Color.white;
                ColoredFoldout.onActive.textColor = Color.white;
                ColoredFoldout.onFocused.textColor = Color.white;
                ColoredFoldout.onHover.textColor = Color.white;
                ColoredFoldout.onNormal.textColor = Color.white;

                if (_skin.button != null)
                    MenuOptionsStyle = new GUIStyle(_skin.button);
                else
                    MenuOptionsStyle = new GUIStyle();

                MenuOptionsStyle.fontSize = 10;
                MenuOptionsStyle.fontStyle = FontStyle.Bold;
                MenuOptionsStyle.wordWrap = false;
                MenuOptionsStyle.clipping = TextClipping.Overflow;
                MenuOptionsStyle.margin = new RectOffset(0, 0, 0, 0);

                NotificationCloseStyle = new GUIStyle(MenuOptionsStyle);
                NotificationCloseStyle.normal.background = null;
                NotificationCloseStyle.margin = new RectOffset(0, 0, 0, 0);

                NotificationTextStyle = new GUIStyle(GUI.skin.label);
                NotificationTextStyle.margin = new RectOffset(0, 0, 0, 0);

                var proBg = GlobalStyles.LoadImageResource("pkpro_selector_bg_pro");
                var defaultBg = GlobalStyles.LoadImageResource("pkpro_selector_bg");

                BgStyle = new GUIStyle();
                BgStyle.normal.background = EditorGUIUtility.isProSkin ? proBg : defaultBg;
                BgStyle.border = new RectOffset(2, 2, 2, 2);
                BgStyle.padding = new RectOffset(32, 5, 5, 5);
                BgStyle.richText = true;
                BgStyle.normal.textColor = Color.white;
                BgStyle.wordWrap = true;
            }
        }

        [Serializable]
        internal class ClientInfo {
            public string Version = string.Format("{0}.{1}.{2}", GlobalPortalSettings.MajorVersion,
                GlobalPortalSettings.MinorVersion, GlobalPortalSettings.PatchVersion);
        }


        [Serializable]
        internal class NotificationInfo {
            public List<Notification> Notifications;

            public NotificationInfo()
            {
                Notifications = new List<Notification>();
            }

            public void IgnoreMessage(int id)
            {
                SKSGlobalRenderSettings.IgnoredNotifications.Add(id);
            }

            public List<Notification> GetActiveNotifications()
            {
                var returnedList = new List<Notification>();
                foreach (var n in Notifications) {
                    if (SKSGlobalRenderSettings.IgnoredNotifications.Contains(n.Id)) continue;
                    returnedList.Add(n);
                }

                returnedList.Sort();
                return returnedList;
            }


            [Serializable]
            public class Notification : IComparable {
                public int Id;
                public string Message;
                public int Type;

                public Notification(string message, int type, int id)
                {
                    Message = message;
                    Type = type;
                    Id = id;
                }

                public int CompareTo(object obj)
                {
                    if (obj == null) return 1;
                    var other = obj as Notification;
                    if (other != null)
                        return other.Type - Type;
                    throw new ArgumentException("Object is not a Notification");
                }
            }
        }
    }
}