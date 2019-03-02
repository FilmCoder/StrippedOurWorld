using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class TriggerSpawnManager : MonoBehaviour
{
    public SpawnInformation[] spawnInfoArray;

    private VRTK_HeadsetCollision headsetCollision;

    // Set up manager to receive headset collision events
    private void OnEnable()
    {
        headsetCollision = FindObjectOfType<VRTK_HeadsetCollision>();
        headsetCollision.HeadsetCollisionDetect += new HeadsetCollisionEventHandler(OnHeadsetCollisionDetect);
        TriggerSpawnManagerLogic.init(spawnInfoArray);
    }

    private void OnDisable()
    {
        headsetCollision.HeadsetCollisionDetect -= new HeadsetCollisionEventHandler(OnHeadsetCollisionDetect);
    }

    private void OnHeadsetCollisionDetect(object sender, HeadsetCollisionEventArgs e)
    {
        TriggerSpawnManagerLogic.ProcessCollider(e.collider);
    }
}

/**
 * Information for spawning and killing a specific group of enemies/objects.
 * 
 * If the user touches a spawn trigger, all objects in spawnObjects are spawned.
 */

[System.Serializable]
public class SpawnInformation
{
    public string enemyGroupName;
    public GameObject[] spawnTriggers;
    public GameObject[] killTriggers;
    public GameObject[] spawnObjects;

    // Store initial transforms of all the spawn Objects

    [HideInInspector]
    public Vector3[] positions;

    public void SaveSpawnObjectsInitialPositions()
    {
        positions = new Vector3[spawnObjects.Length];
        for(int i = 0; i < positions.Length; i++)
        {
            positions[i] = spawnObjects[i].transform.position;
        }
    }
}

public class TriggerSpawnManagerLogic
{
    private static SpawnInformation[] spawnInformationArray;
    public static void init(SpawnInformation[] _spawnInformationArray)
    {
        Debug.Log("Trigger manager logic init");
        spawnInformationArray = _spawnInformationArray;
        foreach(SpawnInformation info in spawnInformationArray)
        {
            info.SaveSpawnObjectsInitialPositions();
            foreach(GameObject obj in info.spawnObjects)
            {
                obj.SetActive(false);
            }
        }
    }

    public static void ProcessCollider(Collider collider)
    {
        foreach(SpawnInformation info in spawnInformationArray)
        {
            foreach(GameObject spawnTrigger in info.spawnTriggers)
            {
                if (spawnTrigger.GetComponent<Collider>().Equals(collider))
                {
                    if(!info.spawnObjects[0].activeSelf)
                    {
                        for (int i = 0; i < info.spawnObjects.Length; i++)
                        { 
                            info.spawnObjects[i].SetActive(true);
                            info.spawnObjects[i].transform.position = info.positions[i];
                        }
                    }

                }
            }

            foreach (GameObject spawnTrigger in info.killTriggers)
            {
                if (spawnTrigger.GetComponent<Collider>().Equals(collider))
                {
                    if (info.spawnObjects[0].activeSelf)
                    {
                        for (int i = 0; i < info.spawnObjects.Length; i++)
                        {
                            info.spawnObjects[i].SetActive(false);
                        }
                    }
                }
            }
        }
    }
}