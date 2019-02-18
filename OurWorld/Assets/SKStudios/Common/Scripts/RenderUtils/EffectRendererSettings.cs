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
using System.Text;
using SKStudios.Rendering;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace SKStudios.Common {
    /// <summary>
    ///     ScriptableObject class that contains settings for <see cref="EffectRenderer" />s.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(menuName = "ScriptableObjects/EffectRendererSettings")]
    public class EffectRendererSettings : ScriptableObject {
        private const string NewSettingsLocation = "Assets/SKStudios/Common/Resources";
        private const string NewSettingsDirName = "EffectRendererSettings";
        private const string NewSettingsDefaultName = "_Settings_";

        [SerializeField] private Texture2D _mask;

        [SerializeField] private Material _originalMaterial;

        /// <summary>
        ///     Is the <see cref="EffectRenderer" />  3d, such as a crystal ball or similar effect?
        ///     [Likely to be deprecated, as it has limited use and it appears that nobody has ever, ever actually used it]
        /// </summary>
        public bool Is3D;

        /// <summary>
        ///     Is this <see cref="EffectRenderer" /> rendering as a low resolution effect?
        /// </summary>
        public bool IsLowQualityEffect;

        /// <summary>
        ///     How many extra pixels to draw on all sides of the <see cref="EffectRenderer" /> . Useful for if your
        ///     <see cref="OriginalMaterial" /> heavily
        ///     distorts the image and is warping in undrawn pixels.
        /// </summary>
        public int PixelPadding = 2;

        /// <summary>
        ///     Does the <see cref="EffectRenderer" /> have recursion enabled?
        /// </summary>
        public bool RecursionEnabled = true;

        /// <summary>
        ///     The <see cref="EffectRenderer" /> prefab to use.
        /// </summary>
        public GameObject RendererPrefab;

        /// <summary>
        ///     Does the <see cref="EffectRenderer" /> have rendering enabled?
        /// </summary>
        public bool RenderingEnabled = true;

        /// <summary>
        ///     The <see cref="Material" /> to use for the associated <see cref="EffectRenderer" />
        /// </summary>
        public Material OriginalMaterial {
            get { return _originalMaterial; }
            set {
                if (OnChangeMaterial != null && _originalMaterial != value)
                    OnChangeMaterial.Invoke(value);
                _originalMaterial = value;
            }
        }

        /// <summary>
        ///     The <see cref="Texture2D" />Mask for the associated <see cref="EffectRenderer" />, to control transparency
        ///     per-instance.
        /// </summary>
        public Texture2D Mask {
            get {
                if (SKSGlobalRenderSettings.ShouldOverrideMask)
                    return SKSGlobalRenderSettings.Mask;

                return _mask;
            }
            set {
#if UNITY_EDITOR
                if (value != null)
                    Undo.RecordObject(_mask, "Updating Mask");
#endif
                if (SKSGlobalRenderSettings.ShouldOverrideMask && SKSGlobalRenderSettings.Mask &&
                    SKSGlobalRenderSettings.Mask == value)
                    return;
                _mask = value;
                if (OnChangeMask != null)
                    OnChangeMask.Invoke(_mask);
            }
        }

        public event Action<Material> OnChangeMaterial;
        public event Action<Texture2D> OnChangeMask;

#if UNITY_EDITOR
        /// <summary>
        ///     Create a new <see cref="EffectRendererSettings" /> <see cref="ScriptableObject" /> in the default directory.
        /// </summary>
        /// <param name="name">Name of the new Settings object</param>
        /// <param name="material">Default material to spawn with</param>
        /// <param name="prefab">Default prefab to spawn with</param>
        /// <returns>the newly instantiated Settings object</returns>
        public static EffectRendererSettings CreateNewEffectRendererSettings(string name, Material material,
            GameObject prefab)
        {
            var finalDir = NewSettingsLocation + "/" + NewSettingsDirName;
            if (!AssetDatabase.IsValidFolder(finalDir))
                AssetDatabase.CreateFolder(NewSettingsLocation, NewSettingsDirName);

            finalDir += "/";
            //This is dumb but also realistically how many effect renderers
            //with the exact same name are you gonna have that don't share settings
            var currentNameIndex = 0;
            var fileName = new StringBuilder();

            do {
                fileName.Length = 0;
                fileName.Append(finalDir)
                    .Append(name)
                    .Append(NewSettingsDefaultName)
                    .Append(currentNameIndex++)
                    .Append(".asset");
            } while (AssetDatabase.LoadAssetAtPath<EffectRendererSettings>(fileName.ToString()) != null);

            var newObj = CreateInstance<EffectRendererSettings>();
            newObj.OriginalMaterial = material;
            newObj.RendererPrefab = prefab;
            AssetDatabase.CreateAsset(newObj, fileName.ToString());
            AssetDatabase.Refresh();
            return newObj;
        }
#endif
    }
}