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

namespace SKStudios.Portals {
    /// <summary>
    ///     Component that relays physics messages back to the associated <see cref="Portals.Portal" />
    /// </summary>
    public class PortalTrigger : MonoBehaviour {
        /// <summary>
        ///     Trigger segregated from <see cref="Portals.Portal" /> to prevent scaling interaction.
        /// </summary>
        public Portal Portal;

        private void Awake()
        {
            enabled = false;
            gameObject.GetComponent<Collider>().enabled = false;
        }

        /// <summary>
        /// Wrapper for OnTriggerEnter to delegate to the connected <see cref="Portals.Portal" />.
        /// </summary>
        /// <param name="col">Collider</param>
        public void OnTriggerEnter(Collider col)
        {
            if (col.gameObject.isStatic) return;
            if (!col || !Portal)
                return;
            Portal.E_OnTriggerEnter(col);
        }

        /// <summary>
        /// Wrapper for OnTriggerStay to delegate to the connected <see cref="Portals.Portal" />.
        /// </summary>
        /// <param name="col">Collider</param>
        public void OnTriggerStay(Collider col)
        {
            if (col.gameObject.isStatic) return;
            if (!col || !Portal)
                return;
            Portal.E_OnTriggerStay(col);
        }

        /// <summary>
        /// Wrapper for OnTriggerExit to delegate to the connected <see cref="Portals.Portal" />.
        /// </summary>
        /// <param name="col">Collider</param>
        public void OnTriggerExit(Collider col)
        {
            if (col.gameObject.isStatic) return;
            if (!col || !Portal)
                return;
            Portal.E_OnTriggerExit(col);
        }

    }
}