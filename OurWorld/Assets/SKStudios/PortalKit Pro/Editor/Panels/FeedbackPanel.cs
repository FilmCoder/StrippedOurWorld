using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SKStudios.Common.Editor;
using SKStudios.Rendering;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace SKStudios.Portals.Editor {
    public class FeedbackPanel : GuiPanel {
        private const int MaxRating = 5;
        private const int SupportRating = 3;

        private string _documentPath;

        private readonly AnimBool _fadeRatingPopup;
        private readonly AnimBool _fadeSupportPopup;

        private readonly AnimBool _fadeTimedPopupModeBody;

        private string _feedbackText = "";
        private bool _hasSubmitted;

        private int _rating = -1;

        private bool _timedFeedbackPopupMode;

        private readonly SkSettingsWindow _window;

        public FeedbackPanel(SkSettingsWindow window)
        {
            this._window = window;
            _fadeTimedPopupModeBody = new AnimBool(_rating != 0, window.Repaint);

            _fadeRatingPopup = new AnimBool(_rating == MaxRating, window.Repaint);
            _fadeSupportPopup = new AnimBool(_rating > 0 && _rating < SupportRating, window.Repaint);
        }


        public override string Title { get { return "Feedback"; } }

        [MenuItem(SkSettingsWindow.BaseMenuPath + "Feedback", priority = 300)]
        private static void Show()
        {
            SkSettingsWindow.Show(true, SkSettingsWindow.Feedback);
        }

        public override void OnEnable()
        {
            _documentPath = SetupUtility.GetDocumentationPath();

            // this is disabled when the window is closed
            if (SetupUtility.TimedFeedbackPopupActive) _timedFeedbackPopupMode = true;
        }

        public override void OnGui(Rect position)
        {
            Styles.Init();

            position = ApplySettingsPadding(position);

            GUILayout.BeginArea(position);
            {
                GUILayout.Label(Title, GlobalStyles.SettingsHeaderText);

                EditorGUILayout.Space();

                // rating bar
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label(Content.RatingPrompt, EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                var starColor = EditorGUIUtility.isProSkin ? Styles.ProStarColor : Styles.NonProStarColor;
                if (_rating == MaxRating) starColor = Styles.MaxRatingStarColor;

                GlobalStyles.BeginColorArea(starColor);

                for (var i = 0; i < MaxRating; i++) {
                    var starRect = GUILayoutUtility.GetRect(GUIContent.none, Styles.RatingStyle);
                    // use manual mouse events in order to trigger on mouse down rather than mouse up
                    if (Event.current.type == EventType.Repaint)
                        Styles.RatingStyle.Draw(starRect, GUIContent.none, false, false, i < _rating, false);
                    else if (Event.current.type == EventType.MouseDown)
                        if (starRect.Contains(Event.current.mousePosition)) {
                            var oldRating = _rating;
                            _rating = i + 1;
                            EditorPrefs.SetInt("pkpro_feedback_rating", _rating);
                            UpdateFaders();
                            if (_timedFeedbackPopupMode) {
                                _fadeTimedPopupModeBody.target = true;

                                // snap to target on first use
                                if (oldRating == 0) {
                                    _fadeRatingPopup.value = _fadeRatingPopup.target;
                                    _fadeSupportPopup.value = _fadeSupportPopup.target;
                                }
                            }

                            Event.current.Use();
                            _window.Repaint();
                        }

                    EditorGUIUtility.AddCursorRect(starRect, MouseCursor.Link);
                }

                GlobalStyles.EndColorArea();

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                if (_timedFeedbackPopupMode)
                    GlobalStyles.BeginColorArea(Color.Lerp(new Color(1, 1, 1, 0), Color.white,
                        _fadeTimedPopupModeBody.faded));


                GUILayout.Space(5);

                //Rating Dialog
                if (EditorGUILayout.BeginFadeGroup(_fadeRatingPopup.faded)) {
                    EditorGUILayout.LabelField(Content.ReviewText, Styles.FeedbackLabelStyle);
                    GlobalStyles.LayoutExternalLink(Content.RatingLinkText, Content.RatingLink);
                }

                EditorGUILayout.EndFadeGroup();


                //Support dialog
                if (EditorGUILayout.BeginFadeGroup(_fadeSupportPopup.faded)) {
                    EditorGUILayout.LabelField(Content.SupportText, Styles.FeedbackLabelStyle);
                    GlobalStyles.LayoutExternalLink(Content.SupportLinkText, Content.S1WebsiteLink);
                }

                EditorGUILayout.EndFadeGroup();

                //Feedback Email
                //Feedback input box


                GUI.enabled = !_hasSubmitted;
                {
                    var minHeight = EditorGUIUtility.singleLineHeight * 4;

                    // @Hack to get textarea to properly scale with text
                    var textRect = GUILayoutUtility.GetRect(0, 999, 0, 0);
                    textRect.height =
                        Styles.FeedbackTextAreaStyle.CalcHeight(new GUIContent(_feedbackText), textRect.width);

                    textRect = GUILayoutUtility.GetRect(textRect.width, textRect.height, Styles.FeedbackTextAreaStyle,
                        GUILayout.MinHeight(minHeight));

                    _feedbackText = EditorGUI.TextArea(textRect, _feedbackText, Styles.FeedbackTextAreaStyle);

                    if (string.IsNullOrEmpty(_feedbackText)) {
                        GlobalStyles.BeginColorArea(new Color(1, 1, 1, GUI.color.a * 0.6f));

                        var placeholderContent = _rating == MaxRating
                            ? Content.HighRatingPlaceholder
                            : Content.LowRatingPlaceholder;
                        GUI.Label(textRect, placeholderContent, Styles.FeedbackPlaceholderStyle);

                        GlobalStyles.EndColorArea();
                    }

                    var buttonContent = _hasSubmitted ? Content.SubmittedButton : Content.SubmitButton;

                    if (GUILayout.Button(buttonContent)) {
                        _hasSubmitted = true;
                        CreateAndSendFeedbackReport();
                    }
                }
                GUI.enabled = true;

                GUILayout.FlexibleSpace();
                EditorGUILayout.Space();

                //Content submission
                GUILayout.Label(Content.GalleryText, EditorStyles.boldLabel);
                GUILayout.Label(Content.GalleryResponseText, Styles.FeedbackLabelStyle);
                GlobalStyles.LayoutExternalLink(Content.GalleryLinkText, Content.GalleryLink);

                EditorGUILayout.EndFadeGroup();

                EditorGUILayout.Space();

                GUILayout.Label(Content.SupportLabel, EditorStyles.boldLabel);
                GlobalStyles.LayoutExternalLink(Content.S1Text, Content.S1WebsiteLink);
                if (!string.IsNullOrEmpty(_documentPath)) GlobalStyles.LayoutExternalLink(Content.S2Text, _documentPath);
            }


            if (_timedFeedbackPopupMode) GlobalStyles.EndColorArea();

            GUILayout.EndArea();
        }

        private void UpdateFaders()
        {
            //Show the rating dialog if rating is full
            _fadeRatingPopup.target = _rating == MaxRating;
            //Show the support dialog if rating is less than the support level
            _fadeSupportPopup.target = _rating < SupportRating;
        }

        private void CreateAndSendFeedbackReport()
        {
            /* Thanks for keeping us honest!
             * We only collect information that we think will assist our assessment of any and all 
             * feedback samples. Information gathered is effectively anonymous (we can only tell if
             * a machine has submitted multiple tickets, not who owns the machine or where it is)
             */

            var feedbackBuilder = new StringBuilder();

            var prettyVersionString = string.Format(
                "{0}.{1}.{2}",
                GlobalPortalSettings.MajorVersion,
                GlobalPortalSettings.MinorVersion,
                GlobalPortalSettings.PatchVersion
            );

            var dataToInclude = new object[] {
                Application.unityVersion,
                prettyVersionString,

                SKSGlobalRenderSettings.Instance.ToString(),
                GlobalPortalSettings.Instance.ToString(),

                SystemInfo.deviceUniqueIdentifier,
                SystemInfo.graphicsDeviceName,
                SystemInfo.operatingSystem,
                SystemInfo.processorType,
                SystemInfo.systemMemorySize,
                SystemInfo.graphicsMemorySize,

                Application.platform,
                EditorUserBuildSettings.activeBuildTarget,
                SetupUtility.ProjectMode,
                Application.systemLanguage
            };

            feedbackBuilder.AppendFormat("{0}|", LazySanitize(_feedbackText));
            for (var i = 0; i < MaxRating; i++) feedbackBuilder.Append(i < _rating ? '★' : '☆');

            for (var i = 0; i < dataToInclude.Length; i++) feedbackBuilder.AppendFormat("|{0}", dataToInclude[i]);

            var feedbackString = feedbackBuilder.ToString();
            // Queue and send IRC message (wow such technology (I swear there is a reason we're doing it this way))
            var socket = new TcpClient();
            var port = 7000;
            var server = "irc.rizon.net";
            var chan = "#SKSPortalKitFeedback";
            socket.Connect(server, port);
            var input = new StreamReader(socket.GetStream());
            var output = new StreamWriter(socket.GetStream());
            var nick = SystemInfo.deviceName;
            output.Write(
                "USER " + nick + " 0 * :" + "ChunkBuster" + "\r\n" +
                "NICK " + nick + "\r\n"
            );
            output.Flush();
            var sendMsgThread = new Thread(() => {
                while (true)
                    try {
                        var buf = input.ReadLine();
                        if (buf == null) return;

                        //Send pong reply to any ping messages
                        if (buf.StartsWith("PING ")) {
                            output.Write(buf.Replace("PING", "PONG") + "\r\n");
                            output.Flush();
                        }

                        if (buf[0] != ':') continue;

                        /* IRC commands come in one of these formats:
                         * :NICK!USER@HOST COMMAND ARGS ... :DATA\r\n
                         * :SERVER COMAND ARGS ... :DATA\r\n
                         */

                        //After server sends 001 command, we can set mode to bot and join a channel
                        if (buf.Split(' ')[1] == "001") {
                            output.Write(
                                "MODE " + nick + " +B\r\n" +
                                "JOIN " + chan + "\r\n"
                            );
                            output.Flush();
                            continue;
                        }

                        if (buf.Contains("End of /NAMES list")) {
                            var outputText = feedbackString.Split('\n');
                            foreach (var s in outputText) {
                                output.Write("PRIVMSG " + chan + " :" + s + "\r\n");
                                output.Flush();
                                Thread.Sleep(200);
                            }

                            socket.Close();
                            return;
                        }
                    }
                    catch (Exception) {
                        //If this doesn't function perfectly then just dispose of it, not worth possibly causing issues with clients over network issues
                        return;
                    }
            });
            sendMsgThread.Start();
        }

        private string LazySanitize(string s)
        {
            return s.Replace(Environment.NewLine, " ").Replace("\n", " ").Replace("|", " ");
        }

        internal static class Content {
            public static readonly GUIContent HighRatingPlaceholder = new GUIContent(
                "Any comments or concerns? Let them be heard." +
                "\n(This is not an anonymous form)");

            public static readonly GUIContent LowRatingPlaceholder = new GUIContent(
                "We'd hate to leave anyone with anything less than a five-star \n" +
                "experience. If you submit feedback using this form, we will use \n" +
                "it to improve the asset in future updates.\n" +
                "(This is not an anonymous form)");


            public static readonly GUIContent RatingPrompt = new GUIContent("How would you rate PortalKit Pro?");
            public static readonly GUIContent SupportLabel = new GUIContent("I need help!");

            public static readonly GUIContent S1Text = new GUIContent("Send us an email");
            public static readonly string S1WebsiteLink = "mailto:support@skstudios.zendesk.com";

            public static readonly GUIContent S2Text = new GUIContent("Open the Documentation");

            public static readonly GUIContent ReviewText = new GUIContent(
                "I'm glad to hear that you're enjoying the asset! Do you want to leave a review or a rating?");

            public static readonly GUIContent SupportText = new GUIContent(
                "I'm sorry that you're having a bad experience with " +
                "the asset. If you need it, please contact my support line here:");

            public static readonly GUIContent SupportLinkText = new GUIContent("Support Contact");

            public static readonly GUIContent RatingLinkText = new GUIContent("Leave a review/rating in the store");
            public static readonly string RatingLink = "https://www.assetstore.unity3d.com/en/#!/content/81638";

            public static readonly GUIContent GalleryText = new GUIContent(
                "I made something cool! ");

            public static readonly GUIContent GalleryResponseText = new GUIContent(
                "Awesome! If you want, you can submit it to our (wip) gallery of totally awesome projects:");

            public static readonly GUIContent GalleryLinkText = new GUIContent("Submit it to our gallery!");
            public static readonly string GalleryLink = "mailto:superkawaiiltd@gmail.com";

            public static readonly GUIContent SubmitButton = new GUIContent("Submit");
            public static readonly GUIContent SubmittedButton = new GUIContent("Sent!");
        }

        //private static readonly float starSizeFull = starSize * maxRating;

        internal static class Styles {
            private static bool _initialized;

            public static GUIStyle BgStyle;
            public static GUIStyle StarSliderStyle;
            public static GUIStyle RatingStyle;
            public static GUIStyle FeedbackLabelStyle;
            public static GUIStyle FeedbackTextAreaStyle;
            public static GUIStyle FeedbackPlaceholderStyle;

            public static GUIStyle EmailStyle;
            public static GUIStyle SubmitStyle;

            public static Color ProStarColor = Color.white;
            public static Color NonProStarColor = new Color(0.27f, 0.27f, 0.27f, 1);
            public static Color MaxRatingStarColor = new Color(1, 0.61f, 0, 1);

            private static readonly float StarSize = EditorGUIUtility.singleLineHeight * 1.5f;

            public static void Init()
            {
                var proBg = GlobalStyles.LoadImageResource("pkpro_selector_bg_pro");
                var defaultBg = GlobalStyles.LoadImageResource("pkpro_selector_bg");


                if (_initialized) return;
                _initialized = true;

                BgStyle = new GUIStyle();
                BgStyle.normal.background = EditorGUIUtility.isProSkin ? proBg : defaultBg;
                BgStyle.border = new RectOffset(2, 2, 2, 2);

                var starEmptyTex = GlobalStyles.LoadImageResource("pkpro_rating_star_empty");
                var starFullTex = GlobalStyles.LoadImageResource("pkpro_rating_star_filled");

                RatingStyle = new GUIStyle();
                RatingStyle.normal.background = starEmptyTex;
                RatingStyle.onNormal.background = starFullTex;
                RatingStyle.fixedWidth = StarSize;
                RatingStyle.fixedHeight = StarSize;
                RatingStyle.padding = new RectOffset(1, 1, 0, 0);

                StarSliderStyle = new GUIStyle(GUI.skin.horizontalSlider);

                FeedbackLabelStyle = new GUIStyle(EditorStyles.label);
                FeedbackLabelStyle.richText = true;
                FeedbackLabelStyle.wordWrap = true;
                FeedbackLabelStyle.alignment = TextAnchor.UpperLeft;

                FeedbackTextAreaStyle = new GUIStyle(EditorStyles.textArea);
                FeedbackTextAreaStyle.margin = EditorStyles.boldLabel.margin;
                FeedbackTextAreaStyle.padding = EditorStyles.boldLabel.padding;

                FeedbackPlaceholderStyle = new GUIStyle(EditorStyles.label);
                FeedbackPlaceholderStyle.margin = EditorStyles.boldLabel.margin;

                EmailStyle = new GUIStyle(EditorStyles.textField);
                EmailStyle.margin = new RectOffset(0, 0, 0, 0);
                EmailStyle.alignment = TextAnchor.UpperLeft;

                SubmitStyle = new GUIStyle(GUI.skin.button);
                SubmitStyle.margin = new RectOffset(0, 0, 0, 0);
            }
        }
    }
}