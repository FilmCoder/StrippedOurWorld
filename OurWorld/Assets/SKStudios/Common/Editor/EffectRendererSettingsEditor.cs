using UnityEditor;
using UnityEngine;

namespace SKStudios.Common.Editor {
    /// <summary>
    ///     Custom editor script for <see cref="EffectRendererSettings" />.
    /// </summary>
    [CustomEditor(typeof(EffectRendererSettings))]
    public class EffectRendererSettingsEditor : UnityEditor.Editor {
        private Texture2D _icon;

        private Texture2D Icon {
            get {
                if (_icon == null) {
                    var image = Resources.Load<Texture2D>("UI/EffectRendererIcon");
                    if (image == null) return null;
                    _icon = new Texture2D(image.width, image.height, TextureFormat.ARGB32, true);
                    Graphics.CopyTexture(image, _icon);
                    _icon.Apply();
                }

                return _icon;
            }
        }

        /// <summary>
        ///     Draws the custom preview thumbnail for the asset in the Project window
        /// </summary>
        /// <param name="assetPath">Path of the asset</param>
        /// <param name="subAssets">Array of children assets</param>
        /// <param name="width">Width of the rendered thumbnail</param>
        /// <param name="height">Height of the rendered thumbnail</param>
        /// <returns></returns>
        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            return Icon;
        }
    }
}