using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SKStudios.Common.Debug;
using SKStudios.Common.Editor;
using SKStudios.Portals;
using SKStudios.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using Debug = System.Diagnostics.Debug;
using Object = UnityEngine.Object;

namespace SKStudios.Dev {
#if SKS_DEV_DEPLOY
    /// <summary>
    ///     Class for deploying SKS libs, for internal use
    /// </summary>
    public static class DeployCommon {

        private const string MenuBasePath = "Tools/SK Studios/Deploy/";

        private const string Path = "./Assets/SKStudios/Common/Plugins/SKSCoreLibs/";

        [MenuItem(MenuBasePath + "Common/Build Common Libs/For Debugging")]
        private static void UnobfuscatedDeploy()
        {
            var fullPath = System.IO.Path.GetFullPath(Path);
            BuildSKSLibs(fullPath);
        }


        [MenuItem(MenuBasePath + "Common/Build Common Libs/For Deployment (Also Obfuscates)")]
        public static void ObfuscatedDeploy()
        {
            var fullPath = System.IO.Path.GetFullPath(Path);
            BuildSKSLibs(fullPath);

            //CleanupDebugInfo(Path);
            //CleanupDebugInfo(Path, "UnityEditor");

            ObfuscatorMenu.ObfuscateExternalDll(fullPath);
        }



#if SKS_PORTALS
        private const string PortalKitAssetPath = "Assets/SKStudios/PortalKit Pro";

        private const string PortalKitDefaultSettingsName =
            PortalKitAssetPath + "/Resources/DefaultPortalKitSettings.asset";

        [MenuItem(MenuBasePath + "PortalKit Pro/Set current settings as default")]
        private static void SetPortalKitDefaultSettings()
        {
            AssetDatabase.CreateAsset(Object.Instantiate(GlobalPortalSettings.Instance),
                PortalKitDefaultSettingsName);
        }
#endif

#if SKS_MIRRORS
        private const string MirrorKitAssetPath = "Assets/SKStudios/MirrorKit Pro";

        private const string MirrorKitDefaultSettingsName =
            MirrorKitAssetPath + "/Resources/DefaultPortalKitSettings.asset";
#endif

        private const string SkCommonDefaultSettingsName =
            "Assets/SKStudios/Common/Resources/DefaultSKCommonSettings.asset";

        [MenuItem(MenuBasePath + "Common/Set current settings as default")]
        private static void SetSKSDefaultSettings()
        {
            AssetDatabase.CreateAsset(Object.Instantiate(SKSGlobalRenderSettings.Instance),
                SkCommonDefaultSettingsName);
        }

#if SKS_PORTALS
        private static void ResetPortalSettings()
        {
            GlobalPortalSettings portalSettings = Object.Instantiate(
                AssetDatabase.LoadAssetAtPath<GlobalPortalSettings>(PortalKitDefaultSettingsName));
            portalSettings.NaggedField = false;
            portalSettings.TimeInstalledField = 0;
            var path = AssetDatabase.GetAssetPath(GlobalPortalSettings.Instance);
            Object.DestroyImmediate(GlobalPortalSettings.Instance, true);
            AssetDatabase.CreateAsset(
                Object.Instantiate(
                    AssetDatabase.LoadAssetAtPath<GlobalPortalSettings>(PortalKitDefaultSettingsName)), path);
            EditorUtility.SetDirty(GlobalPortalSettings.Instance);
        }
#endif

        private static void ResetCommonSettings()
        {
            var path = AssetDatabase.GetAssetPath(SKSGlobalRenderSettings.Instance);
            Object.DestroyImmediate(SKSGlobalRenderSettings.Instance, true);
            AssetDatabase.CreateAsset(
                Object.Instantiate(
                    AssetDatabase.LoadAssetAtPath<SKSGlobalRenderSettings>(SkCommonDefaultSettingsName)), path);
            EditorUtility.SetDirty(SKSGlobalRenderSettings.Instance);
            ApplyPluginFlags();
        }


        private static void ApplyPluginFlags()
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

        private static void ApplyPluginFlags(string pluginLocation, bool includedInBuild)
        {
            //var plugin =  AssetDatabase.LoadAssetAtPath<GlobalPortalSettings>(pluginLocation);
            PluginImporter importer = (PluginImporter)PluginImporter.GetAtPath(pluginLocation);//new PluginImporter();

            importer.ClearSettings();

            if (!includedInBuild)
            {
                importer.SetCompatibleWithAnyPlatform(false);
                importer.SetCompatibleWithEditor(false);
            }
            else
            {
                importer.SetCompatibleWithAnyPlatform(true);
                importer.SetCompatibleWithEditor(true);
            }


            importer.SaveAndReimport();
        }

#if SKS_PORTALS
        private const string PortalKitDemoName = "Demo Scenes.unitypackage";
        [MenuItem(MenuBasePath + "PortalKit Pro/Package Demos")]
        private static void PackagePortalDemos()
        {
            var path = PortalKitAssetPath + "/DemoScenes";
            var file = PortalKitAssetPath + "/" + PortalKitDemoName;


            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            ArchiveAsset(path, PortalKitAssetPath, file, "Demos");
        }
#endif

#if SKS_MIRRORS
        private const string MirrorKitDemoName = "Demo Scenes.unitypackage";
        [MenuItem(MenuBasePath + "MirrorKit Pro/Package Demos")]
        private static void PackageMirrorDemos()
        {
            var path = MirrorKitAssetPath + "/DemoScenes";
            var file = MirrorKitAssetPath + "/" + MirrorKitDemoName;


            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            ArchiveAsset(path, MirrorKitAssetPath, file, "Demos");
        }
#endif

