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

using System.Collections.Generic;
using UnityEngine;

namespace SKStudios.Common.Utils {
    /// <summary>
    ///     Class that stores SharedMaterials from renderers so that they only
    ///     have to be retrieved once to prevent gc alloc. Only use for renderers
    ///     that will not be changing materials.
    /// </summary>
    public static class SharedMaterialsCache {
        private static readonly Dictionary<Renderer, Material[]> SharedMaterials;

        static SharedMaterialsCache()
        {
            SharedMaterials = new Dictionary<Renderer, Material[]>();
        }

        /// <summary>
        ///     Get the shared materials of a given renderer.
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Material[] GetSharedMaterials(Renderer r)
        {
            Material[] matArr;
            if (!SharedMaterials.TryGetValue(r, out matArr)) {
                matArr = r.sharedMaterials;
                SharedMaterials[r] = matArr;
            }

            return matArr;
        }
    }
}