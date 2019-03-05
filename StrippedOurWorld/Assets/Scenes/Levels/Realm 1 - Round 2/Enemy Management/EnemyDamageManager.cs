using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class EnemyDamageManager : MonoBehaviour
{
    private PlayerStatsManager playerStats;
    private VRTK_HeadsetCollision headsetCollision;

    private void Start()
    {
        playerStats = GetComponent<PlayerStatsManager>();
    }

    // Set up manager to receive headset collision events
    private void OnEnable()
    {
        headsetCollision = FindObjectOfType<VRTK_HeadsetCollision>();
        headsetCollision.HeadsetCollisionDetect += new HeadsetCollisionEventHandler(OnHeadsetCollisionDetect);
    }

    private void OnDisable()
    {
        headsetCollision.HeadsetCollisionDetect -= new HeadsetCollisionEventHandler(OnHeadsetCollisionDetect);
    }

    private void OnHeadsetCollisionDetect(object sender, HeadsetCollisionEventArgs e)
    {
        ProcessCollider(e.collider);
    }

    private void ProcessCollider(Collider collider)
    {
        EnemyAttackStats stats = collider.gameObject.GetComponent<EnemyAttackStats>();
        if(stats)
        {
            playerStats.health -= stats.attackPerSecond * Time.deltaTime;
        }
    }
}

