using UnityEditor;
using UnityEngine;
using UnityEditor.AnimatedValues;
using SKStudios.Rendering;
using SKStudios.Common.Editor;
using SKStudios.Mirrors.Editor;
using SKStudios.Portals.Editor;

namespace SKStudios.Mirrors.Editor
{

    public abstract class SettingsPanel : GuiPanel
    {
        public override void OnGui(Rect position)
        {
            position = ApplySettingsPadding(position);

            GUILayout.BeginArea(position);

            GUILayout.Label(Title, GlobalStyles.SettingsHeaderText);

            EditorGUIUtility.labelWidth = 250;

            position.x = 0;
            position.y = 0;

            EditorGUILayout.Space();

            DoSettingsGui(position);

            GUILayout.EndArea();
        }

        protected abstract void DoSettingsGui(Rect position);
    }

    public class InteractionSettingsPanel : SettingsPanel
    {
        private SKSGlobalRenderSettingsEditor _globalSettingsEditor;
        [MenuItem(SkSettingsWindow.BaseMenuPath + "Interactions", priority = 210)]
        static void Show()
        {
            SkSettingsWindow.Show(true, SkSettingsWindow.InteractionSettings);
        }

        internal static class Content
        {

        }

        public override string Title {
            get {
                return "Interactions";
            }
        }

        public InteractionSettingsPanel(SkSettingsWindow window)
        {
            _globalSettingsEditor = new SKSGlobalRenderSettingsEditor(window.Repaint);
        }

        protected override void DoSettingsGui(Rect position)
        {
            _globalSettingsEditor.InteractionSettingsGui();
        }
    }

    public class EditorSettingsPanel : SettingsPanel
    {
        private SKSGlobalRenderSettingsEditor _globalSettingsEditor;
        [MenuItem(SkSettingsWindow.BaseMenuPath + "Editor", priority = 220)]
        static void Show()
        {
            SkSettingsWindow.Show(true, SkSettingsWindow.EditorSettings);
        }

        public EditorSettingsPanel(SkSettingsWindow window)
        {
            _globalSettingsEditor = new SKSGlobalRenderSettingsEditor(window.Repaint);

        }

        public override string Title {
            get {
                return "Editor";
            }
        }

        protected override void DoSettingsGui(Rect position)
        {
            Undo.RecordObject(SKSGlobalRenderSettings.Instance, "Update PortalKit Pro Settings");
            _globalSettingsEditor.EditorSettingsGui();
        }
    }

    public class ImageSettingsPanel : SettingsPanel
    {
        [MenuItem(SkSettingsWindow.BaseMenuPath + "General", priority = 200)]
        static void Show()
        {
            SkSettingsWindow.Show(true, SkSettingsWindow.GeneralSettings);
        }

        public override string Title {
            get {
                return "General";
            }
        }

        private SKSGlobalRenderSettingsEditor _globalSettingsEditor;
        public ImageSettingsPanel(SkSettingsWindow window)
        {
            _globalSettingsEditor = new SKSGlobalRenderSettingsEditor(window.Repaint);
        }

        protected override void DoSettingsGui(Rect position)
        {
            Undo.RecordObject(SKSGlobalRenderSettings.Instance, "Update Settings");

            _globalSettingsEditor.RenderingGui();
            EditorGUILayout.Space();
            _globalSettingsEditor.RecursionGui();
        }

    }
}
