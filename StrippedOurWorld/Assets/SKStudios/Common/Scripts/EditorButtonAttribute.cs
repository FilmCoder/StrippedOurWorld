// © 2018, SKStudios LLC. All Rights Reserved.
// 
// The software, artwork and data, both as individual files and as a complete software package known as 'PortalKit Pro', or 'MirrorKit Pro'
// without regard to source or channel of acquisition, are bound by the terms and conditions set forth in the Unity Asset 
// Store license agreement in addition to the following terms;
// 
// One license per seat is required for Companies, teams, studios or collaborations using PortalKit Pro and/or MirrorKit Pro that have over 
// 10 members or that make more than $10,000 USD per year. 
// 
// Addendum;
// If PortalKit Pro or MirrorKit pro constitute a major portion of your game's mechanics, please consider crediting the software and/or SKStudios.
// You are in no way obligated to do so, but it would be sincerely appreciated.

using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SKStudios.Common.Editor {
    /// <summary>
    ///     Stick this on a method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class EditorButtonAttribute : PropertyAttribute {
        //empty
    }

#if UNITY_EDITOR
    /// <summary>
    ///     Adds an editor button attribute
    ///     Initial Concept by http://www.reddit.com/user/zaikman
    ///     Revised by http://www.reddit.com/user/quarkism
    ///     This file was made by the credited authors and is NOT a part of any SKStudios product.
    ///     As a result, you are free to use it as you please.
    /// </summary>
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class EditorButton : UnityEditor.Editor {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var mono = target as MonoBehaviour;

            var methods = mono.GetType()
                .GetMembers(BindingFlags.Instance | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                            BindingFlags.NonPublic)
                .Where(o => Attribute.IsDefined(o, typeof(EditorButtonAttribute)));

            foreach (var memberInfo in methods)
                if (GUILayout.Button(memberInfo.Name)) {
                    var method = memberInfo as MethodInfo;
                    method.Invoke(mono, null);
                }
        }
    }
#endif
}