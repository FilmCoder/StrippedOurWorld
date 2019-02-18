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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SKStudios.Common.Debug;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

#if UNITY_EDITOR
namespace SKStudios.Common.Editor {
    using Debug = UnityEngine.Debug;
    public class CombineTextures {
        [MenuItem("Assets/SKS/Combine selected into array texture")]
        private static void CreateTexture3D()
        {
            var textures = new List<Texture2D>();

            foreach (var o in Selection.objects)
                if (o.GetType() == typeof(Texture2D))
                    textures.Add((Texture2D) o);

            textures = textures.OrderBy(a => Convert.ToInt32(Path.GetFileNameWithoutExtension(a.name))).ToList();

            var xSize = textures[0].width;
            var ySize = textures[0].height;
            var zSize = textures.Count;

            var zPow2 = (uint) zSize;
            zPow2--;
            zPow2 |= zPow2 >> 1;
            zPow2 |= zPow2 >> 2;
            zPow2 |= zPow2 >> 4;
            zPow2 |= zPow2 >> 8;
            zPow2 |= zPow2 >> 16;
            zPow2++;

            var colorArray = new Color[xSize * ySize];
            var texture = new Texture2DArray(xSize, ySize, (int) zPow2, TextureFormat.RHalf, true);
            for (var z = 0; z < zSize; z++) {
                for (var x = 0; x < xSize; x++)
                for (var y = 0; y < ySize; y++)
                    colorArray[x + y * xSize] = textures[z].GetPixel(x, y);

                texture.SetPixels(colorArray, z);
            }


            texture.Apply();
            AssetDatabase.CreateAsset(texture, AssetDatabase.GetAssetPath(Selection.activeObject) + "3dTexture.asset");
        }

        [MenuItem("Assets/SKS/Split selected array texture")]
        private static void SplitTexture3D()
        {
            var array = Selection.activeObject as Texture2DArray;
            if (array == null) {
                SKLogger.LogWarning("Must have an array texture selected!");
                return;
            }


            var xSize = array.width;
            var ySize = array.height;
            var zSize = array.depth;

            var colorArray = new Color[xSize * ySize];

            //if(!AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(Selection.activeObject) + '/' + Path.GetFileNameWithoutExtension(Selection.activeObject.name)))
            //AssetDatabase.CreateFolder(AssetDatabase.GetAssetPath(Selection.activeObject), Path.GetFileNameWithoutExtension(Selection.activeObject.name));

            for (var z = 0; z < zSize; z++) {
                var texture = new Texture2D(xSize, ySize, TextureFormat.RHalf, true);
                var colorArray3 = array.GetPixels(z);
                for (var x = 0; x < xSize; x++)
                for (var y = 0; y < ySize; y++)
                    colorArray[x + y * xSize] = colorArray3[x + y * xSize];

                texture.SetPixels(colorArray);

                texture.Apply();
                var bytes = texture.EncodeToPNG();
                var path = AssetDatabase.GetAssetPath(Selection.activeObject);
                path = path.Substring(7, path.LastIndexOf('.') - 7);
                File.WriteAllBytes(Application.dataPath + '/' + path + '/' + z + ".png", bytes);
            }
        }
    }
}
#endif