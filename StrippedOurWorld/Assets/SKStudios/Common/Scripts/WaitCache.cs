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

using UnityEngine;

namespace SKStudios.Common.Utils {
    /// <summary>
    ///     For some reason, Unity coroutine waits generate garbage no matter what. I'm not happy with that.
    /// </summary>
    public static class WaitCache {
        public static readonly WaitForSeconds OneTenthS = new WaitForSeconds(0.1f);
        public static readonly WaitForSeconds OneS = new WaitForSeconds(1f);
        public static readonly WaitForEndOfFrame Frame = new WaitForEndOfFrame();
        public static readonly WaitForFixedUpdate Fixed = new WaitForFixedUpdate();
    }
}