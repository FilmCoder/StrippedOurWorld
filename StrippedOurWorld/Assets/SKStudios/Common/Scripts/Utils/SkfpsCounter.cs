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
using UnityEngine.UI;

namespace SKStudios.Common.Utils {
    /// <summary>
    ///     FPS counter
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class SkfpsCounter : MonoBehaviour {
        private const uint AvgFrames = 100;
        private readonly float[] _timeBuffer = new float[AvgFrames];
        private uint _currentFrame;

        private Text _text;

        private void Start()
        {
            _text = GetComponent<Text>();
        }

        private void Update()
        {
            _timeBuffer[_currentFrame] = Time.deltaTime;
            _currentFrame = (_currentFrame + 1) % AvgFrames;
            float msec = 0;
            for (var i = 0; i < AvgFrames; i++)
                msec += _timeBuffer[i];
            msec /= AvgFrames;
            msec *= 1000.0f;
            var fps = 1000.0f / msec;
            _text.text = string.Format("{0:00.0} ms ({1:000} fps)", msec, fps);
        }
    }
}