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

using UnityEditor;
using UnityEngine;

namespace SKStudios.Common.Editor {
    using Debug = UnityEngine.Debug;
#if SKS_DEV
    /// <summary>
    ///     Recursively find missing
    /// </summary>
    public class FindMissingScriptsRecursively : EditorWindow {
        private static int _goCount, _componentsCount, _missingCount;

        [MenuItem("Tools/SK Studios/FindMissingScriptsRecursively")]
        public static void ShowWindow()
        {
            GetWindow(typeof(FindMissingScriptsRecursively));
        }

        public void OnGUI()
        {
            if (GUILayout.Button("Find Missing Scripts in selected GameObjects")) FindInSelected();
        }

        private static void FindInSelected()
        {
            var go = Selection.gameObjects;
            _goCount = 0;
            _componentsCount = 0;
            _missingCount = 0;
            foreach (var g in go) FindInGo(g);
            Debug.Log(string.Format("Searched {0} GameObjects, {1} components, found {2} missing", _goCount,
                _componentsCount, _missingCount));
        }

        private static void FindInGo(GameObject g)
        {
            _goCount++;
            var components = g.GetComponents<Component>();
            for (var i = 0; i < components.Length; i++) {
                _componentsCount++;
                if (components[i] == null) {
                    _missingCount++;
                    var s = g.name;
                    var t = g.transform;
                    while (t.parent != null) {
                        s = t.parent.name + "/" + s;
                        t = t.parent;
                    }

                    Debug.Log(s + " has an empty script attached in position: " + i, g);
                }
            }

            // Now recurse through each child GO (if there are any):
            foreach (Transform childT in g.transform)
                //Debug.Log("Searching " + childT.name  + " " );
                FindInGo(childT.gameObject);
        }
    }
#endif
}