using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectGraphicsSettings : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Settings target framerate.");
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 90;
    }

    void Update()
    {
        if(Application.targetFrameRate != 90)
        {
            Debug.Log("REEEEEEEE");
            Application.targetFrameRate = 90;
        }
    }

}
