#define PKPRO_SHOW_DEBUG
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace SKStudios.Portals.Editor {
    public class ProjectModeSelector {
        private bool _importDemos
#if PORTALKIT_DEMOS_IMPORTED
        = false;
#else
        = true;
#endif
        private bool _needVrtk;

        private readonly AnimBool _showModeTooltip;

        private readonly SkSettingsWindow _window;

        public ProjectModeSelector(SkSettingsWindow window)
        {
            EditorUtility.ClearProgressBar();
            this._window = window;
            _showModeTooltip =
                new AnimBool(SetupUtility.IgnoringInitialSetup || SetupUtility.ProjectMode != ProjectMode.None,
                    window.Repaint);

            _needVrtk = SetupUtility.VrtkIsMaybeInstalled;
        }

        public void DrawLayout() { }

        public void Draw(Rect rect)
        {
            Styles.Init();

            var ignoreButtonRect = new Rect(rect.x + rect.width * 0.5f - rect.width * 0.25f, rect.y + rect.height - 50,
                rect.width * 0.5f, 40);

            var selectedMode = _window.SelectedMode;

            if (SetupUtility.ProjectMode == ProjectMode.None) {
                if (selectedMode == ProjectMode.None) {
                    /*
                    if ( GUI.Button( ignoreButtonRect, Content.ignoreButtonText ) ) {
						if( EditorUtility.DisplayDialog( "PKPro Setup", Content.ignorePopupWarningText, "Ok", "Cancel" ) ) {
							SetupUtility.ignoringInitialSetup = true;
							window.Close();
						}
					}*/
                }
                else {
                    GUI.Box(ignoreButtonRect, Content.IgnoreButtonText, new GUIStyle("button"));
                }
            }

            var tooltipHeight = 60;
            var localRect = new Rect(0, 0, (int) rect.width, (int) rect.height - tooltipHeight);

            var currentTooltipHeight = _showModeTooltip.Fade(0, tooltipHeight);
            rect.height -= tooltipHeight - currentTooltipHeight;
            GUI.BeginGroup(rect);

            var boxRect = localRect;
            boxRect.height += currentTooltipHeight;

            GUI.Box(boxRect, GUIContent.none, Styles.BgStyle);

            var tooltipRect = new Rect();
            tooltipRect.width = Mathf.Clamp(boxRect.width * 0.6f, 380, 500);
            tooltipRect.height = tooltipHeight;
            tooltipRect.x = (int) (boxRect.width * 0.5f - tooltipRect.width * 0.5f);
            tooltipRect.y = boxRect.height - currentTooltipHeight;
            GUI.BeginClip(tooltipRect, Vector2.zero, Vector2.zero, false);

            var tooltipText = selectedMode == ProjectMode.Vr
                ? Content.VrModeTooltipText
                : Content.DefaultModeTooltipText;
            GUI.Label(new Rect(10, 0, tooltipRect.width - 130, tooltipRect.height), tooltipText,
                Styles.ModeTooltipStyle);

            var effectiveTextHeight = Styles.ModeTooltipStyle.CalcHeight(tooltipText, tooltipRect.width - 130);
            if (selectedMode == ProjectMode.Vr) {
                var vrToggleRect = new Rect(10, effectiveTextHeight, tooltipRect.width - 130, 20);
                GUI.enabled = SetupUtility.ProjectMode != selectedMode;
                _needVrtk = GUI.Toggle(vrToggleRect, _needVrtk, Content.ImportVrtkText);
                GUI.enabled = true;
            }

            var demoToggleRect = new Rect(10, effectiveTextHeight + 17, tooltipRect.width - 130, 20);
            _importDemos = GUI.Toggle(demoToggleRect, _importDemos, Content.ImportDemosText);


            var alreadyUsingMode = selectedMode == SetupUtility.ProjectMode;

            var buttonText = GUIContent.none;
            if (alreadyUsingMode) {
                if (selectedMode == ProjectMode.Default) buttonText = Content.UsingDefaultModeText;
                else buttonText = Content.UsingVrModeText;
            }
            else {
                if (selectedMode == ProjectMode.Default) buttonText = Content.ApplyDefaultModeText;
                else buttonText = Content.ApplyVrModeText;
            }

            GUI.enabled = !alreadyUsingMode;
            if (GUI.Button(new Rect(tooltipRect.width - 110, 0, 100, 30), buttonText)) {
                if (selectedMode == ProjectMode.Vr)
                    SetupUtility.ApplyVr(!_needVrtk);
                else
                    SetupUtility.ApplyDefault();
                if (_importDemos)
                    SetupUtility.ImportDemos();
            }

            GUI.enabled = true;
            GUI.EndClip();

            localRect.width = Mathf.Clamp(localRect.width * 0.6f, 380, 500);
            localRect.x = rect.width * 0.5f - localRect.width * 0.5f;

            var buttonHeight = 150;
            var buttonRect = new Rect(localRect.x + 5, localRect.height * 0.5f - buttonHeight * 0.5f,
                localRect.width * 0.5f - 10, buttonHeight);


            DoDefaultButton(buttonRect, selectedMode == ProjectMode.Default);
            buttonRect.x += localRect.width * 0.5f;
            DoVrButton(buttonRect, selectedMode == ProjectMode.Vr);

            GUI.EndGroup();
        }

        private void DoVrButton(Rect buttonRect, bool selected)
        {
            var text = Content.VrModeText;
            if (SetupUtility.ProjectMode == ProjectMode.Vr) text = Content.VrModeActiveText;

            if (ModeButton(buttonRect, text, selected)) {
                _window.SelectedMode = ProjectMode.Vr;
                _showModeTooltip.target = true;
            }
        }

        private void DoDefaultButton(Rect buttonRect, bool selected)
        {
            var text = Content.NormalModeText;
            if (SetupUtility.ProjectMode == ProjectMode.Default) text = Content.NormalModeActiveText;

            if (ModeButton(buttonRect, text, selected)) {
                _window.SelectedMode = ProjectMode.Default;
                _showModeTooltip.target = true;
            }
        }

        private bool ModeButton(Rect buttonRect, GUIContent content, bool selected)
        {
            var textHeight = 40;
            var textRect = new Rect(buttonRect.x, buttonRect.y + buttonRect.height - textHeight, buttonRect.width,
                textHeight);
            var imgRect = new Rect(buttonRect.x + (buttonRect.width * 0.5f - 64),
                buttonRect.y + (buttonRect.height * 0.4f - 64), 128, 128);

            var c = GUI.color;
            GUIStyle textStyle;
            if (selected) {
                GUI.color = Styles.SelectedBorderColor;
                textStyle = Styles.SelectedTextStyle;
            }
            else {
                GUI.color = Styles.DeselectedBorderColor;
                textStyle = Styles.DeselectedTextStyle;
            }

            var hit = GUI.Button(buttonRect, GUIContent.none, Styles.LargeButtonStyle);
            GUI.color = c;

            GUI.Label(textRect, content.text, textStyle);
            GUI.DrawTexture(imgRect, content.image);

            return hit;
        }

        internal static class Content {
            public static readonly GUIContent ApplyVrModeText = new GUIContent("Use VR");
            public static readonly GUIContent UsingVrModeText = new GUIContent("Using VR");
            public static readonly GUIContent ApplyDefaultModeText = new GUIContent("Use Default");
            public static readonly GUIContent UsingDefaultModeText = new GUIContent("Using Default");

            public static readonly GUIContent VrModeTooltipText =
                new GUIContent("Choose <b>VR Mode</b> if you wish to use portals in a VR game.");

            public static readonly GUIContent DefaultModeTooltipText =
                new GUIContent("Choose <b>Default Mode</b> if you wish to use portals for desktop, tablet, or mobile.");

            public static readonly GUIContent ImportVrtkText = new GUIContent("I already have VRTK installed.",
                "VR portals typically require VRTK as a dependency, for ease of movement systems.\nIf you are providing your own version of VRTK or want to write your own VR compatibility, uncheck this to prevent conflicts.");

            public static readonly GUIContent ImportDemosText = new GUIContent("Import Demo Scenes and Assets");
            public static readonly GUIContent IgnoreButtonText = new GUIContent("No thanks, I'll set up PKPro later.");

            public static readonly string IgnorePopupWarningText =
                string.Format(
                    "Warning! PortalKit will *not* function properly until it has been configured!\n\nYou can always return to this setup menu by going to {0}.",
                    SkSettingsWindow.MenuPath.Replace("/", " > "));

            private static readonly Texture2D VrIcon = GlobalStyles.LoadImageResource("pkpro_vr");
            private static readonly Texture2D DefaultIcon = GlobalStyles.LoadImageResource("pkpro_default");

            public static readonly GUIContent VrModeText = new GUIContent("VR", VrIcon);
            public static readonly GUIContent NormalModeText = new GUIContent("Default", DefaultIcon);
            public static readonly GUIContent VrModeActiveText = new GUIContent("VR (Active)", VrIcon);
            public static readonly GUIContent NormalModeActiveText = new GUIContent("Default (Active)", DefaultIcon);
        }

        internal static class Styles {
            private static bool _initialized;

            public static readonly Texture2D BorderImg = GlobalStyles.LoadImageResource("pkpro_border");

            public static GUIStyle ModeTooltipStyle;

            public static GUIStyle SelectedTextStyle;
            public static GUIStyle DeselectedTextStyle;
            public static GUIStyle LargeButtonStyle;
            public static GUIStyle BgStyle;

            public static readonly Color SelectedBorderColor = new Color(1, 0.61f, 0, 1);
            public static readonly Color DeselectedBorderColor = new Color(0.4f, 0.4f, 0.4f, 1);

            public static void Init()
            {
                if (_initialized) return;
                _initialized = true;

                DeselectedTextStyle = new GUIStyle(EditorStyles.label);
                DeselectedTextStyle.fontSize = 13;
                DeselectedTextStyle.richText = false;
                DeselectedTextStyle.alignment = TextAnchor.MiddleCenter;

                SelectedTextStyle = new GUIStyle(DeselectedTextStyle);
                SelectedTextStyle.fontStyle = FontStyle.Bold;

                LargeButtonStyle = new GUIStyle();
                LargeButtonStyle.normal.background = BorderImg;
                LargeButtonStyle.border = new RectOffset(4, 4, 4, 4);

                ModeTooltipStyle = new GUIStyle();
                ModeTooltipStyle.wordWrap = true;
                ModeTooltipStyle.fontSize = 11;
                ModeTooltipStyle.richText = true;
                ModeTooltipStyle.alignment = TextAnchor.UpperLeft;


                var proBg = GlobalStyles.LoadImageResource("pkpro_selector_bg_pro");
                var defaultBg = GlobalStyles.LoadImageResource("pkpro_selector_bg");

                BgStyle = new GUIStyle();
                BgStyle.normal.background = EditorGUIUtility.isProSkin ? proBg : defaultBg;
                BgStyle.border = new RectOffset(2, 2, 2, 2);
            }
        }
    }
}