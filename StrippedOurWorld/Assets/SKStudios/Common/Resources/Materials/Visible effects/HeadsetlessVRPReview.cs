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

using SKStudios.Common.Utils;
using UnityEngine;

namespace SKStudios.Common.Demos {
    public class HeadsetlessVrpReview : MonoBehaviour {
        private Camera _cam;

        private bool _eye;
        public Material Mat;

        private void Start()
        {
            _cam = GetComponent<Camera>();
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            var test = Camera.current;
            _eye = !_eye;
            if (_eye)
                Mat.SetTexture(Keywords.ShaderKeys.LeftEye, src);
            else
                Mat.SetTexture(Keywords.ShaderKeys.RightEye, src);
            Graphics.Blit(src, dest, Mat);
        }
    }
}