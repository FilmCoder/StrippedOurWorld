#define PKPRO_SHOW_DEBUG
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/*
So basically, the requirements for the window are this:
1. It should probably have some sort of Documentation button (although it's not required)
2. Prompt the user to click either VR or NonVR. The VR button automagically imports VRTK and related scripts
3. Prompt the user to hit the "RequireComponent" button there
4. If the user has an existing established project, the "Reassign Layers" button is optional, and it re-assigns the layers used in Prefabs, since during the Import process the asset creates layers in empty slots, but the values are serialized off of ID and not name so they need to be shuffled around
*/

namespace SKStudios.Portals.Editor {
    internal static class GlobalStyles {
        private static bool _initialized;

        public static Color LinkColor = new Color(0.24f, 0.49f, 0.90f, 1);

        public static Texture2D SettingsHeaderImg = LoadImageResource("pkpro_header");

        public static GUIStyle SettingsHeaderText;
        public static GUIStyle WelcomeTextStyle;

        public static GUIStyle LinkStyle;

        private static Stack<Color> _guiColorStack;

        public static void Init()
        {
            if (_initialized) return;
            _initialized = true;

            SettingsHeaderText = new GUIStyle(EditorStyles.largeLabel);
            SettingsHeaderText.alignment = TextAnchor.UpperLeft;
            SettingsHeaderText.fontSize = 18;
            SettingsHeaderText.margin.top = 10;
            SettingsHeaderText.margin.left += 1;

            if (!EditorGUIUtility.isProSkin)
                SettingsHeaderText.normal.textColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            else
                SettingsHeaderText.normal.textColor = new Color(0.7f, 0.7f, 0.7f, 1f);


            WelcomeTextStyle = new GUIStyle(EditorStyles.label);
            WelcomeTextStyle.wordWrap = true;
            WelcomeTextStyle.padding = new RectOffset(30, 30, 30, 30);
            WelcomeTextStyle.fontSize = 12;
            WelcomeTextStyle.richText = true;


            LinkStyle = new GUIStyle(EditorStyles.label);
            LinkStyle.normal.textColor = LinkColor;
            _guiColorStack = new Stack<Color>();
        }

        public static bool LayoutButtonLink(GUIContent content, params GUILayoutOption[] options)
        {
            var controlId =
                GUIUtility.GetControlID(FocusType.Passive) +
                1; // @Hack, predicting the next control id... may not always work.
            var clicked = GUILayout.Button(content, LinkStyle, options);
            var rect = GUILayoutUtility.GetLastRect();

            var widthOffset = 0f;
            var widthContent = content;
            if (content.image != null) {
                widthContent = new GUIContent(content.text);
                widthOffset = EditorGUIUtility.GetIconSize().x;
            }

            float min, max;
            LinkStyle.CalcMinMaxWidth(widthContent, out min, out max);

            var start = new Vector2(rect.x + 2 + widthOffset, rect.y + rect.height - 2);
            var end = new Vector2(rect.x - 2 + min + widthOffset, start.y);

            var color = LinkStyle.normal.textColor;
            if (GUIUtility.hotControl == controlId) color = LinkStyle.active.textColor;

            color.a *= GUI.color.a;

            Handles.BeginGUI();
            var originalColor = Handles.color;
            Handles.color = color;
            Handles.DrawLine(start, end);
            Handles.EndGUI();
            Handles.color = originalColor;

            return clicked;
        }

        public static void LayoutExternalLink(GUIContent content, string url, params GUILayoutOption[] options)
        {
            if (LayoutButtonLink(content, options)) Application.OpenURL(url);
        }

        public static Texture2D LoadImageResource(string filename)
        {
            var imgGuid = AssetDatabase.FindAssets(filename + " t:Texture");

            if (imgGuid.Length > 0) {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(imgGuid[0]));
                return texture;
            }

            return null;
        }


        public static void BeginColorArea(Color color)
        {
            _guiColorStack.Push(GUI.color);
            GUI.color = color;
        }

        public static void EndColorArea()
        {
            GUI.color = _guiColorStack.Pop();
        }
    }
}