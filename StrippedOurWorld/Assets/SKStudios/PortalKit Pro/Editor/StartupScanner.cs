using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace SKStudios.Portals.Editor {
    /// <summary>
    ///     Scans for if the project has been properly configured. Opens a StartupPrompt if it has not been.
    /// </summary>
    [InitializeOnLoad]
    public class StartupScanner : ScriptableObject {
        [DidReloadScripts]
        static StartupScanner()
        {
            /*
            TextAsset fileContents = Resources.Load<TextAsset>("VersionInfo");
            StartupPrompt.fileContents = fileContents.ToString().Split(System.Environment.NewLine.ToCharArray());*/
            //if (StartupPrompt.fileContents[0] == "0") {
            var path = new StackTrace(true).GetFrame(0).GetFileName();
            if (path == null)
                return;

            path = path.Substring(0, path.LastIndexOf(Path.DirectorySeparatorChar));
            var versionInfoPath = /* path + */ @"VersionInfo.txt";
            if (!File.Exists(versionInfoPath)) {
                //        SettingsWindow.Show();
            }

            //AssetDatabase.importPackageCompleted += RequireComponentDetector.DelayedScan;
        }

        [DidReloadScripts]
        private static void DidScriptsReload()
        {
            //AssetDatabase.importPackageCompleted += RequireComponentDetector.DelayedScan;
        }
    }
}