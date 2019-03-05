using SKStudios.Common.Editor;
using UnityEditor;
using UnityEngine;

namespace SKStudios.Portals.Editor {
    public class AboutPanel : GuiPanel {
        public override string Title { get { return "About"; } }

        [MenuItem(SkSettingsWindow.BaseMenuPath + "About", priority = 400)]
        private static void Show()
        {
            SkSettingsWindow.Show(true, SkSettingsWindow.About);
        }

        public override void OnGui(Rect position)
        {
            Styles.Init();

            position = ApplySettingsPadding(position);

            GUILayout.BeginArea(position);

            GUILayout.FlexibleSpace();

            var img = Content.Logo;
            var imgRect = GUILayoutUtility.GetRect(img.width, img.height);
            GUI.DrawTexture(imgRect, img, ScaleMode.ScaleToFit);

            GUILayout.Space(30);

            GUILayout.Label(Content.AboutVersionText, Styles.AboutVersionStyle);

            GUILayout.Space(20);


            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();


            GUILayout.BeginVertical();
            GUILayout.Label(Content.CreatedByText, Styles.CreatedByTextStyle, GUILayout.Width(160));
            GUILayout.Space(10);
            GUILayout.Label(Content.C1, EditorStyles.boldLabel);
            GlobalStyles.LayoutExternalLink(Content.C1WebsiteText, Content.C1WebsiteLink);
            GlobalStyles.LayoutExternalLink(Content.C1EmailText, Content.C1EmailLink);
            GlobalStyles.LayoutExternalLink(Content.C1TwitterText, Content.C1TwitterLink);
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical();
            GUILayout.Label(Content.CreatedByText2, Styles.CreatedByTextStyle, GUILayout.Width(160));
            GUILayout.Space(10);
            GUILayout.Label(Content.C2, EditorStyles.boldLabel);
            GlobalStyles.LayoutExternalLink(Content.C2TwitterText, Content.C2TwitterLink);
            GlobalStyles.LayoutExternalLink(Content.C2WebsiteText, Content.C2WebsiteLink);
            GUILayout.EndVertical();


            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Space(25);
            GUILayout.Label(Content.DisclaimerText, Styles.DisclaimerStyle);
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            GUILayout.EndArea();
        }

        internal static class Content {
            public static readonly GUIContent AboutVersionText = new GUIContent(
                string.Format("PortalKit PRO v{0}.{1}.{2}", GlobalPortalSettings.MajorVersion,
                    GlobalPortalSettings.MinorVersion, GlobalPortalSettings.PatchVersion));

            public static readonly GUIContent DisclaimerText = new GUIContent(
                "Note: These are <i>not</i> support emails.\nIf you need help, please see the 'Feedback' tab.");

            public static readonly GUIContent CreatedByText = new GUIContent("Made with <color=#ad164b>❤</color> by:");
            public static readonly GUIContent CreatedByText2 = new GUIContent("Editor by:");
            public static readonly GUIContent C1 = new GUIContent("SKStudios");
            public static readonly GUIContent C2 = new GUIContent("Madgvox");

            public static readonly GUIContent C1WebsiteText = new GUIContent("skstudios.io");
            public static readonly GUIContent C1EmailText = new GUIContent("superkawaiiltd@gmail.com");
            public static readonly GUIContent C1TwitterText = new GUIContent("@studios_sk");

            public static readonly string C1WebsiteLink = "http://skstudios.io";
            public static readonly string C1EmailLink = "mailto:superkawaiiltd@gmail.com";
            public static readonly string C1TwitterLink = "https://twitter.com/studios_sk";

            public static readonly GUIContent C2WebsiteText = new GUIContent("madgvox.com");
            public static readonly GUIContent C2TwitterText = new GUIContent("@madgvox");

            public static readonly string C2WebsiteLink = "https://madgvox.com/";
            public static readonly string C2TwitterLink = "https://twitter.com/madgvox";

            public static readonly Texture2D Logo = GlobalStyles.LoadImageResource("pkpro_about_logo");
        }


        internal static class Styles {
            private static bool _initialized;

            public static GUIStyle CreatedByTextStyle;
            public static GUIStyle CreatedByTextStyle2;
            public static GUIStyle AboutVersionStyle;
            public static GUIStyle DisclaimerStyle;

            public static void Init()
            {
                if (_initialized) return;
                _initialized = true;

                AboutVersionStyle = new GUIStyle(EditorStyles.largeLabel);
                AboutVersionStyle.alignment = TextAnchor.MiddleCenter;

                CreatedByTextStyle = new GUIStyle(EditorStyles.label);
                CreatedByTextStyle.richText = true;
                CreatedByTextStyle.alignment = TextAnchor.LowerLeft;
                CreatedByTextStyle.normal.textColor = new Color(0.38f, 0.38f, 0.38f, 1);

                CreatedByTextStyle2 = new GUIStyle(EditorStyles.label);
                CreatedByTextStyle2.richText = true;

                DisclaimerStyle = new GUIStyle(EditorStyles.label);
                DisclaimerStyle.richText = true;
                DisclaimerStyle.alignment = TextAnchor.LowerLeft;
                DisclaimerStyle.normal.textColor = new Color(0.38f, 0.38f, 0.38f, 1);
            }
        }
    }
}