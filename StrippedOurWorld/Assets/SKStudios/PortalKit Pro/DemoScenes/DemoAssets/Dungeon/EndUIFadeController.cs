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

using System.Collections;
using SKStudios.Common.Demos;
using SKStudios.Common.Utils;
using UnityEngine;

namespace SKStudios.Portals.Demos {
    public class EndUIFadeController : MonoBehaviour {
        public CanvasGroup BaseGroup;
        public CanvasGroup FinalGroup;

        private void OnTriggerEnter(Collider col)
        {
            var fps = col.GetComponent<BasicFPSExample>();
            if (fps) StartCoroutine(FadeOutIn(fps));
        }

        private IEnumerator FadeOutIn(BasicFPSExample fps)
        {
            yield return new WaitForSeconds(5f);
            var fadeTime = 3f;
            var currentTime = fadeTime;
            while (currentTime > 0) {
                currentTime -= Time.deltaTime;
                BaseGroup.alpha = currentTime / fadeTime;
                yield return WaitCache.Frame;
            }

            BaseGroup.gameObject.SetActive(false);
            yield return new WaitForSeconds(1f);
            FinalGroup.gameObject.SetActive(true);

            currentTime = fadeTime;
            while (currentTime > 0) {
                currentTime -= Time.deltaTime;
                FinalGroup.alpha = 1 - currentTime / fadeTime;
                yield return WaitCache.Frame;
            }

            fps.ForcePause = true;
        }
    }
}