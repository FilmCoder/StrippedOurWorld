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

using UnityEngine;

namespace SKStudios.Portals.Demos {
    [ExecuteInEditMode]
    public class UVOffsetSlide : MonoBehaviour {
        private Vector2 _currOffset = Vector2.zero;

        private Material _sharedMat;

        /// <summary>
        ///     Property to change the offset of
        /// </summary>
        public string PropertyName = "_MainTex";

        /// <summary>
        ///     Vector to multiply the offset by, to determine speed and axis
        /// </summary>
        public Vector2 UVOffsetVector = Vector2.one;

        private void Start()
        {
            _sharedMat = GetComponent<Renderer>().sharedMaterial;
        }

        private void Update()
        {
            _sharedMat.SetTextureOffset(PropertyName, _currOffset);
            _currOffset += UVOffsetVector * Time.deltaTime;
            if (_currOffset.x > 1)
                _currOffset.x -= 1;
            if (_currOffset.y > 1)
                _currOffset.y -= 1;
        }
    }
}