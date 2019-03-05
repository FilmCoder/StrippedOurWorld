// © 2018, SKStudios LLC. All Rights Reserved.
// 
// The software, artwork and data, both as individual files and as a complete software package known as 'PortalKit Pro', 
// without regard to source or channel of acquisition, are bound by the terms and conditions set forth in the Unity Asset 
// Store license agreement in addition to the following terms;
// 
// One license per seat is required for Companies, teams, studios or collaborations using PortalKit Pro that have over 
// 10 members or that make more than $50,000 USD per year. 
// 
// Addendum;
// If PortalKitPro constitutes a major portion of your game's mechanics, please consider crediting the software and/or SKStudios.
// You are in no way obligated to do so, but it would be sincerely appreciated.

using System;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace SKStudios.Portals {
    /// <summary>
    ///     Class containing saved settings data specific to the PortalKit Pro asset.
    /// </summary>
    [CreateAssetMenu(menuName = "ScriptableObjects/GlobalPortalSettings")]
    public class GlobalPortalSettings : ScriptableObject {
        public const int MajorVersion = 8;
        public const int MinorVersion = 3;
        public const int PatchVersion = 0;
        private static GlobalPortalSettings _instance;

        [SerializeField] [HideInInspector] public bool NaggedField;

        [SerializeField] [HideInInspector] public double TimeInstalledField;

        /// <summary>
        ///     Returns the singleton instance of the Global Portal Settings
        /// </summary>
        public static GlobalPortalSettings Instance {
            get {
                if (!_instance) {
#if UNITY_EDITOR
                    AssetDatabase.Refresh();
#endif

                    _instance = Resources.Load<GlobalPortalSettings>("Global Portal Settings");
                }

                return _instance;
            }
        }

        /// <summary>
        /// Time the asset was installed
        /// </summary>
        public static double TimeInstalled {
            get { return Instance.TimeInstalledField; }
            set {
                Instance.TimeInstalledField = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
#endif
            }
        }

        /// <summary>
        /// Has the user been asked if they would like to review the asset?
        /// </summary>
        public static bool Nagged {
            get { return Instance.NaggedField; }
            set {
                Instance.NaggedField = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
#endif
            }
        }

        public override string ToString()
        {
            try {
                var builder = new StringBuilder();
                var duration = new TimeSpan((long) (DateTime.UtcNow.Ticks - TimeInstalled));
                var minutes = duration.TotalMinutes;
                builder.Append("SKSRenderSettings:{");
                builder.Append(minutes).Append('|');
                builder.Append(NaggedField);
                builder.Append("}");
                return builder.ToString();
            }
            catch (Exception e) {
                return string.Format("SKSRenderSettings:Error {0}", e.Message);
            }
        }
    }
}