        /// <summary>
        /// Archive assets at a given path into a given package. Also moves the original assets
        /// into a new directory beneath Assets/Backup/<paramref name="backupDirName"/>/[EpochTime] as a backup.
        /// </summary>
        /// <param name="path">Location of the assets</param>
        /// <param name="packageName">Name of the package to output</param>
        /// <param name="backupDirName">Name of the subdir for the backup</param>
        /// <returns></returns>
        private static bool ArchiveAsset(String path, String assetPath, String packageName, String backupDirName)
        {
            if (AssetDatabase.LoadAssetAtPath<Object>(path) == null)
            {
                SKLogger.LogWarning(String.Format("Unable to find files to package at directory {0}, using existing package.", path));
                return false;
            }

            AssetDatabase.ExportPackage(path, packageName,
                ExportPackageOptions.Default | ExportPackageOptions.Recurse);

            var saveDir = String.Format("Assets/Backups/{0}", backupDirName);

           // var backupFolder = CreateOrUseDirectory("Assets/Backups");
           // var parentFolder = CreateOrUseDirectory(backupFolder + '/' + backupDirName);
            CreateOrUseDirectory(saveDir);

            string finalPath = saveDir + '/' + DateTime.Now.ToString("MM_dd_yyyy hh_mm");
            AssetDatabase.MoveAsset(path, finalPath);
            //string finalPathHidden = finalPath.Substring(0, finalPath.LastIndexOf('/')) + "/." + new DirectoryInfo(finalPath).Name;
            //Directory.Move(finalPath, finalPathHidden);
            SKLogger.Log(String.Format("Packaged all files in {0} into package {1}. Files backed up to {2}", path, packageName, saveDir));
            AssetDatabase.CreateFolder(assetPath, "DemoScenes");
            return true;
        }

        private static string CreateOrUseDirectory(string dir)
        {
            var dirs = dir.Split('/');
            var currentPath = "Assets";

            for (var i = 1; i < dirs.Length; i++) {
                var d = dirs[i];
                if (String.IsNullOrEmpty(d))
                    continue;
                var newPath = currentPath + '/' + d;
                if (AssetDatabase.IsValidFolder(newPath)) {
                    currentPath = newPath;
                    continue;
                }

                currentPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.CreateFolder(currentPath, d));
            }

            return currentPath;
        }

#if SKS_PORTALS
        /// <summary>
        /// Deploy PKPRO
        /// </summary>
        [MenuItem(MenuBasePath + "Deploy PortalKit Pro", false, 1)]
        public static void DeployPortalKitPro()
        {
            var fullPath = System.IO.Path.GetFullPath(Path);
            CleanupDebugInfo(Path);
            CleanupDebugInfo(Path, "UnityEditor");

            ObfuscatorMenu.ObfuscateExternalDll(fullPath);

            ResetPortalSettings();
            ResetCommonSettings();
            PackagePortalDemos();
        }
#endif

#if SKS_MIRRORS
/// <summary>
/// Deploy MKPRO
/// </summary>
        [MenuItem(MenuBasePath + "Deploy PortalKit Pro", false, 1)]
        public static void DeployPortalKitPro()
        {
            var fullPath = System.IO.Path.GetFullPath(Path);
            CleanupDebugInfo(Path);
            CleanupDebugInfo(Path, "UnityEditor");

            ObfuscatorMenu.ObfuscateExternalDll(fullPath);

            //ResetMirrorSettings();
            ResetCommonSettings();
            PackageMirrorDemos();
        }
#endif

        private static void CleanupDebugInfo(params string[] dirBits)
        {
            var localPath = "";
            foreach (var dirBit in dirBits) localPath = System.IO.Path.Combine(localPath, dirBit);
            var di = new DirectoryInfo(localPath);
            var files = di.GetFiles("*.pdb")
                .Where(p => p.Extension == ".pdb").ToArray();
            foreach (var file in files)
                try {
                    file.Attributes = FileAttributes.Normal;
                    File.Delete(file.FullName);
                }
                catch { }

            files = di.GetFiles("*.mdb")
                .Where(p => p.Extension == ".mdb").ToArray();
            foreach (var file in files)
                try {
                    file.Attributes = FileAttributes.Normal;
                    File.Delete(file.FullName);
                }
                catch { }
        }

         private static void BuildSKSLibs(string fullPath, bool cleanup = true)
        {
            var buildSourcePath = System.IO.Path.Combine(SkEditorUtils.GetAssetRoot("Common"), ".SKS Common Core");
            var commonArgs = "-NoExit msbuild.exe 'SKS Common Core.sln' /m ";

            var startInfo = new ProcessStartInfo("PowerShell")
            {
                WorkingDirectory = buildSourcePath,
                Arguments = commonArgs + "/property:Configuration='Unity Editor'"
            };

            var editorBuild = Process.Start(startInfo);
            startInfo.Arguments = commonArgs + "/property:Configuration='Release'";
            var releaseBuild = Process.Start(startInfo);
            editorBuild.WaitForExit();
            releaseBuild.WaitForExit();
            AssetDatabase.Refresh(ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceSynchronousImport);
        }


}
#endif
    }