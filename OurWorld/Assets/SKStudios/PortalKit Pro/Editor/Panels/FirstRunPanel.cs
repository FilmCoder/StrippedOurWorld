using SKStudios.Common.Editor;
using UnityEngine;

namespace SKStudios.Portals.Editor {
    public class FirstRunPanel : GuiPanel {
        private readonly ProjectModeSelector _selector;

        public FirstRunPanel(SkSettingsWindow window)
        {
            _selector = new ProjectModeSelector(window);
        }

        public override string Title { get { return "Welcome"; } }

        public override void OnGui(Rect position)
        {
            Styles.Init();

            Rect headerRect;

            var halfWidth = position.width * 0.5f;
            float runningHeight = 0;

            float headerHeight = 200;
            var splashImg = Content.SplashImg;

            if (splashImg != null) {
                // image splash
                headerRect = new Rect(position.width * 0.5f - splashImg.width * 0.5f, 0, splashImg.width, headerHeight);
                GUI.Box(new Rect(0, 0, position.width, headerHeight), GUIContent.none, Styles.FallbackHeaderStyle);
                GUI.DrawTexture(headerRect, splashImg, ScaleMode.ScaleToFit);
            }
            else {
                // text splash
                headerRect = new Rect(0, 0, position.width, headerHeight);
                GUI.Label(headerRect, Content.HeaderText, Styles.FallbackHeaderStyle);
            }

            runningHeight += headerRect.height;

            var textWidth = Mathf.Min(position.width * 0.6f, 500);
            var welcomeTextRect = new Rect(halfWidth - textWidth * 0.5f, headerRect.height, textWidth,
                GlobalStyles.WelcomeTextStyle.CalcHeight(Content.WelcomeText, textWidth));
            GUI.Label(welcomeTextRect, Content.WelcomeText, GlobalStyles.WelcomeTextStyle);

            runningHeight += welcomeTextRect.height;

            var selectionBounds = new Rect(0, runningHeight, position.width, 260);

            _selector.Draw(selectionBounds);
        }

        internal static class Content {
            public static readonly Texture2D SplashImg = GlobalStyles.LoadImageResource("pkpro_splash");

            public static readonly GUIContent HeaderText =
                new GUIContent("Welcome to PortalKit <color=#ff9c00><b>PRO</b></color>!");

            public static readonly GUIContent WelcomeText = new GUIContent(
                "Thank you for supporting <b>PortalKit <color=#ff9c00>PRO</color></b>! We'll have you up and running in no time! Simply choose what type of project you're using this asset with below:");
        }

        internal static class Styles {
            private static bool _initialized;

            public static GUIStyle FallbackHeaderStyle;

            public static void Init()
            {
                if (_initialized) return;
                _initialized = true;

                FallbackHeaderStyle = new GUIStyle();
                FallbackHeaderStyle.alignment = TextAnchor.MiddleCenter;
                FallbackHeaderStyle.fontSize = 30;
                FallbackHeaderStyle.richText = true;

                var bgTex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                bgTex.SetPixel(0, 0, new Color(14 / 255f, 3 / 255f, 1 / 255f));
                bgTex.Apply();

                FallbackHeaderStyle.normal.background = bgTex;
                FallbackHeaderStyle.normal.textColor = Color.white;
            }
        }
    }
}