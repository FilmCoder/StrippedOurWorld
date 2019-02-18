#define PKPRO_SHOW_DEBUG

using System;
using System.IO;
using SKStudios.Common.Debug;
using SKStudios.Common.Editor;
using SKStudios.Common.Utils;
using SKStudios.Common.Utils.SafeRemoveComponent;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace SKStudios.Portals.Editor
{
    public enum ProjectImportStatus
    {
        Unknown,
        Uninitialized,
        Initialized
    }

    public enum ProjectMode
    {
        None,
        Default,
        Vr
    }

    public static class SetupUtility
    {

        public const string VrSymbol = "SKS_VR";
        public const string ProjectSymbol = "SKS_PORTALS";
        public const string DemoImportedSymbol = "PORTALKIT_DEMOS_IMPORTED";

        private const string IgnoreSetupPref = "pkpro_dont_show_setup";
        private const string ImportingVrtkPref = "skspro_importing_vrtk";
        private const string PerformingSetupPref = "skspro_performing_import";
        private const string PerformingFirstTimeSetupPref = "skspro_performing_first_time_setup";
        private const string TimedFeedbackPopupActivePref = "pkpro_feedback_popup_triggered";

        private static ProjectMode _projectMode;
        private static ProjectImportStatus _isProjectInitialized;

        public static bool IgnoringInitialSetup {
            get { return EditorPrefs.GetBool(IgnoreSetupPref); }
            set { EditorPrefs.SetBool(IgnoreSetupPref, value); }
        }

        public static bool PerformingSetup {
            get { return EditorPrefs.GetBool(PerformingSetupPref); }
            set { EditorPrefs.SetBool(PerformingSetupPref, value); }
        }

        public static bool PerformingFirstRunSetup {
            get { return EditorPrefs.GetBool(PerformingFirstTimeSetupPref); }
            set { EditorPrefs.SetBool(PerformingFirstTimeSetupPref, value); }
        }

        public static bool TimedFeedbackPopupActive {
            get { return EditorPrefs.GetBool(TimedFeedbackPopupActivePref); }
            set { EditorPrefs.SetBool(TimedFeedbackPopupActivePref, value); }
        }

        // This is an int instead of a bool to account for the two-step reload process due to VRTK adding its own symbol defines and triggering an extra reload after import.
        public static int ImportingVrtk {
            get { return EditorPrefs.GetInt(ImportingVrtkPref); }
            set { EditorPrefs.SetInt(ImportingVrtkPref, value); }
        }

        // This doesn't account for a situation in which the VRTK scripting symbols are defined but the VRTK package isn't present.
        public static bool VrtkIsMaybeInstalled { get { return IsScriptingSymbolDefined("VRTK_VERSION_"); } }

        public static bool ProjectInitialized { get { return ImportStatus == ProjectImportStatus.Initialized; } }

        // We can safely cache this because any changes to the scripting symbols results in a reload of the project.
        public static ProjectImportStatus ImportStatus {
            get {
                if (_isProjectInitialized != ProjectImportStatus.Unknown) return _isProjectInitialized;

                if (IsScriptingSymbolDefined(ProjectSymbol))
                    _isProjectInitialized = ProjectImportStatus.Initialized;
                else
                    _isProjectInitialized = ProjectImportStatus.Uninitialized;

                return _isProjectInitialized;
            }
        }

        // We can safely cache this because any changes to the scripting symbols results in a reload of the project.
        public static ProjectMode ProjectMode {
            get {
                if (_projectMode != ProjectMode.None) return _projectMode;

                if (ImportStatus == ProjectImportStatus.Initialized)
                {
                    if (IsScriptingSymbolDefined(VrSymbol))
                        _projectMode = ProjectMode.Vr;
                    else
                        _projectMode = ProjectMode.Default;
                }
                else
                {
                    _projectMode = ProjectMode.None;
                }

                return _projectMode;
            }
        }

        /// <summary>
        ///     Makes sure the project has been properly configured, and handles properly showing the setup screen on first
        ///     install.
        /// </summary>
        [DidReloadScripts]
        private static void OnProjectReload()
        {
            // don't bother the user if they're playing
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            try
            {
                if (Math.Abs(GlobalPortalSettings.TimeInstalled) < 10000)
                {
                    GlobalPortalSettings.TimeInstalled = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
                    EditorUtility.SetDirty(GlobalPortalSettings.Instance);
                }

            }
            catch (NullReferenceException)
            {
                return;
            }


            if (ProjectInitialized)
            {
                if (ProjectMode == ProjectMode.Vr)
                {
                    // check to see if we're currently trying to load vrtk
                    var importingStep = ImportingVrtk;
                    if (importingStep > 0)
                    {
                        if (importingStep == 2)
                        {
                            // has vrtk been imported successfully?
                            if (VrtkIsMaybeInstalled)
                            {
                                // first reload of vrtk, wait out vrtk adding its own compile symbols
                                ImportingVrtk = 1;
                            }
                            else
                            {
                                // vrtk probably didn't install/run correctly... maybe detect improper import later
                                ImportingVrtk = 0;
                                CheckImportFlags();
                            }
                        }
                        else
                        {
                            // second reload of vrtk, check the import flags
                            ImportingVrtk = 0;
                            CheckImportFlags();
                        }
                    }
                    else
                    {
                        // we're not importing vrtk
                        CheckImportFlags();
                    }
                }
                else
                {
                    CheckImportFlags();
                }

                var timeSinceInstall = (DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond) - GlobalPortalSettings.TimeInstalled;

#if SKS_DEV
                var nagTime = 300000;
#else
                var nagTime = 4.32e+8;
#endif


#if SKS_DEV
                SKLogger.Log("User has " + (GlobalPortalSettings.Nagged ? "been nagged." : ("not been nagged. Asset installed on " +
                             +GlobalPortalSettings.TimeInstalled + " and window will open in " + (nagTime - timeSinceInstall))));
#endif

                if (!GlobalPortalSettings.Nagged)
                {
                    if (timeSinceInstall > nagTime)
                    {
                        GlobalPortalSettings.Nagged = true;
                        TimedFeedbackPopupActive = true;
                        SkSettingsWindow.Show(true, SkSettingsWindow.Feedback);
                        EditorUtility.SetDirty(GlobalPortalSettings.Instance);
                    }
                }
            }
            else
            {
                // project is uninitialized, show the setup window
                if (!IgnoringInitialSetup)
                {
                    EditorUtility.ClearProgressBar();
                    SkSettingsWindow.Show();
                }
            }
        }

        private static void CheckImportFlags()
        {
            if (PerformingSetup)
            {
                PerformingSetup = false;
                Dependencies.RescanDictionary();


                if (PerformingFirstRunSetup)
                {
                    PerformingFirstRunSetup = false;
                    GlobalPortalSettings.TimeInstalled = DateTime.UtcNow.Ticks;
                    EditorUtility.SetDirty(GlobalPortalSettings.Instance);
                    // first time setup is completed
                    SkSettingsWindow.Show(true, SkSettingsWindow.Setup);
                }
                else
                {
                    // setup is completed
                    SkSettingsWindow.Show(true, SkSettingsWindow.Setup);
                }

                EditorUtility.ClearProgressBar();
            }
            else
            {
                //  all done, no need to pop up the window automatically!
                SkSettingsWindow.Hide();
            }
        }

        public static void ImportDemos()
        {
            var demoPath = Directory.GetFiles("Assets", "Demo Scenes.unitypackage", SearchOption.AllDirectories);
            if (demoPath.Length > 0) AssetDatabase.ImportPackage(demoPath[0], false);
            SKSEditorUtil.AddDefine(DemoImportedSymbol,
                GetGroupFromBuildTarget(EditorUserBuildSettings.activeBuildTarget));
        }

        public static void ApplyVr(bool includeVrtk)
        {
            EditorUtility.DisplayProgressBar("Applying VR Presets...", "", 1f);

            if (includeVrtk)
            {
                EditorPrefs.SetInt(ImportingVrtkPref, 2);

                // import VRTK package
                var vrtkPath = Directory.GetFiles("Assets", "vrtk.unitypackage", SearchOption.AllDirectories);
                if (vrtkPath.Length > 0) AssetDatabase.ImportPackage(vrtkPath[0], false);
            }
            else
            {
                // if the user decides to set VR mode without importing VRTK the compile flags can cause errors/
                // detect those errors and let the user know the problem.
                if (!VrtkIsMaybeInstalled)
                    ConsoleCallbackHandler.AddCallback(HandleVrtkImportError, LogType.Error, "CS0246");
            }

            EditorPrefs.SetBool(PerformingSetupPref, true);
            if (!ProjectInitialized) PerformFirstTimeSetup();

            SKSEditorUtil.AddDefine(VrSymbol, GetGroupFromBuildTarget(EditorUserBuildSettings.activeBuildTarget));

            MonoScript cMonoScript = MonoImporter.GetAllRuntimeMonoScripts()[0];
            MonoImporter.SetExecutionOrder(cMonoScript, MonoImporter.GetExecutionOrder(cMonoScript));
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", "PortalKit Pro has been successfully set up for VR! \n" +
                                                   "If you experience any issues, please restart your editor.\n" +
                                                   "If issues persist, please check readme.pdf", "ok");
        }

        private static void HandleVrtkImportError()
        {
            ConsoleCallbackHandler.RemoveCallback(LogType.Error, "CS0246");
            EditorUtility.ClearProgressBar();

            EditorUtility.DisplayDialog("No VRTK Installation Found",
                "No suitable VRTK installation found. VR Portal scripts will not function and may throw errors if VRTK is not present.\n\nIf you have no existing VRTK installation, you should check the 'Also import VRTK' box before applying VR mode.",
                "Okay");

            SKSEditorUtil.RemoveDefine(VrSymbol, GetGroupFromBuildTarget(EditorUserBuildSettings.activeBuildTarget));
            PerformingSetup = false;

            if (PerformingFirstRunSetup)
            {
                SKSEditorUtil.RemoveDefine(ProjectSymbol,
                    GetGroupFromBuildTarget(EditorUserBuildSettings.activeBuildTarget));
                PerformingFirstRunSetup = false;
            }
        }

        public static void ApplyDefault()
        {
            EditorUtility.DisplayProgressBar("Applying Default Presets...", "", 1f);

            EditorPrefs.SetBool(PerformingSetupPref, true);
            if (!ProjectInitialized) PerformFirstTimeSetup();

            SKSEditorUtil.RemoveDefine(VrSymbol, GetGroupFromBuildTarget(EditorUserBuildSettings.activeBuildTarget));
        }

#if SKS_DEV && UNITY_EDITOR
        [MenuItem("Tools/Apply Plugin Flags")]
#endif
        public static void ApplyPluginFlags()
        {
            var basePath = "Assets/SKStudios/Common/Plugins/SKSCoreLibs/";
            ApplyPluginFlags(basePath + "RenderStructs.dll", false);
            ApplyPluginFlags(basePath + "SK_Common.dll", false);
            ApplyPluginFlags(basePath + "SK_CustomRender.dll", false);
            ApplyPluginFlags(basePath + "UnityEditor/RenderStructs.dll", true);
            ApplyPluginFlags(basePath + "UnityEditor/SK_Common.dll", true);
            ApplyPluginFlags(basePath + "UnityEditor/SK_CustomRender.dll", true);
            ApplyPluginFlags(basePath + "UnityEditor/SK_Editor.dll", true);
        }

        private static void PerformFirstTimeSetup()
        {
            SKSEditorUtil.AddDefine(ProjectSymbol, GetGroupFromBuildTarget(EditorUserBuildSettings.activeBuildTarget));
            EditorPrefs.SetBool(PerformingFirstTimeSetupPref, true);
            EditorPrefs.DeleteKey(IgnoreSetupPref);

            //Create the default tags and layers
            GenTagDatabase();

            ReassignPrefabLayers();
            ApplyPluginFlags();
        }

        /// <summary>
        /// Apply platform import flags to plugin. 
        /// </summary>
        /// <param name="pluginLocation">Location of the plugin to flag</param>
        /// <param name="editorOnly">If true, plugin is editor-only. Otherwise, it will be flagged for every platform excluding the editor</param>
        private static void ApplyPluginFlags(string pluginLocation, bool editorOnly)
        {
            //var plugin =  AssetDatabase.LoadAssetAtPath<GlobalPortalSettings>(pluginLocation);
            PluginImporter importer = (PluginImporter)PluginImporter.GetAtPath(pluginLocation);//new PluginImporter();

            importer.ClearSettings();

            if (!editorOnly)
            {
                importer.SetCompatibleWithAnyPlatform(true);
                importer.SetExcludeEditorFromAnyPlatform(true);
                importer.SetCompatibleWithEditor(false);
            }
            else
            {
                importer.SetCompatibleWithAnyPlatform(false);
                importer.SetCompatibleWithEditor(true);
            }


            importer.SaveAndReimport();
        }

        [MenuItem("Tools/Generate Tag Database")]
        private static void GenTagDatabase()
        {
            foreach (var t in Keywords.Tags.AllValues())
                SKTagManager.CreateTag(t);

            foreach (var l in Keywords.Layers.AllCustomLayers)
                SKTagManager.CreateLayer(l);

            SKTagManager.CreateTag("SKSEditorTemp");
            SKTagManager.CreateTag("PhysDupe");
        }

#if SKS_DEV
        [MenuItem("Tools/Reassign prefab layers")]
        public static void ReassignPrefabLayersMenu()
        {
            ReassignPrefabLayers();
        }
#endif
        /// Reshuffles layers created during startup process.
        public static void ReassignPrefabLayers()
        {
            var objects = Resources.LoadAll<GameObject>("");
            foreach (var go in objects)
            {
                if (TagDatabase.Tags.ContainsKey(go.name))
                {
                    if (TagDatabase.Tags[go.name].Count == 0) continue;

                    foreach (var transform in go.transform.GetComponentsInChildren<Transform>())
                        if (TagDatabase.Tags[go.name].ContainsKey(transform.gameObject.name))
                        {
#if SKS_DEV
                            var initialLayer = LayerMask.LayerToName(transform.gameObject.layer);
#endif
                            var layerDict = TagDatabase.Tags[go.name];
                            var layerString = layerDict[transform.gameObject.name];
                            transform.gameObject.layer = LayerMask.NameToLayer(layerString);
#if SKS_DEV
                            Debug.Log(string.Format("Changed parent {0}, child {1}, from layer {2} to layer {3}",
                                go.name, transform.name, initialLayer,
                                LayerMask.LayerToName(transform.gameObject.layer)));
#endif
                            EditorUtility.SetDirty(transform.gameObject);
                        }
                }

                EditorUtility.SetDirty(go);
            }

            AssetDatabase.Refresh();
            // unload unused assets?
        }

        // Allows for fuzzy matching of scripting symbols, eg. a query of "SOME_" will return true for "SOME_SYMBOL"
        public static bool IsScriptingSymbolDefined(string symbolFragment)
        {
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var targetGroup = GetGroupFromBuildTarget(buildTarget);
            var scriptingSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

            return scriptingSymbols.IndexOf(symbolFragment) > -1;
        }

        private static BuildTargetGroup GetGroupFromBuildTarget(BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case BuildTarget.StandaloneOSX:
                    return BuildTargetGroup.Standalone;

                case BuildTarget.StandaloneOSXIntel64:
                    return BuildTargetGroup.Standalone;

                case BuildTarget.StandaloneOSXIntel:
                    return BuildTargetGroup.Standalone;

                case BuildTarget.StandaloneWindows64:
                    return BuildTargetGroup.Standalone;

                case BuildTarget.StandaloneWindows:
                    return BuildTargetGroup.Standalone;

                case BuildTarget.StandaloneLinux64:
                    return BuildTargetGroup.Standalone;

                case BuildTarget.StandaloneLinux:
                    return BuildTargetGroup.Standalone;

                case BuildTarget.StandaloneLinuxUniversal:
                    return BuildTargetGroup.Standalone;

                case BuildTarget.Android:
                    return BuildTargetGroup.Android;

                case BuildTarget.iOS:
                    return BuildTargetGroup.iOS;

                case BuildTarget.WebGL:
                    return BuildTargetGroup.WebGL;

                case BuildTarget.WSAPlayer:
                    return BuildTargetGroup.WSA;

                case BuildTarget.Tizen:
                    return BuildTargetGroup.Tizen;

                case BuildTarget.PSP2:
                    return BuildTargetGroup.PSP2;

                case BuildTarget.PS4:
                    return BuildTargetGroup.PS4;

                case BuildTarget.PSM:
                    return BuildTargetGroup.PSM;

                case BuildTarget.XboxOne:
                    return BuildTargetGroup.XboxOne;

                case BuildTarget.SamsungTV:
                    return BuildTargetGroup.SamsungTV;

                case BuildTarget.N3DS:
                    return BuildTargetGroup.N3DS;

                case BuildTarget.WiiU:
                    return BuildTargetGroup.WiiU;

                case BuildTarget.tvOS:
                    return BuildTargetGroup.tvOS;

                case BuildTarget.NoTarget:
                    return BuildTargetGroup.Unknown;

                default:
                    return BuildTargetGroup.Unknown;
            }
        }

        public static string GetDocumentationPath()
        {
            var path = SkEditorUtils.GetAssetRoot("PortalKit Pro") + "readme.pdf";
            if (File.Exists(path)) return path;
            return string.Empty;
        }


        // This clears all your scripting symbols, not just ones created by the setup process! Only use for debugging purposes!
        public static void DEBUG_ClearSetupData()
        {
            EditorPrefs.DeleteKey(IgnoreSetupPref);
            EditorPrefs.DeleteKey(PerformingSetupPref);
            EditorPrefs.DeleteKey(PerformingFirstTimeSetupPref);

            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var targetGroup = GetGroupFromBuildTarget(buildTarget);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "");
        }
    }
}